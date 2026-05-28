namespace Hevelab2026.Models
{
    public class OrdenCompra
    {
        public int Id { get; set; }
        public string Referencia { get; set; } // ORD-YYYY-###
        public DateTime FechaConfirmacion { get; set; }
        public string NombreProveedor { get; set; }
        public int ProveedorId { get; set; }
        public string Comprador { get; set; }
        public decimal Total { get; set; }
        public string EstadoFacturacion { get; set; } // "FACTURADO", "PENDIENTE", "PARCIAL"
        public DateTime EntregaEsperada { get; set; }
        public string Estado { get; set; } // "BORRADOR", "CONFIRMADA", "EN_TRANSITO", "RECIBIDA", "CERRADA", "CANCELADA"
        public string Moneda { get; set; } // "SOL", "USD"
    }
}
