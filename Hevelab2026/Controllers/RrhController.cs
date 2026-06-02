using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Dapper;
using Sis_ERP.Models;

namespace Sis_ERP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RrhhController : Controller
    {
        private readonly string _conn;
        public RrhhController(IConfiguration config)
            => _conn = config.GetConnectionString("DefaultConnection")!;

        // ════════════════════════════════════════════════════════════════
        //  VISTA PRINCIPAL RRHH
        // ════════════════════════════════════════════════════════════════
        
        [HttpGet("/RRHH")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult Index()
        {
            return View("~/Views/RRHH/Index.cshtml");
        }

        // ════════════════════════════════════════════════════════════════
        //  DEPARTAMENTOS
        // ════════════════════════════════════════════════════════════════

        /// <summary>Lista todos los departamentos.</summary>
        [HttpGet("departamentos")]
        public async Task<IActionResult> GetDepartamentos()
        {
            using var db = new MySqlConnection(_conn);
            var data = await db.QueryAsync<HrDepartamento>(
                "SELECT id AS Id, nombre AS Nombre, responsable_id AS ResponsableId, " +
                "responsable_nombre AS ResponsableNombre FROM hr_departamento ORDER BY nombre");
            return Ok(data);
        }

        /// <summary>Crea un nuevo departamento.</summary>
        [HttpPost("departamentos")]
        public async Task<IActionResult> CrearDepartamento([FromBody] HrDepartamento dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Nombre))
                return BadRequest("El nombre del departamento es obligatorio.");

            using var db = new MySqlConnection(_conn);
            var id = await db.ExecuteScalarAsync<int>(
                "INSERT INTO hr_departamento (nombre, responsable_id, responsable_nombre) " +
                "VALUES (@Nombre, @ResponsableId, @ResponsableNombre); SELECT LAST_INSERT_ID();", dto);
            dto.Id = id;
            return Ok(dto);
        }

        // ════════════════════════════════════════════════════════════════
        //  CARGOS
        // ════════════════════════════════════════════════════════════════

        /// <summary>Lista todos los cargos.</summary>
        [HttpGet("cargos")]
        public async Task<IActionResult> GetCargos()
        {
            using var db = new MySqlConnection(_conn);
            var data = await db.QueryAsync<HrCargo>(
                "SELECT id AS Id, nombre AS Nombre FROM hr_cargo ORDER BY nombre");
            return Ok(data);
        }

        /// <summary>Crea un nuevo cargo.</summary>
        [HttpPost("cargos")]
        public async Task<IActionResult> CrearCargo([FromBody] HrCargo dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Nombre))
                return BadRequest("El nombre del cargo es obligatorio.");

            using var db = new MySqlConnection(_conn);
            var id = await db.ExecuteScalarAsync<int>(
                "INSERT INTO hr_cargo (nombre) VALUES (@Nombre); SELECT LAST_INSERT_ID();", dto);
            dto.Id = id;
            return Ok(dto);
        }

        // ════════════════════════════════════════════════════════════════
        //  EMPLEADOS
        // ════════════════════════════════════════════════════════════════

        /// <summary>Lista empleados activos.</summary>
        [HttpGet("empleados")]
        public async Task<IActionResult> GetEmpleados([FromQuery] bool soloActivos = true)
        {
            using var db = new MySqlConnection(_conn);
            var sql = @"
                SELECT e.id AS Id, e.usuario_id AS UsuarioId,
                       e.departamento_id AS DepartamentoId, d.nombre AS DepartamentoNombre,
                       e.cargo_id AS CargoId, c.nombre AS CargoNombre,
                       e.nombres AS Nombres, e.apellidos AS Apellidos,
                       e.tipo_documento AS TipoDocumento, e.numero_documento AS NumeroDocumento,
                       e.fecha_ingreso AS FechaIngreso, e.tipo_contrato AS TipoContrato,
                       e.regimen_laboral AS RegimenLaboral, e.activo AS Activo
                FROM hr_employee e
                INNER JOIN hr_departamento d ON e.departamento_id = d.id
                INNER JOIN hr_cargo c ON e.cargo_id = c.id";
            if (soloActivos) sql += " WHERE e.activo = 1";
            sql += " ORDER BY e.apellidos, e.nombres";

            var data = await db.QueryAsync<HrEmployee>(sql);
            return Ok(data);
        }

        /// <summary>Obtiene un empleado por ID.</summary>
        [HttpGet("empleados/{id}")]
        public async Task<IActionResult> GetEmpleado(int id)
        {
            using var db = new MySqlConnection(_conn);
            var emp = await db.QueryFirstOrDefaultAsync<HrEmployee>(@"
                SELECT e.id AS Id, e.usuario_id AS UsuarioId,
                       e.departamento_id AS DepartamentoId, d.nombre AS DepartamentoNombre,
                       e.cargo_id AS CargoId, c.nombre AS CargoNombre,
                       e.nombres AS Nombres, e.apellidos AS Apellidos,
                       e.tipo_documento AS TipoDocumento, e.numero_documento AS NumeroDocumento,
                       e.fecha_ingreso AS FechaIngreso, e.tipo_contrato AS TipoContrato,
                       e.regimen_laboral AS RegimenLaboral, e.activo AS Activo
                FROM hr_employee e
                INNER JOIN hr_departamento d ON e.departamento_id = d.id
                INNER JOIN hr_cargo c ON e.cargo_id = c.id
                WHERE e.id = @id", new { id });

            if (emp is null) return NotFound("Empleado no encontrado.");
            return Ok(emp);
        }

        /// <summary>
        /// Registra un nuevo empleado.
        /// Regla: si tiene usuario_id, debe existir en tabla usuario.
        /// </summary>
        [HttpPost("empleados")]
        public async Task<IActionResult> CrearEmpleado([FromBody] HrEmployee dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Nombres)) return BadRequest("Los nombres son obligatorios.");
            if (string.IsNullOrWhiteSpace(dto.Apellidos)) return BadRequest("Los apellidos son obligatorios.");
            if (string.IsNullOrWhiteSpace(dto.NumeroDocumento)) return BadRequest("El número de documento es obligatorio.");
            if (dto.DepartamentoId <= 0) return BadRequest("Debe indicar un departamento.");
            if (dto.CargoId <= 0) return BadRequest("Debe indicar un cargo.");

            using var db = new MySqlConnection(_conn);

            // Regla: si viene usuario_id, debe existir en tabla usuario (Grupo 2)
            if (dto.UsuarioId.HasValue)
            {
                var usuarioExiste = await db.ExecuteScalarAsync<int>(
                    "SELECT COUNT(*) FROM usuario WHERE id = @UsuarioId", dto);
                if (usuarioExiste == 0)
                    return BadRequest("El usuario_id indicado no existe en la tabla usuario.");
            }

            // Verificar departamento
            var deptExiste = await db.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM hr_departamento WHERE id = @DepartamentoId", dto);
            if (deptExiste == 0) return BadRequest("El departamento indicado no existe.");

            // Verificar cargo
            var cargoExiste = await db.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM hr_cargo WHERE id = @CargoId", dto);
            if (cargoExiste == 0) return BadRequest("El cargo indicado no existe.");

            var id = await db.ExecuteScalarAsync<int>(@"
                INSERT INTO hr_employee
                    (usuario_id, departamento_id, cargo_id, nombres, apellidos,
                     tipo_documento, numero_documento, fecha_ingreso,
                     tipo_contrato, regimen_laboral, activo)
                VALUES
                    (@UsuarioId, @DepartamentoId, @CargoId, @Nombres, @Apellidos,
                     @TipoDocumento, @NumeroDocumento, @FechaIngreso,
                     @TipoContrato, @RegimenLaboral, 1);
                SELECT LAST_INSERT_ID();", dto);

            dto.Id = id;
            dto.Activo = true;
            return Ok(dto);
        }

        // ════════════════════════════════════════════════════════════════
        //  CONTRATOS
        // ════════════════════════════════════════════════════════════════

        /// <summary>Lista contratos de un empleado.</summary>
        [HttpGet("empleados/{empleadoId}/contratos")]
        public async Task<IActionResult> GetContratos(int empleadoId)
        {
            using var db = new MySqlConnection(_conn);
            var data = await db.QueryAsync<HrContrato>(@"
                SELECT id AS Id, empleado_id AS EmpleadoId, empleado_nombre AS EmpleadoNombre,
                       nombre AS Nombre, fecha_inicio AS FechaInicio, fecha_fin AS FechaFin,
                       sueldo AS Sueldo, moneda AS Moneda, tipo_contrato AS TipoContrato,
                       estado AS Estado
                FROM hr_contrato WHERE empleado_id = @empleadoId ORDER BY fecha_inicio DESC",
                new { empleadoId });
            return Ok(data);
        }

        /// <summary>
        /// Registra un nuevo contrato.
        /// Regla: el empleado solo puede tener UN contrato ACTIVO; el anterior se cierra automáticamente.
        /// </summary>
        [HttpPost("contratos")]
        public async Task<IActionResult> CrearContrato([FromBody] HrContrato dto)
        {
            if (dto.EmpleadoId <= 0) return BadRequest("Debe indicar el empleado.");
            if (string.IsNullOrWhiteSpace(dto.Nombre)) return BadRequest("El nombre del contrato es obligatorio.");
            if (dto.Sueldo <= 0) return BadRequest("El sueldo debe ser mayor a 0.");
            if (dto.FechaInicio == default) return BadRequest("La fecha de inicio es obligatoria.");

            using var db = new MySqlConnection(_conn);

            // Verificar que el empleado existe y está activo
            var empleado = await db.QueryFirstOrDefaultAsync<HrEmployee>(
                "SELECT id AS Id, nombres AS Nombres, apellidos AS Apellidos, activo AS Activo " +
                "FROM hr_employee WHERE id = @EmpleadoId", dto);
            if (empleado is null) return NotFound("Empleado no encontrado.");
            if (!empleado.Activo) return BadRequest("El empleado está inactivo y no puede tener contratos nuevos.");

            using var tx = await db.BeginTransactionAsync();
            try
            {
                // Regla: cerrar contrato activo anterior
                await db.ExecuteAsync(
                    "UPDATE hr_contrato SET estado = 'CERRADO', fecha_fin = @FechaInicio " +
                    "WHERE empleado_id = @EmpleadoId AND estado = 'ACTIVO'",
                    new { dto.FechaInicio, dto.EmpleadoId }, tx);

                dto.EmpleadoNombre = empleado.NombreCompleto;
                dto.Estado = "ACTIVO";

                var id = await db.ExecuteScalarAsync<int>(@"
                    INSERT INTO hr_contrato
                        (empleado_id, empleado_nombre, nombre, fecha_inicio, fecha_fin,
                         sueldo, moneda, tipo_contrato, estado)
                    VALUES
                        (@EmpleadoId, @EmpleadoNombre, @Nombre, @FechaInicio, @FechaFin,
                         @Sueldo, @Moneda, @TipoContrato, 'ACTIVO');
                    SELECT LAST_INSERT_ID();", dto, tx);

                await tx.CommitAsync();
                dto.Id = id;
                return Ok(new
                {
                    mensaje = "Contrato creado. Si existía uno anterior, fue cerrado automáticamente.",
                    contrato = dto
                });
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        // ════════════════════════════════════════════════════════════════
        //  ASISTENCIA
        // ════════════════════════════════════════════════════════════════

        /// <summary>Lista asistencias de un empleado (últimos 30 días por defecto).</summary>
        [HttpGet("empleados/{empleadoId}/asistencias")]
        public async Task<IActionResult> GetAsistencias(int empleadoId,
            [FromQuery] DateTime? desde, [FromQuery] DateTime? hasta)
        {
            desde ??= DateTime.Today.AddDays(-30);
            hasta ??= DateTime.Today;

            using var db = new MySqlConnection(_conn);
            var data = await db.QueryAsync<HrAsistencia>(@"
                SELECT id AS Id, empleado_id AS EmpleadoId, empleado_nombre AS EmpleadoNombre,
                       fecha AS Fecha, hora_entrada AS HoraEntrada,
                       hora_salida AS HoraSalida, horas_trabajadas AS HorasTrabajadas
                FROM hr_asistencia
                WHERE empleado_id = @empleadoId AND fecha BETWEEN @desde AND @hasta
                ORDER BY fecha DESC",
                new { empleadoId, desde, hasta });
            return Ok(data);
        }

        /// <summary>
        /// Registra la entrada del empleado.
        /// Regla: empleado debe estar activo.
        /// </summary>
        [HttpPost("asistencia/entrada")]
        public async Task<IActionResult> RegistrarEntrada([FromBody] EntradaDto dto)
        {
            if (dto.EmpleadoId <= 0) return BadRequest("Debe indicar el empleado.");

            using var db = new MySqlConnection(_conn);

            var empleado = await db.QueryFirstOrDefaultAsync<HrEmployee>(
                "SELECT id AS Id, nombres AS Nombres, apellidos AS Apellidos, activo AS Activo " +
                "FROM hr_employee WHERE id = @EmpleadoId", dto);
            if (empleado is null) return NotFound("Empleado no encontrado.");

            // Regla: empleado inactivo no puede registrar asistencia
            if (!empleado.Activo)
                return BadRequest("El empleado está inactivo y no puede registrar asistencia.");

            var hoy = DateTime.Today;

            // Verificar que no tenga entrada ya registrada hoy
            var yaEntro = await db.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM hr_asistencia WHERE empleado_id = @EmpleadoId AND fecha = @hoy",
                new { dto.EmpleadoId, hoy });
            if (yaEntro > 0)
                return BadRequest("El empleado ya tiene una entrada registrada para hoy.");

            var id = await db.ExecuteScalarAsync<int>(@"
                INSERT INTO hr_asistencia (empleado_id, empleado_nombre, fecha, hora_entrada)
                VALUES (@EmpleadoId, @EmpleadoNombre, @hoy, NOW());
                SELECT LAST_INSERT_ID();",
                new { dto.EmpleadoId, EmpleadoNombre = empleado.NombreCompleto, hoy });

            return Ok(new
            {
                id,
                mensaje = $"Entrada registrada para {empleado.NombreCompleto}.",
                fecha = hoy,
                horaEntrada = DateTime.Now
            });
        }

        /// <summary>
        /// Registra la salida del empleado.
        /// Regla: debe existir entrada del mismo día; horas_trabajadas no puede ser negativa.
        /// </summary>
        [HttpPut("asistencia/salida")]
        public async Task<IActionResult> RegistrarSalida([FromBody] EntradaDto dto)
        {
            if (dto.EmpleadoId <= 0) return BadRequest("Debe indicar el empleado.");

            using var db = new MySqlConnection(_conn);
            var hoy = DateTime.Today;

            // Regla: debe existir entrada del mismo día sin salida
            var asistencia = await db.QueryFirstOrDefaultAsync<HrAsistencia>(@"
                SELECT id AS Id, hora_entrada AS HoraEntrada, hora_salida AS HoraSalida
                FROM hr_asistencia
                WHERE empleado_id = @EmpleadoId AND fecha = @hoy AND hora_salida IS NULL",
                new { dto.EmpleadoId, hoy });

            if (asistencia is null)
                return BadRequest("No existe una entrada registrada hoy sin salida para este empleado.");

            var horaSalida = DateTime.Now;
            var horasTrabajadas = (horaSalida - asistencia.HoraEntrada).TotalHours;

            // Regla: horas_trabajadas no puede ser negativa
            if (horasTrabajadas < 0)
                return BadRequest("Error: la hora de salida es anterior a la hora de entrada.");

            await db.ExecuteAsync(
                "UPDATE hr_asistencia SET hora_salida = @horaSalida, horas_trabajadas = @horasTrabajadas " +
                "WHERE id = @Id",
                new { horaSalida, horasTrabajadas = Math.Round(horasTrabajadas, 2), asistencia.Id });

            return Ok(new
            {
                mensaje = "Salida registrada correctamente.",
                horaSalida,
                horasTrabajadas = Math.Round(horasTrabajadas, 2)
            });
        }

        // ════════════════════════════════════════════════════════════════
        //  TIPOS DE AUSENCIA
        // ════════════════════════════════════════════════════════════════

        /// <summary>Lista los tipos de ausencia configurados.</summary>
        [HttpGet("tipos-ausencia")]
        public async Task<IActionResult> GetTiposAusencia()
        {
            using var db = new MySqlConnection(_conn);
            var data = await db.QueryAsync<HrTipoAusencia>(
                "SELECT id AS Id, nombre AS Nombre, " +
                "requiere_aprobacion AS RequiereAprobacion, dias_maximos AS DiasMaximos " +
                "FROM hr_tipo_ausencia ORDER BY nombre");
            return Ok(data);
        }

        /// <summary>Crea un tipo de ausencia (Vacaciones, Permiso, etc.).</summary>
        [HttpPost("tipos-ausencia")]
        public async Task<IActionResult> CrearTipoAusencia([FromBody] HrTipoAusencia dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Nombre)) return BadRequest("El nombre es obligatorio.");
            if (dto.DiasMaximos <= 0) return BadRequest("Los días máximos deben ser mayor a 0.");

            using var db = new MySqlConnection(_conn);
            var id = await db.ExecuteScalarAsync<int>(
                "INSERT INTO hr_tipo_ausencia (nombre, requiere_aprobacion, dias_maximos) " +
                "VALUES (@Nombre, @RequiereAprobacion, @DiasMaximos); SELECT LAST_INSERT_ID();", dto);
            dto.Id = id;
            return Ok(dto);
        }

        // ════════════════════════════════════════════════════════════════
        //  AUSENCIAS
        // ════════════════════════════════════════════════════════════════

        /// <summary>Lista ausencias de un empleado.</summary>
        [HttpGet("empleados/{empleadoId}/ausencias")]
        public async Task<IActionResult> GetAusencias(int empleadoId)
        {
            using var db = new MySqlConnection(_conn);
            var data = await db.QueryAsync<HrAusencia>(@"
                SELECT a.id AS Id, a.empleado_id AS EmpleadoId, a.empleado_nombre AS EmpleadoNombre,
                       a.tipo_ausencia_id AS TipoAusenciaId, t.nombre AS TipoAusenciaNombre,
                       a.aprobador_id AS AprobadorId, a.aprobador_nombre AS AprobadorNombre,
                       a.fecha_inicio AS FechaInicio, a.fecha_fin AS FechaFin,
                       a.dias_solicitados AS DiasSolicitados, a.motivo AS Motivo, a.estado AS Estado
                FROM hr_ausencia a
                INNER JOIN hr_tipo_ausencia t ON a.tipo_ausencia_id = t.id
                WHERE a.empleado_id = @empleadoId ORDER BY a.fecha_inicio DESC",
                new { empleadoId });
            return Ok(data);
        }

        /// <summary>
        /// Solicita una ausencia.
        /// Reglas: empleado activo, no auto-aprobarse, días ≤ dias_maximos del tipo.
        /// </summary>
        [HttpPost("ausencias")]
        public async Task<IActionResult> SolicitarAusencia([FromBody] HrAusencia dto)
        {
            if (dto.EmpleadoId <= 0) return BadRequest("Debe indicar el empleado.");
            if (dto.TipoAusenciaId <= 0) return BadRequest("Debe indicar el tipo de ausencia.");
            if (dto.AprobadorId <= 0) return BadRequest("Debe indicar el aprobador.");
            if (dto.FechaInicio == default || dto.FechaFin == default)
                return BadRequest("Las fechas de inicio y fin son obligatorias.");
            if (dto.FechaFin < dto.FechaInicio)
                return BadRequest("La fecha fin no puede ser anterior a la fecha inicio.");

            using var db = new MySqlConnection(_conn);

            // Regla: empleado debe estar activo
            var empleado = await db.QueryFirstOrDefaultAsync<HrEmployee>(
                "SELECT id AS Id, nombres AS Nombres, apellidos AS Apellidos, activo AS Activo " +
                "FROM hr_employee WHERE id = @EmpleadoId", dto);
            if (empleado is null) return NotFound("Empleado no encontrado.");
            if (!empleado.Activo) return BadRequest("El empleado está inactivo y no puede solicitar ausencias.");

            // Regla: el aprobador no puede ser el mismo empleado
            if (dto.AprobadorId == dto.EmpleadoId)
                return BadRequest("El empleado no puede ser su propio aprobador.");

            // Validar tipo de ausencia y días máximos
            var tipoAusencia = await db.QueryFirstOrDefaultAsync<HrTipoAusencia>(
                "SELECT id AS Id, nombre AS Nombre, dias_maximos AS DiasMaximos " +
                "FROM hr_tipo_ausencia WHERE id = @TipoAusenciaId", dto);
            if (tipoAusencia is null) return BadRequest("El tipo de ausencia no existe.");

            // Calcular días solicitados
            dto.DiasSolicitados = (int)(dto.FechaFin - dto.FechaInicio).TotalDays + 1;

            // Regla: días solicitados no pueden exceder días máximos
            if (dto.DiasSolicitados > tipoAusencia.DiasMaximos)
                return BadRequest($"Los días solicitados ({dto.DiasSolicitados}) superan el máximo permitido ({tipoAusencia.DiasMaximos}) para '{tipoAusencia.Nombre}'.");

            dto.EmpleadoNombre = empleado.NombreCompleto;
            dto.TipoAusenciaNombre = tipoAusencia.Nombre;
            dto.Estado = "PENDIENTE";

            // Obtener nombre del aprobador
            var aprobadorNombre = await db.ExecuteScalarAsync<string>(
                "SELECT CONCAT(nombres, ' ', apellidos) FROM hr_employee WHERE id = @AprobadorId",
                dto) ?? "";
            dto.AprobadorNombre = aprobadorNombre;

            var id = await db.ExecuteScalarAsync<int>(@"
                INSERT INTO hr_ausencia
                    (empleado_id, empleado_nombre, tipo_ausencia_id, aprobador_id, aprobador_nombre,
                     fecha_inicio, fecha_fin, dias_solicitados, motivo, estado)
                VALUES
                    (@EmpleadoId, @EmpleadoNombre, @TipoAusenciaId, @AprobadorId, @AprobadorNombre,
                     @FechaInicio, @FechaFin, @DiasSolicitados, @Motivo, 'PENDIENTE');
                SELECT LAST_INSERT_ID();", dto);

            dto.Id = id;
            return Ok(dto);
        }

        /// <summary>
        /// Aprueba o rechaza una ausencia.
        /// Regla: solo el aprobador_id asignado puede aprobar.
        /// </summary>
        [HttpPut("ausencias/{id}/resolver")]
        public async Task<IActionResult> ResolverAusencia(int id, [FromBody] ResolverAusenciaDto dto)
        {
            if (dto.AprobadorId <= 0) return BadRequest("Debe indicar el aprobador.");
            if (dto.Decision != "APROBADA" && dto.Decision != "RECHAZADA")
                return BadRequest("La decisión debe ser APROBADA o RECHAZADA.");

            using var db = new MySqlConnection(_conn);

            var ausencia = await db.QueryFirstOrDefaultAsync<HrAusencia>(
                "SELECT id AS Id, estado AS Estado, aprobador_id AS AprobadorId " +
                "FROM hr_ausencia WHERE id = @id", new { id });
            if (ausencia is null) return NotFound("Ausencia no encontrada.");
            if (ausencia.Estado != "PENDIENTE") return BadRequest("Solo se pueden resolver ausencias en estado PENDIENTE.");

            // Regla: solo el aprobador asignado puede aprobar/rechazar
            if (ausencia.AprobadorId != dto.AprobadorId)
                return BadRequest("Solo el aprobador asignado a esta ausencia puede aprobarla o rechazarla.");

            await db.ExecuteAsync(
                "UPDATE hr_ausencia SET estado = @Decision WHERE id = @id",
                new { dto.Decision, id });

            return Ok(new { mensaje = $"Ausencia {dto.Decision.ToLower()} correctamente." });
        }
    }

    // ════════════════════════════════════════════════════════════════
    //  DTOs auxiliares RRHH
    // ════════════════════════════════════════════════════════════════
    public class EntradaDto
    {
        public int EmpleadoId { get; set; }
    }

    public class ResolverAusenciaDto
    {
        public int AprobadorId { get; set; }
        public string Decision { get; set; } = ""; // APROBADA / RECHAZADA
    }
}