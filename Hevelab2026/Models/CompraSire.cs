namespace Hevelab2026.Models
{
    public class CompraSire
    {
        public string Periodo { get; set; } = string.Empty;
        public string TipoDoc { get; set; } = string.Empty;
        public string Serie { get; set; } = string.Empty;
        public string Correlativo { get; set; } = string.Empty;
        public string Fecha { get; set; } = string.Empty;
        public string FechaVenc { get; set; } = string.Empty;
        public string Ruc { get; set; } = string.Empty;
        public string RazonSocial { get; set; } = string.Empty;
        public string Moneda { get; set; } = string.Empty;
        public double TC { get; set; }
        public double BaseGrav { get; set; }
        public double Igv { get; set; }
        public double NoGravado { get; set; }
        public double Total { get; set; }
        public string Anotado { get; set; } = string.Empty;
    }
}
