namespace Hevelab2026.Models
{
    public class CuentaMayor
    {
        public string CodigoCuenta { get; set; } = string.Empty;
        public string NombreCuenta { get; set; } = string.Empty;
        public string TipoCuenta { get; set; } = string.Empty;
        public double SaldoInicial { get; set; }
        public double DebeAnt { get; set; }
        public double HaberAnt { get; set; }
        public double SaldoAnt { get; set; }
    }
}
