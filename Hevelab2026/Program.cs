using System.Text;
using Hevelab2026.Data;
using Hevelab2026.Middleware;
using Hevelab2026.Repositories;
using Hevelab2026.Services.Auth;
using Hevelab2026.Services.Compras;
using Hevelab2026.Services.Inventario;
using Hevelab2026.Services.Productos;
using Hevelab2026.Services.Socios;
using Hevelab2026.Services.Ventas;
using Hevelab2026.Services.Dashboard;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Resolver proveedor: Auto = intenta MySQL, si falla usa InMemory (desarrollo local)
var configuredProvider = builder.Configuration["DatabaseProvider"] ?? "InMemory";
var effectiveProvider = configuredProvider;

if (configuredProvider.Equals("MySql", StringComparison.OrdinalIgnoreCase)
    || configuredProvider.Equals("Auto", StringComparison.OrdinalIgnoreCase))
{
    try
    {
        var cs = MySqlConnectionFactory.BuildConnectionString(builder.Configuration);
        await MySqlConnectionFactory.ValidateConnectionAsync(cs);
        effectiveProvider = "MySql";
        Console.WriteLine("[BD] Conectado a MySQL correctamente.");
    }
    catch (Exception ex)
    {
        if (configuredProvider.Equals("Auto", StringComparison.OrdinalIgnoreCase)
            || builder.Environment.IsDevelopment())
        {
            effectiveProvider = "InMemory";
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("[BD] MySQL no disponible → usando datos en memoria (InMemory).");
            Console.WriteLine($"[BD] Motivo: {ex.Message.Split('\n')[0]}");
            Console.ResetColor();
        }
        else
        {
            throw;
        }
    }
}

builder.Services.AddControllersWithViews(options =>
{
    var policy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    options.Filters.Add(new AuthorizeFilter(policy));
});
builder.Services.AddControllers();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (effectiveProvider.Equals("MySql", StringComparison.OrdinalIgnoreCase))
    {
        var cs = MySqlConnectionFactory.BuildConnectionString(builder.Configuration);
        options.UseMySql(cs, ServerVersion.Parse("8.0.36-mysql"), mySqlOptions =>
        {
            mySqlOptions.EnableRetryOnFailure(3, TimeSpan.FromSeconds(2), null);
        });
    }
    else
    {
        options.UseInMemoryDatabase("HeveLabErp");
    }
});

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IWebAuthService, WebAuthService>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<ISocioService, SocioService>();
builder.Services.AddScoped<IProductoService, ProductoService>();
builder.Services.AddScoped<IPedidoVentaService, PedidoVentaService>();
builder.Services.AddScoped<ICompraService, CompraService>();
builder.Services.AddScoped<IInventarioService, InventarioService>();

var jwtKey = builder.Configuration["Jwt:Key"]!;
builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    })
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Auth/Login";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    })
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                if (string.IsNullOrEmpty(context.Token))
                {
                    context.Token = context.Request.Cookies["hevelab_access_token"];
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "HeveLab ERP API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT: Bearer {token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Database");

    if (effectiveProvider.Equals("InMemory", StringComparison.OrdinalIgnoreCase))
    {
        await db.Database.EnsureCreatedAsync();
        logger.LogInformation("Base de datos InMemory lista.");
    }
    else
    {
        logger.LogInformation(
            "MySQL (Hostinger): se usa el esquema existente en el servidor; no se ejecuta EnsureCreated.");
    }

    if (effectiveProvider.Equals("MySql", StringComparison.OrdinalIgnoreCase))
        await MySqlBootstrap.EnsureAdminAndConnectionAsync(db, logger);
    else
    {
        try
        {
            await DbSeeder.SeedAsync(db);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Seed omitido o parcial (la BD puede tener datos importados).");
        }
    }
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGet("/login", () => Results.Redirect("/Auth/Login"));
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
