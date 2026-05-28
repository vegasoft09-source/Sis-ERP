namespace Hevelab2026.Models
{
    public class SolicitudDeCotizacion
    {
        public int Id { get; set; }
        public string Referencia { get; set; } // RFQ-YYYY-###
        public int ProveedorId { get; set; }
        public string NombreProveedor { get; set; }
        public string Comprador { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime FechaLimite { get; set; }
        public decimal Total { get; set; }
        public string Estado { get; set; } // "NUEVO", "ENVIADA", "COTIZADA", "APROBADA", "RECHAZADA", "VENCIDA"
        public string Moneda { get; set; } // "SOL", "USD"
        public string Tipo { get; set; } // "Compra", "Merma"
    }
}
