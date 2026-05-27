namespace Hevelab2026.Models;

public class OrdenVenta
{
    public string NumeroPedido { get; set; } = "";
    public string FechaCreacion { get; set; } = "";
    public string Cliente { get; set; } = "";
    public string RazonSocial { get; set; } = "";
    public string Ruc { get; set; } = "";
    public decimal Subtotal { get; set; }
    public decimal Total { get; set; }
    public string Estado { get; set; } = "";

    // Nuevos campos
    public string Moneda { get; set; } = "PEN";
    public string CondicionPago { get; set; } = "";
    public string Vendedor { get; set; } = "";
    public decimal Descuento { get; set; }
    public decimal Impuestos { get; set; }
    public string Notas { get; set; } = "";
}