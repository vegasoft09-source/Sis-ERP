using System;

namespace Hevelab2026.Models
{
    public class DocumentoContable
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string TipoDocumento { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public string FechaSubida { get; set; } = string.Empty;
        public int TamanoKB { get; set; }
        public string Extension { get; set; } = string.Empty;
        public string SubidoPor { get; set; } = string.Empty;
        public string[] Etiquetas { get; set; } = Array.Empty<string>();
        public string Estado { get; set; } = string.Empty;
    }
}
