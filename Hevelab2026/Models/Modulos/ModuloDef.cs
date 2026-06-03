namespace Hevelab2026.Models.Modulos
{
    /// <summary>
    /// Definición de un módulo/acceso del sistema. Registrar aquí nuevos módulos.
    /// </summary>
    public class ModuloDef
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Controller { get; set; } = string.Empty;
        public string Action { get; set; } = "Index";
        public string Category { get; set; } = ModuloCategoria.Gestion;
        public string IconSvg { get; set; } = string.Empty;
        public string BadgeText { get; set; } = string.Empty;
        public string BadgeType { get; set; } = "secondary";
        public int SortOrder { get; set; }
        public bool Enabled { get; set; } = true;
    }
}
