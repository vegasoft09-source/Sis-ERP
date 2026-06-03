namespace Hevelab2026.Models.Configuracion
{
    /// <summary>Políticas de sesión — preparado para persistencia futura.</summary>
    public class SeguridadConfigForm
    {
        public int HorasSesion { get; set; } = 8;
        public bool RequiereMayuscula { get; set; } = true;
        public bool RequiereNumero { get; set; } = true;
        public int LongitudMinimaPassword { get; set; } = 8;
        public bool ModuloEnDesarrollo { get; set; } = true;
    }
}
