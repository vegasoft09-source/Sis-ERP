namespace Sis_ERP.Models
{
    //RrhhModels.cs
    // ── Departamento ──────────────────────────────────────────────────────
    public class HrDepartamento
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
        public int ResponsableId { get; set; }
        public string ResponsableNombre { get; set; } = "";
    }

    // ── Cargo ─────────────────────────────────────────────────────────────
    public class HrCargo
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
    }

    // ── Empleado ──────────────────────────────────────────────────────────
    public class HrEmployee
    {
        public int Id { get; set; }
        public int? UsuarioId { get; set; }
        public int DepartamentoId { get; set; }
        public string DepartamentoNombre { get; set; } = "";
        public int CargoId { get; set; }
        public string CargoNombre { get; set; } = "";
        public string Nombres { get; set; } = "";
        public string Apellidos { get; set; } = "";
        public string NombreCompleto => $"{Nombres} {Apellidos}";
        public string TipoDocumento { get; set; } = "DNI";
        public string NumeroDocumento { get; set; } = "";
        public DateTime FechaIngreso { get; set; }
        public string TipoContrato { get; set; } = "";  // FIJO / TEMPORAL / PRACTICAS
        public string RegimenLaboral { get; set; } = "";
        public bool Activo { get; set; } = true;
    }

    // ── Contrato ──────────────────────────────────────────────────────────
    public class HrContrato
    {
        public int Id { get; set; }
        public int EmpleadoId { get; set; }
        public string EmpleadoNombre { get; set; } = "";
        public string Nombre { get; set; } = "";
        public DateTime FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public decimal Sueldo { get; set; }
        public string Moneda { get; set; } = "PEN";
        public string TipoContrato { get; set; } = "";
        public string Estado { get; set; } = "ACTIVO";  // ACTIVO / CERRADO
    }

    // ── Asistencia ────────────────────────────────────────────────────────
    public class HrAsistencia
    {
        public int Id { get; set; }
        public int EmpleadoId { get; set; }
        public string EmpleadoNombre { get; set; } = "";
        public DateTime Fecha { get; set; }
        public DateTime HoraEntrada { get; set; }
        public DateTime? HoraSalida { get; set; }
        public double? HorasTrabajadas { get; set; }
    }

    // ── Tipo de Ausencia ──────────────────────────────────────────────────
    public class HrTipoAusencia
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
        public bool RequiereAprobacion { get; set; }
        public int DiasMaximos { get; set; }
    }

    // ── Ausencia ──────────────────────────────────────────────────────────
    public class HrAusencia
    {
        public int Id { get; set; }
        public int EmpleadoId { get; set; }
        public string EmpleadoNombre { get; set; } = "";
        public int TipoAusenciaId { get; set; }
        public string TipoAusenciaNombre { get; set; } = "";
        public int AprobadorId { get; set; }
        public string AprobadorNombre { get; set; } = "";
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public int DiasSolicitados { get; set; }
        public string Motivo { get; set; } = "";
        public string Estado { get; set; } = "PENDIENTE"; // PENDIENTE / APROBADA / RECHAZADA
    }
}