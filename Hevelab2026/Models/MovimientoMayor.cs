namespace Hevelab2026.Models
{
    public class MovimientoMayor
    {
        public string Cuenta { get; set; } = string.Empty;
        public string Fecha { get; set; } = string.Empty;
        public string Asiento { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public double Debe { get; set; }
        public double Haber { get; set; }
        public double DebeAnt { get; set; }
        public double HaberAnt { get; set; }
    }
}
