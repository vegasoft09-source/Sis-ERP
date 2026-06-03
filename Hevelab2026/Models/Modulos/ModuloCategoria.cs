namespace Hevelab2026.Models.Modulos
{
    /// <summary>
    /// Categorías funcionales del ERP para filtros en dashboard y navegación.
    /// </summary>
    public static class ModuloCategoria
    {
        public const string Todos = "todos";
        public const string Gestion = "gestion";
        public const string Comercial = "comercial";
        public const string Operaciones = "operaciones";
        public const string Finanzas = "finanzas";
        public const string Rrhh = "rrhh";
        public const string Sistema = "sistema";

        public static readonly IReadOnlyList<(string Id, string Label)> Filtros = new[]
        {
            (Todos, "Todos"),
            (Gestion, "Gestión"),
            (Comercial, "Comercial"),
            (Operaciones, "Operaciones"),
            (Finanzas, "Finanzas"),
            (Rrhh, "RRHH"),
            (Sistema, "Sistema")
        };
    }
}
