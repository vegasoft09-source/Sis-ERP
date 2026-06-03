namespace Hevelab2026.Models.Configuracion
{
    public class ConfiguracionIndexViewModel
    {
        public string TabActiva { get; set; } = ConfiguracionTabs.Empresa;
        public EmpresaConfigForm Empresa { get; set; } = new();
        public RegionalConfigForm Regional { get; set; } = new();
        public CorreoConfigForm Correos { get; set; } = new();
        public SeguridadConfigForm Seguridad { get; set; } = new();
        public SistemaEstadoViewModel Estado { get; set; } = new();
        public IReadOnlyList<CatalogoItem> Paises { get; set; } = Array.Empty<CatalogoItem>();
        public IReadOnlyList<CatalogoItem> Departamentos { get; set; } = Array.Empty<CatalogoItem>();
        public IReadOnlyList<CatalogoItem> Monedas { get; set; } = Array.Empty<CatalogoItem>();
    }

    public static class ConfiguracionTabs
    {
        public const string Empresa = "empresa";
        public const string Regional = "regional";
        public const string Correos = "correos";
        public const string Seguridad = "seguridad";

        public static readonly IReadOnlyList<(string Id, string Label, string Icon)> All = new[]
        {
            (Empresa, "Empresa", "building"),
            (Regional, "Regional", "globe"),
            (Correos, "Correos", "mail"),
            (Seguridad, "Seguridad", "shield")
        };

        public static bool EsValida(string? tab) =>
            All.Any(t => t.Id == tab);
    }

    public class SistemaEstadoViewModel
    {
        public string Version { get; set; } = "1.0.0";
        public bool BaseDatosConectada { get; set; }
        public string BaseDatosDetalle { get; set; } = string.Empty;
        public string UltimoRespaldo { get; set; } = "—";
        public string EspacioUsado { get; set; } = "—";
        public int UsuariosActivos { get; set; }
    }
}
