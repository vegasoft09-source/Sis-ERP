namespace Hevelab2026.Models
{
    public class Proveedor
    {
        public int Id { get; set; }
        public string NombreRazonSocial { get; set; }
        public string RUC { get; set; }
        public string Direccion { get; set; }
        public string Telefono { get; set; }
        public string CorreoElectronico { get; set; }
        public string Estado { get; set; } // "ACTIVO", "INACTIVO"
        public DateTime FechaAlta { get; set; }
        public string Categoria { get; set; } // "Manufacturero", "Distribuidor", etc.
    }
}
