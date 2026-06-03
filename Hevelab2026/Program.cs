using Hevelab2026.Services;
using Hevelab2026.Services.Configuracion;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// ═══════════════════════════════════════════════════════════════
// SERVICIOS
// ═══════════════════════════════════════════════════════════════

// MVC con vistas Razor
builder.Services.AddControllersWithViews();

// ── Cookie Authentication (sin ASP.NET Identity completo) ──
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath          = "/Auth/Login";      // redirige aquí si no autenticado
        options.LogoutPath         = "/Auth/Logout";
        options.AccessDeniedPath   = "/Auth/Login";
        options.Cookie.Name        = "HeveLab.Auth";
        options.Cookie.HttpOnly    = true;               // protección XSS
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.SameSite    = SameSiteMode.Lax;
        options.ExpireTimeSpan     = TimeSpan.FromHours(8);
        options.SlidingExpiration  = true;               // renueva la cookie en cada petición
    });

// ── Servicio de usuarios (Dapper + MySQL) ──
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IEmpresaConfigService, EmpresaConfigService>();
builder.Services.AddScoped<IConfiguracionCatalogoService, ConfiguracionCatalogoService>();
builder.Services.AddScoped<ISistemaEstadoService, SistemaEstadoService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();

// ═══════════════════════════════════════════════════════════════
// PIPELINE
// ═══════════════════════════════════════════════════════════════

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();   // ← PRIMERO autenticación
app.UseAuthorization();    // ← DESPUÉS autorización

// Ruta por defecto sigue siendo Home/Index (protegida con [Authorize])
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
