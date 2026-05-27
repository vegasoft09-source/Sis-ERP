using System.Collections.Generic;

namespace Hevelab2026.Models
{
    public class LibroMayorViewModel
    {
        public List<CuentaMayor> Cuentas { get; set; } = new List<CuentaMayor>();
        public List<MovimientoMayor> Movimientos { get; set; } = new List<MovimientoMayor>();
    }
}
