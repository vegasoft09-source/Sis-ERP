namespace Hevelab2026.Models
{
    public class Producto
    {
        public string Sku { get; set; }
        public string CodigoBarras { get; set; }
        public string Nombre { get; set; }

        public decimal Precio { get; set; }
        public int Stock { get; set; }

        public string Unidad { get; set; }
        public string Categoria { get; set; }
        public string Marca { get; set; }

        public DateTime? FechaCaducidad { get; set; }
        public string Lote { get; set; }

        public string Proveedor { get; set; }
        public string Presentacion { get; set; }
        public string PesoVolumen { get; set; }
        public string UnidadMedida { get; set; }

        public string UbicacionAlmacen { get; set; }
        public int StockMinimo { get; set; }

        public bool Disponible { get; set; }

        public string Descripcion { get; set; }
    }
}
