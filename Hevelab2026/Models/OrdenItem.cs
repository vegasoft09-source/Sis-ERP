namespace Hevelab2026.Models
{
    public class OrdenItem
    {
        public int Item { get; set; }
        public string Codigo { get; set; } = "";
        public string Descripcion { get; set; } = "";
        public string SubDescripcion { get; set; } = "";
        public int Stock { get; set; }
        public decimal Cantidad { get; set; }
        public decimal Descuento { get; set; }
        public string Unidad { get; set; } = "UND";
        public decimal PrecioUnitario { get; set; }
        public decimal PrecioIgv { get; set; }
        public decimal Importe { get; set; }
        public string Tipo { get; set; } = "producto";
    }
}
