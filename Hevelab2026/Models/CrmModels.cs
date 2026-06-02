namespace Sis_ERP.Models
{
    //CrmModels.cs
    // ── Etapa del embudo CRM ──────────────────────────────────────────────
    public class CrmEtapa
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
        public int Secuencia { get; set; }
        public int Probabilidad { get; set; }   // 0-100
        public bool EsGanado { get; set; }
        public bool EsPerdido { get; set; }
    }

    // ── Etiqueta (tag) ────────────────────────────────────────────────────
    public class CrmEtiqueta
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
        public string Color { get; set; } = "#6c757d";
    }

    // ── Lead / Oportunidad ────────────────────────────────────────────────
    public class CrmLead
    {
        public int Id { get; set; }
        public string NombreNegocio { get; set; } = "";
        public int EtapaId { get; set; }
        public string EtapaNombre { get; set; } = "";   // solo para mostrar en vista
        public int? SocioId { get; set; }
        public bool EsOportunidad { get; set; }
        public string ContactoNombre { get; set; } = "";
        public string ContactoCorreo { get; set; } = "";
        public string ContactoTelefono { get; set; } = "";
        public int VendedorId { get; set; }
        public string VendedorNombre { get; set; } = "";
        public string Origen { get; set; } = "";        // web, referido, llamada
        public int Prioridad { get; set; }              // 0=baja, 1=media, 2=alta
        public decimal? IngresoEsperado { get; set; }
        public DateTime? FechaCierre { get; set; }
        public string? MotivoPerdida { get; set; }
        public bool Activo { get; set; } = true;
        public int? PedidoVentaId { get; set; }
        public int ProbabilidadEtapa { get; set; }

        // Lista de etiquetas asignadas
        public List<int> EtiquetaIds { get; set; } = new();
    }

    // ── Actividad de seguimiento ──────────────────────────────────────────
    public class CrmActividad
    {
        public int Id { get; set; }
        public int LeadId { get; set; }
        public int UsuarioId { get; set; }
        public string UsuarioNombre { get; set; } = "";
        public string Tipo { get; set; } = "";          // LLAMADA / CORREO / REUNION / TAREA
        public string Titulo { get; set; } = "";
        public string Descripcion { get; set; } = "";
        public DateTime FechaProgramada { get; set; }
        public string Estado { get; set; } = "PENDIENTE"; // PENDIENTE / REALIZADA
        public DateTime? FechaRealizada { get; set; }
    }
}