using Hevelab2026.Data;
using Hevelab2026.Services.Compras;
using Hevelab2026.Services.Productos;
using Hevelab2026.Services.Ventas;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hevelab2026.Controllers;

public class ReportesController : Controller
{
    private readonly IPedidoVentaService _ventas;
    private readonly ICompraService _compras;
    private readonly IProductoService _productos;
    private readonly ApplicationDbContext _db;

    public ReportesController(
        IPedidoVentaService ventas,
        ICompraService compras,
        IProductoService productos,
        ApplicationDbContext db)
    {
        _ventas = ventas;
        _compras = compras;
        _productos = productos;
        _db = db;
    }

    public class RepVenta
    {
        public string Fecha { get; set; } = "";
        public string Vendedor { get; set; } = "";
        public string Cliente { get; set; } = "";
        public decimal Monto { get; set; }
    }

    public class RepCompra
    {
        public string Fecha { get; set; } = "";
        public string Proveedor { get; set; } = "";
        public string Categoria { get; set; } = "";
        public decimal Total { get; set; }
    }

    public class RepInventario
    {
        public string Sku { get; set; } = "";
        public string Producto { get; set; } = "";
        public int Stock { get; set; }
        public decimal ValorUnitario { get; set; }
    }

    public class RepContabilidad
    {
        public string Fecha { get; set; } = "";
        public string Concepto { get; set; } = "";
        public string Tipo { get; set; } = "";
        public decimal Monto { get; set; }
    }

    public class RepEmpleado
    {
        public string Codigo { get; set; } = "";
        public string Nombre { get; set; } = "";
        public string Departamento { get; set; } = "";
        public string Estado { get; set; } = "";
        public decimal Desempeno { get; set; }
    }

    public IActionResult Index() => RedirectToAction(nameof(Ventas));

    public async Task<IActionResult> Ventas(CancellationToken ct)
    {
        ViewBag.ActiveMenu = "Ventas";
        var pedidos = await _ventas.GetCotizacionesAsync(null, null, null, ct);
        var data = pedidos.Select(p => new RepVenta
        {
            Fecha = p.FechaCreacion.ToString("yyyy-MM-dd"),
            Vendedor = p.Vendedor ?? "—",
            Cliente = p.Cliente ?? p.RazonSocial ?? "—",
            Monto = p.Total
        }).ToList();
        return View(data);
    }

    public async Task<IActionResult> Compras(CancellationToken ct)
    {
        ViewBag.ActiveMenu = "Compras";
        var ordenes = await _compras.GetOrdenesCompraAsync(ct);
        var data = ordenes.Select(o => new RepCompra
        {
            Fecha = o.FechaEmision.ToString("yyyy-MM-dd"),
            Proveedor = o.Proveedor,
            Categoria = "Compra",
            Total = o.Total
        }).ToList();
        return View(data);
    }

    public async Task<IActionResult> Inventario(CancellationToken ct)
    {
        ViewBag.ActiveMenu = "Inventario";
        var productos = await _productos.GetAllAsync(null, ct);
        var data = productos.Select(p => new RepInventario
        {
            Sku = p.Sku,
            Producto = p.Nombre,
            Stock = p.Stock,
            ValorUnitario = p.Precio
        }).ToList();
        return View(data);
    }

    public async Task<IActionResult> Contabilidad(CancellationToken ct)
    {
        ViewBag.ActiveMenu = "Contabilidad";
        var pedidos = await _db.PedidosVenta
            .OrderByDescending(p => p.FechaEmision)
            .Take(50)
            .AsNoTracking()
            .ToListAsync(ct);

        var data = pedidos.Select(p => new RepContabilidad
        {
            Fecha = p.FechaEmision.ToString("yyyy-MM-dd"),
            Concepto = $"Pedido {p.NumeroDocumento}",
            Tipo = "Ingreso",
            Monto = p.Total
        }).ToList();
        return View(data);
    }

    public IActionResult Empleados()
    {
        ViewBag.ActiveMenu = "Empleados";
        return View(new List<RepEmpleado>());
    }
}
