namespace Hevelab2026.Models
{
    public class Factura
    {
        public string NumeroFactura { get; set; } = string.Empty;
        public string Fecha { get; set; } = string.Empty;
        public string Cliente { get; set; } = string.Empty;
        public string RUC { get; set; } = string.Empty;
        public string TipoComprobante { get; set; } = string.Empty;
        public string Moneda { get; set; } = string.Empty;
        public double MontoBase { get; set; }
        public double IGV { get; set; }
        public double Total { get; set; }
        public string Estado { get; set; } = string.Empty;
    }
}
