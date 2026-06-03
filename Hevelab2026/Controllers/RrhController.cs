using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using Dapper;
using Sis_ERP.Models;
using System.Text.Json;
using Hevelab2026.Models.Auth;
using System.Security.Claims;
using Hevelab2026.Data; 
namespace Sis_ERP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RrhhController : Controller
    {
        private readonly string _conn;

        public RrhhController(IConfiguration config)
            => _conn = MySqlConnectionFactory.BuildConnectionString(config);

        // ════════════════════════════════════════════════════════════════
        //  HELPER — Sesión
        // ════════════════════════════════════════════════════════════════

        private UsuarioSesion? GetSesion()
        {
            if (User.Identity?.IsAuthenticated != true) return null;
            return User.ToUsuarioSesion();
        }

        private int GetEmpresaId()
        {
            if (User.Identity?.IsAuthenticated != true) return 0;
            return User.ToUsuarioSesion().EmpresaId;
        }

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
        //  AUXILIARES — Selects para dropdowns
        // ════════════════════════════════════════════════════════════════

        /// <summary>Lista usuarios disponibles para asignar a un empleado (dropdown). Excluye ya asignados.</summary>
        [HttpGet("select/usuarios")]
        public async Task<IActionResult> GetUsuariosSelect()
        {
            var empresaId = GetEmpresaId();
            if (empresaId == 0) return Unauthorized("Sesión no válida.");

            using var db = new MySqlConnection(_conn);
            var data = await db.QueryAsync(@"
                SELECT u.id AS Id,
                    CONCAT(u.nombre, ' ', u.apellido) AS NombreCompleto
                FROM usuario u
                WHERE u.empresa_id = @empresaId
                AND u.activo = 1
                AND u.id NOT IN (
                    SELECT usuario_id FROM hr_employee
                    WHERE usuario_id IS NOT NULL AND activo = 1
                )
                ORDER BY u.nombre, u.apellido",
                new { empresaId });
            return Ok(data);
        }

        /// <summary>Lista TODOS los usuarios activos de la empresa (para responsables, aprobadores, etc.).</summary>
        [HttpGet("select/usuarios-todos")]
        public async Task<IActionResult> GetUsuariosTodosSelect()
        {
            var empresaId = GetEmpresaId();
            if (empresaId == 0) return Unauthorized("Sesión no válida.");

            using var db = new MySqlConnection(_conn);
            var data = await db.QueryAsync(@"
                SELECT u.id AS Id,
                    CONCAT(u.nombre, ' ', u.apellido) AS NombreCompleto
                FROM usuario u
                WHERE u.empresa_id = @empresaId
                AND u.activo = 1
                ORDER BY u.nombre, u.apellido",
                new { empresaId });
            return Ok(data);
        }

        /// <summary>Diagnóstico temporal: verifica sesión y cuenta usuarios.</summary>
        [HttpGet("debug-session")]
        public async Task<IActionResult> DebugSession()
        {
            var isAuth = User.Identity?.IsAuthenticated;
            var claimsList = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
            var empresaId = GetEmpresaId();

            int totalUsuarios = 0;
            if (empresaId > 0)
            {
                using var db = new MySqlConnection(_conn);
                totalUsuarios = await db.ExecuteScalarAsync<int>(
                    "SELECT COUNT(*) FROM usuario WHERE empresa_id = @empresaId AND activo = 1",
                    new { empresaId });
            }

            return Ok(new
            {
                IsAuthenticated = isAuth,
                EmpresaId = empresaId,
                TotalUsuariosActivos = totalUsuarios,
                Claims = claimsList
            });
        }

        /// <summary>Lista monedas disponibles (dropdown).</summary>
        [HttpGet("select/monedas")]
        public async Task<IActionResult> GetMonedasSelect()
        {
            using var db = new MySqlConnection(_conn);
            var data = await db.QueryAsync(
                "SELECT id AS Id, CONCAT(nombre, ' (', simbolo, ')') AS Nombre " +
                "FROM moneda WHERE activo = 1 ORDER BY nombre");
            return Ok(data);
        }

        /// <summary>Lista departamentos para dropdown.</summary>
        [HttpGet("select/departamentos")]
        public async Task<IActionResult> GetDepartamentosSelect()
        {
            var empresaId = GetEmpresaId();
            if (empresaId == 0) return Unauthorized("Sesión no válida.");

            using var db = new MySqlConnection(_conn);
            var data = await db.QueryAsync(
                "SELECT id AS Id, nombre AS Nombre FROM hr_departamento " +
                "WHERE empresa_id = @empresaId AND activo = 1 ORDER BY nombre",
                new { empresaId });
            return Ok(data);
        }

        /// <summary>Lista cargos para dropdown.</summary>
        [HttpGet("select/cargos")]
        public async Task<IActionResult> GetCargosSelect()
        {
            var empresaId = GetEmpresaId();
            if (empresaId == 0) return Unauthorized("Sesión no válida.");

            using var db = new MySqlConnection(_conn);
            var data = await db.QueryAsync(
                "SELECT id AS Id, nombre AS Nombre FROM hr_cargo " +
                "WHERE empresa_id = @empresaId AND activo = 1 ORDER BY nombre",
                new { empresaId });
            return Ok(data);
        }

        /// <summary>Lista empleados activos para dropdown (aprobador, responsable, etc.).</summary>
        [HttpGet("select/empleados")]
        public async Task<IActionResult> GetEmpleadosSelect()
        {
            var empresaId = GetEmpresaId();
            if (empresaId == 0) return Unauthorized("Sesión no válida.");

            using var db = new MySqlConnection(_conn);
            var data = await db.QueryAsync(
                "SELECT id AS Id, CONCAT(nombres, ' ', apellidos) AS NombreCompleto " +
                "FROM hr_employee WHERE empresa_id = @empresaId AND activo = 1 " +
                "ORDER BY apellidos, nombres",
                new { empresaId });
            return Ok(data);
        }

        // ════════════════════════════════════════════════════════════════
        //  DEPARTAMENTOS
        // ════════════════════════════════════════════════════════════════

        /// <summary>Lista todos los departamentos de la empresa.</summary>
        [HttpGet("departamentos")]
        public async Task<IActionResult> GetDepartamentos()
        {
            var empresaId = GetEmpresaId();
            if (empresaId == 0) return Unauthorized("Sesión no válida.");

            using var db = new MySqlConnection(_conn);
            var data = await db.QueryAsync<HrDepartamento>(@"
                SELECT d.id          AS Id,
                       d.nombre      AS Nombre,
                       d.responsable_id AS ResponsableId,
                       CONCAT(u.nombre, ' ', u.apellido) AS ResponsableNombre
                FROM hr_departamento d
                LEFT JOIN usuario u ON d.responsable_id = u.id
                WHERE d.empresa_id = @empresaId AND d.activo = 1
                ORDER BY d.nombre",
                new { empresaId });
            return Ok(data);
        }

        /// <summary>Crea un nuevo departamento.</summary>
        [HttpPost("departamentos")]
        public async Task<IActionResult> CrearDepartamento([FromBody] HrDepartamento dto)
        {
            var empresaId = GetEmpresaId();
            if (empresaId == 0) return Unauthorized("Sesión no válida.");
            if (string.IsNullOrWhiteSpace(dto.Nombre))
                return BadRequest("El nombre del departamento es obligatorio.");

            using var db = new MySqlConnection(_conn);

            // Validar que el responsable exista si se envió
            if (dto.ResponsableId > 0)
            {
                var existe = await db.ExecuteScalarAsync<int>(
                    "SELECT COUNT(*) FROM usuario WHERE id = @ResponsableId AND empresa_id = @empresaId",
                    new { dto.ResponsableId, empresaId });
                if (existe == 0)
                    return BadRequest("El responsable indicado no existe.");
            }

            var id = await db.ExecuteScalarAsync<int>(@"
                INSERT INTO hr_departamento (empresa_id, nombre, responsable_id, activo)
                VALUES (@empresaId, @Nombre, @ResponsableId, 1);
                SELECT LAST_INSERT_ID();",
                new { empresaId, dto.Nombre, ResponsableId = dto.ResponsableId > 0 ? dto.ResponsableId : (int?)null });

            dto.Id = id;
            return Ok(dto);
        }

        // ════════════════════════════════════════════════════════════════
        //  CARGOS
        // ════════════════════════════════════════════════════════════════

        /// <summary>Lista todos los cargos de la empresa.</summary>
        [HttpGet("cargos")]
        public async Task<IActionResult> GetCargos()
        {
            var empresaId = GetEmpresaId();
            if (empresaId == 0) return Unauthorized("Sesión no válida.");

            using var db = new MySqlConnection(_conn);
            var data = await db.QueryAsync<HrCargo>(
                "SELECT id AS Id, nombre AS Nombre, descripcion AS Descripcion " +
                "FROM hr_cargo WHERE empresa_id = @empresaId AND activo = 1 ORDER BY nombre",
                new { empresaId });
            return Ok(data);
        }

        /// <summary>Crea un nuevo cargo.</summary>
        [HttpPost("cargos")]
        public async Task<IActionResult> CrearCargo([FromBody] HrCargo dto)
        {
            var empresaId = GetEmpresaId();
            if (empresaId == 0) return Unauthorized("Sesión no válida.");
            if (string.IsNullOrWhiteSpace(dto.Nombre))
                return BadRequest("El nombre del cargo es obligatorio.");

            using var db = new MySqlConnection(_conn);
            var id = await db.ExecuteScalarAsync<int>(@"
                INSERT INTO hr_cargo (empresa_id, nombre, descripcion, activo)
                VALUES (@empresaId, @Nombre, @Descripcion, 1);
                SELECT LAST_INSERT_ID();",
                new { empresaId, dto.Nombre, dto.Descripcion });

            dto.Id = id;
            return Ok(dto);
        }

        // ════════════════════════════════════════════════════════════════
        //  EMPLEADOS
        // ════════════════════════════════════════════════════════════════

        /// <summary>Lista empleados de la empresa.</summary>
        [HttpGet("empleados")]
        public async Task<IActionResult> GetEmpleados([FromQuery] bool soloActivos = true)
        {
            var empresaId = GetEmpresaId();
            if (empresaId == 0) return Unauthorized("Sesión no válida.");

            using var db = new MySqlConnection(_conn);
            var sql = @"
                SELECT e.id               AS Id,
                       e.usuario_id       AS UsuarioId,
                       e.departamento_id  AS DepartamentoId,
                       d.nombre           AS DepartamentoNombre,
                       e.cargo_id         AS CargoId,
                       c.nombre           AS CargoNombre,
                       e.nombres          AS Nombres,
                       e.apellidos        AS Apellidos,
                       e.tipo_documento   AS TipoDocumento,
                       e.numero_documento AS NumeroDocumento,
                       e.fecha_ingreso    AS FechaIngreso,
                       e.tipo_contrato    AS TipoContrato,
                       e.regimen_laboral  AS RegimenLaboral,
                       e.activo           AS Activo
                FROM hr_employee e
                LEFT JOIN hr_departamento d ON e.departamento_id = d.id
                LEFT JOIN hr_cargo        c ON e.cargo_id        = c.id
                WHERE e.empresa_id = @empresaId";

            if (soloActivos) sql += " AND e.activo = 1";
            sql += " ORDER BY e.apellidos, e.nombres";

            var data = await db.QueryAsync<HrEmployee>(sql, new { empresaId });
            return Ok(data);
        }

        /// <summary>Obtiene un empleado por ID.</summary>
        [HttpGet("empleados/{id}")]
        public async Task<IActionResult> GetEmpleado(int id)
        {
            var empresaId = GetEmpresaId();
            if (empresaId == 0) return Unauthorized("Sesión no válida.");

            using var db = new MySqlConnection(_conn);
            var emp = await db.QueryFirstOrDefaultAsync<HrEmployee>(@"
                SELECT e.id               AS Id,
                       e.usuario_id       AS UsuarioId,
                       e.departamento_id  AS DepartamentoId,
                       d.nombre           AS DepartamentoNombre,
                       e.cargo_id         AS CargoId,
                       c.nombre           AS CargoNombre,
                       e.nombres          AS Nombres,
                       e.apellidos        AS Apellidos,
                       e.tipo_documento   AS TipoDocumento,
                       e.numero_documento AS NumeroDocumento,
                       e.fecha_ingreso    AS FechaIngreso,
                       e.tipo_contrato    AS TipoContrato,
                       e.regimen_laboral  AS RegimenLaboral,
                       e.activo           AS Activo
                FROM hr_employee e
                LEFT JOIN hr_departamento d ON e.departamento_id = d.id
                LEFT JOIN hr_cargo        c ON e.cargo_id        = c.id
                WHERE e.id = @id AND e.empresa_id = @empresaId",
                new { id, empresaId });

            if (emp is null) return NotFound("Empleado no encontrado.");
            return Ok(emp);
        }

        /// <summary>
        /// Registra un nuevo empleado.
        /// Regla: usuario_id debe existir en tabla usuario si se envía.
        /// </summary>
        [HttpPost("empleados")]
        public async Task<IActionResult> CrearEmpleado([FromBody] HrEmployee dto)
        {
            var empresaId = GetEmpresaId();
            if (empresaId == 0) return Unauthorized("Sesión no válida.");

            if (string.IsNullOrWhiteSpace(dto.Nombres))      return BadRequest("Los nombres son obligatorios.");
            if (string.IsNullOrWhiteSpace(dto.Apellidos))    return BadRequest("Los apellidos son obligatorios.");
            if (string.IsNullOrWhiteSpace(dto.NumeroDocumento)) return BadRequest("El número de documento es obligatorio.");
            if (dto.DepartamentoId <= 0) return BadRequest("Debe indicar un departamento.");
            if (dto.CargoId <= 0)        return BadRequest("Debe indicar un cargo.");

            using var db = new MySqlConnection(_conn);

            // Regla: si viene usuario_id, debe existir en la empresa
            if (dto.UsuarioId.HasValue)
            {
                var usuarioExiste = await db.ExecuteScalarAsync<int>(
                    "SELECT COUNT(*) FROM usuario WHERE id = @UsuarioId AND empresa_id = @empresaId",
                    new { dto.UsuarioId, empresaId });
                if (usuarioExiste == 0)
                    return BadRequest("El usuario indicado no existe o no pertenece a esta empresa.");

                // Verificar que no esté ya asignado a otro empleado activo
                var yaAsignado = await db.ExecuteScalarAsync<int>(
                    "SELECT COUNT(*) FROM hr_employee WHERE usuario_id = @UsuarioId AND activo = 1",
                    new { dto.UsuarioId });
                if (yaAsignado > 0)
                    return BadRequest("Este usuario ya está asignado a otro empleado activo.");
            }

            // Verificar que departamento y cargo pertenezcan a la empresa
            var deptExiste = await db.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM hr_departamento WHERE id = @DepartamentoId AND empresa_id = @empresaId AND activo = 1",
                new { dto.DepartamentoId, empresaId });
            if (deptExiste == 0) return BadRequest("El departamento indicado no existe o no pertenece a esta empresa.");

            var cargoExiste = await db.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM hr_cargo WHERE id = @CargoId AND empresa_id = @empresaId AND activo = 1",
                new { dto.CargoId, empresaId });
            if (cargoExiste == 0) return BadRequest("El cargo indicado no existe o no pertenece a esta empresa.");

            var id = await db.ExecuteScalarAsync<int>(@"
                INSERT INTO hr_employee
                    (empresa_id, usuario_id, departamento_id, cargo_id,
                     nombres, apellidos, tipo_documento, numero_documento,
                     fecha_ingreso, tipo_contrato, regimen_laboral, activo)
                VALUES
                    (@empresaId, @UsuarioId, @DepartamentoId, @CargoId,
                     @Nombres, @Apellidos, @TipoDocumento, @NumeroDocumento,
                     @FechaIngreso, @TipoContrato, @RegimenLaboral, 1);
                SELECT LAST_INSERT_ID();",
                new
                {
                    empresaId,
                    dto.UsuarioId,
                    dto.DepartamentoId,
                    dto.CargoId,
                    dto.Nombres,
                    dto.Apellidos,
                    dto.TipoDocumento,
                    dto.NumeroDocumento,
                    dto.FechaIngreso,
                    dto.TipoContrato,
                    dto.RegimenLaboral
                });

            dto.Id = id;
            dto.Activo = true;
            return Ok(dto);
        }

        /// <summary>Desactiva un empleado (baja lógica).</summary>
        [HttpPut("empleados/{id}/desactivar")]
        public async Task<IActionResult> DesactivarEmpleado(int id)
        {
            var empresaId = GetEmpresaId();
            if (empresaId == 0) return Unauthorized("Sesión no válida.");

            using var db = new MySqlConnection(_conn);
            var afectados = await db.ExecuteAsync(
                "UPDATE hr_employee SET activo = 0, fecha_cese = CURDATE() " +
                "WHERE id = @id AND empresa_id = @empresaId",
                new { id, empresaId });

            if (afectados == 0) return NotFound("Empleado no encontrado.");
            return Ok(new { mensaje = "Empleado desactivado correctamente." });
        }

        // ════════════════════════════════════════════════════════════════
        //  CONTRATOS
        // ════════════════════════════════════════════════════════════════

        /// <summary>Lista contratos de un empleado.</summary>
        [HttpGet("empleados/{empleadoId}/contratos")]
        public async Task<IActionResult> GetContratos(int empleadoId)
        {
            var empresaId = GetEmpresaId();
            if (empresaId == 0) return Unauthorized("Sesión no válida.");

            using var db = new MySqlConnection(_conn);
            var data = await db.QueryAsync<HrContrato>(@"
                SELECT c.id             AS Id,
                       c.empleado_id    AS EmpleadoId,
                       CONCAT(e.nombres, ' ', e.apellidos) AS EmpleadoNombre,
                       c.nombre         AS Nombre,
                       c.fecha_inicio   AS FechaInicio,
                       c.fecha_fin      AS FechaFin,
                       c.sueldo         AS Sueldo,
                       c.moneda_id      AS MonedaId,
                       CONCAT(m.nombre, ' (', m.simbolo, ')') AS Moneda,
                       c.tipo_contrato  AS TipoContrato,
                       c.estado         AS Estado
                FROM hr_contrato c
                INNER JOIN hr_employee e ON c.empleado_id = e.id
                INNER JOIN moneda      m ON c.moneda_id   = m.id
                WHERE c.empleado_id = @empleadoId
                  AND e.empresa_id  = @empresaId
                ORDER BY c.fecha_inicio DESC",
                new { empleadoId, empresaId });
            return Ok(data);
        }

        /// <summary>
        /// Registra un nuevo contrato.
        /// Regla: solo un contrato ACTIVO por empleado; el anterior se cierra automáticamente.
        /// </summary>
        [HttpPost("contratos")]
        public async Task<IActionResult> CrearContrato([FromBody] HrContrato dto)
        {
            var empresaId = GetEmpresaId();
            if (empresaId == 0) return Unauthorized("Sesión no válida.");

            if (dto.EmpleadoId <= 0)               return BadRequest("Debe indicar el empleado.");
            if (string.IsNullOrWhiteSpace(dto.Nombre)) return BadRequest("El nombre del contrato es obligatorio.");
            if (dto.Sueldo <= 0)                   return BadRequest("El sueldo debe ser mayor a 0.");
            if (dto.MonedaId <= 0)                 return BadRequest("Debe indicar la moneda.");
            if (dto.FechaInicio == default)        return BadRequest("La fecha de inicio es obligatoria.");

            using var db = new MySqlConnection(_conn);
            await db.OpenAsync();

            // Verificar que el empleado existe, está activo y pertenece a la empresa
            var empleado = await db.QueryFirstOrDefaultAsync<HrEmployee>(@"
                SELECT id AS Id, nombres AS Nombres, apellidos AS Apellidos, activo AS Activo
                FROM hr_employee
                WHERE id = @EmpleadoId AND empresa_id = @empresaId",
                new { dto.EmpleadoId, empresaId });

            if (empleado is null)    return NotFound("Empleado no encontrado.");
            if (!empleado.Activo)    return BadRequest("El empleado está inactivo y no puede tener contratos nuevos.");

            // Verificar que la moneda existe
            var monedaExiste = await db.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM moneda WHERE id = @MonedaId AND activo = 1",
                new { dto.MonedaId });
            if (monedaExiste == 0) return BadRequest("La moneda indicada no existe.");

            using var tx = await db.BeginTransactionAsync();
            try
            {
                // Regla: cerrar contrato activo anterior
                await db.ExecuteAsync(@"
                    UPDATE hr_contrato
                    SET estado = 'CERRADO', fecha_fin = @FechaInicio
                    WHERE empleado_id = @EmpleadoId AND estado = 'ACTIVO'",
                    new { dto.FechaInicio, dto.EmpleadoId }, tx);

                var id = await db.ExecuteScalarAsync<int>(@"
                    INSERT INTO hr_contrato
                        (empleado_id, nombre, fecha_inicio, fecha_fin,
                         sueldo, moneda_id, tipo_contrato, estado)
                    VALUES
                        (@EmpleadoId, @Nombre, @FechaInicio, @FechaFin,
                         @Sueldo, @MonedaId, @TipoContrato, 'ACTIVO');
                    SELECT LAST_INSERT_ID();",
                    new
                    {
                        dto.EmpleadoId,
                        dto.Nombre,
                        dto.FechaInicio,
                        dto.FechaFin,
                        dto.Sueldo,
                        dto.MonedaId,
                        dto.TipoContrato
                    }, tx);

                await tx.CommitAsync();
                dto.Id = id;
                dto.Estado = "ACTIVO";
                dto.EmpleadoNombre = empleado.NombreCompleto;

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
            var empresaId = GetEmpresaId();
            if (empresaId == 0) return Unauthorized("Sesión no válida.");

            desde ??= DateTime.Today.AddDays(-30);
            hasta ??= DateTime.Today;

            using var db = new MySqlConnection(_conn);
            var data = await db.QueryAsync<HrAsistencia>(@"
                SELECT a.id                AS Id,
                       a.empleado_id       AS EmpleadoId,
                       CONCAT(e.nombres, ' ', e.apellidos) AS EmpleadoNombre,
                       a.fecha             AS Fecha,
                       a.hora_entrada      AS HoraEntrada,
                       a.hora_salida       AS HoraSalida,
                       a.horas_trabajadas  AS HorasTrabajadas,
                       a.observaciones     AS Observaciones
                FROM hr_asistencia a
                INNER JOIN hr_employee e ON a.empleado_id = e.id
                WHERE a.empleado_id = @empleadoId
                  AND e.empresa_id  = @empresaId
                  AND a.fecha BETWEEN @desde AND @hasta
                ORDER BY a.fecha DESC",
                new { empleadoId, empresaId, desde, hasta });
            return Ok(data);
        }

        /// <summary>
        /// Registra la entrada del empleado.
        /// Regla: empleado activo, no puede tener entrada duplicada el mismo día.
        /// </summary>
        [HttpPost("asistencia/entrada")]
        public async Task<IActionResult> RegistrarEntrada([FromBody] EntradaDto dto)
        {
            var empresaId = GetEmpresaId();
            if (empresaId == 0) return Unauthorized("Sesión no válida.");
            if (dto.EmpleadoId <= 0) return BadRequest("Debe indicar el empleado.");

            using var db = new MySqlConnection(_conn);

            var empleado = await db.QueryFirstOrDefaultAsync<HrEmployee>(@"
                SELECT id AS Id, nombres AS Nombres, apellidos AS Apellidos, activo AS Activo
                FROM hr_employee
                WHERE id = @EmpleadoId AND empresa_id = @empresaId",
                new { dto.EmpleadoId, empresaId });

            if (empleado is null)  return NotFound("Empleado no encontrado.");
            if (!empleado.Activo)  return BadRequest("El empleado está inactivo y no puede registrar asistencia.");

            var hoy = DateTime.Today;

            // Verificar que no tenga entrada registrada hoy
            var yaEntro = await db.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM hr_asistencia WHERE empleado_id = @EmpleadoId AND fecha = @hoy",
                new { dto.EmpleadoId, hoy });
            if (yaEntro > 0)
                return BadRequest("El empleado ya tiene una entrada registrada para hoy.");

            var id = await db.ExecuteScalarAsync<long>(@"
                INSERT INTO hr_asistencia (empleado_id, fecha, hora_entrada)
                VALUES (@EmpleadoId, @hoy, NOW());
                SELECT LAST_INSERT_ID();",
                new { dto.EmpleadoId, hoy });

            return Ok(new
            {
                id,
                mensaje    = $"Entrada registrada para {empleado.NombreCompleto}.",
                fecha      = hoy,
                horaEntrada = DateTime.Now
            });
        }

        /// <summary>
        /// Registra la salida del empleado.
        /// Regla: debe existir entrada del mismo día sin salida. horas_trabajadas no puede ser negativa.
        /// </summary>
        [HttpPut("asistencia/salida")]
        public async Task<IActionResult> RegistrarSalida([FromBody] EntradaDto dto)
        {
            var empresaId = GetEmpresaId();
            if (empresaId == 0) return Unauthorized("Sesión no válida.");
            if (dto.EmpleadoId <= 0) return BadRequest("Debe indicar el empleado.");

            using var db = new MySqlConnection(_conn);
            var hoy = DateTime.Today;

            // Regla: debe existir entrada del mismo día sin salida
            var asistencia = await db.QueryFirstOrDefaultAsync<HrAsistencia>(@"
                SELECT a.id           AS Id,
                       a.hora_entrada AS HoraEntrada,
                       a.hora_salida  AS HoraSalida
                FROM hr_asistencia a
                INNER JOIN hr_employee e ON a.empleado_id = e.id
                WHERE a.empleado_id = @EmpleadoId
                  AND e.empresa_id  = @empresaId
                  AND a.fecha       = @hoy
                  AND a.hora_salida IS NULL",
                new { dto.EmpleadoId, empresaId, hoy });

            if (asistencia is null)
                return BadRequest("No existe una entrada registrada hoy sin salida para este empleado.");

            var horaSalida     = DateTime.Now;
            var horasTrabajadas = (horaSalida - asistencia.HoraEntrada).TotalHours;

            // Regla: horas_trabajadas no puede ser negativa
            if (horasTrabajadas < 0)
                return BadRequest("Error: la hora de salida es anterior a la hora de entrada.");

            await db.ExecuteAsync(@"
                UPDATE hr_asistencia
                SET hora_salida = @horaSalida, horas_trabajadas = @horasTrabajadas
                WHERE id = @Id",
                new { horaSalida, horasTrabajadas = Math.Round(horasTrabajadas, 2), asistencia.Id });

            return Ok(new
            {
                mensaje         = "Salida registrada correctamente.",
                horaSalida,
                horasTrabajadas = Math.Round(horasTrabajadas, 2)
            });
        }

        // ════════════════════════════════════════════════════════════════
        //  TIPOS DE AUSENCIA
        // ════════════════════════════════════════════════════════════════

        /// <summary>Lista los tipos de ausencia configurados para la empresa.</summary>
        [HttpGet("tipos-ausencia")]
        public async Task<IActionResult> GetTiposAusencia()
        {
            var empresaId = GetEmpresaId();
            if (empresaId == 0) return Unauthorized("Sesión no válida.");

            using var db = new MySqlConnection(_conn);
            var data = await db.QueryAsync<HrTipoAusencia>(@"
                SELECT id                   AS Id,
                       nombre               AS Nombre,
                       requiere_aprobacion  AS RequiereAprobacion,
                       dias_maximos         AS DiasMaximos
                FROM hr_tipo_ausencia
                WHERE empresa_id = @empresaId AND activo = 1
                ORDER BY nombre",
                new { empresaId });
            return Ok(data);
        }

        /// <summary>Crea un tipo de ausencia (Vacaciones, Permiso, etc.).</summary>
        [HttpPost("tipos-ausencia")]
        public async Task<IActionResult> CrearTipoAusencia([FromBody] HrTipoAusencia dto)
        {
            var empresaId = GetEmpresaId();
            if (empresaId == 0) return Unauthorized("Sesión no válida.");
            if (string.IsNullOrWhiteSpace(dto.Nombre)) return BadRequest("El nombre es obligatorio.");
            if (dto.DiasMaximos <= 0) return BadRequest("Los días máximos deben ser mayor a 0.");

            using var db = new MySqlConnection(_conn);
            var id = await db.ExecuteScalarAsync<int>(@"
                INSERT INTO hr_tipo_ausencia (empresa_id, nombre, requiere_aprobacion, dias_maximos, activo)
                VALUES (@empresaId, @Nombre, @RequiereAprobacion, @DiasMaximos, 1);
                SELECT LAST_INSERT_ID();",
                new { empresaId, dto.Nombre, dto.RequiereAprobacion, dto.DiasMaximos });

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
            var empresaId = GetEmpresaId();
            if (empresaId == 0) return Unauthorized("Sesión no válida.");

            using var db = new MySqlConnection(_conn);
            var data = await db.QueryAsync<HrAusencia>(@"
                SELECT a.id              AS Id,
                       a.empleado_id     AS EmpleadoId,
                       CONCAT(e.nombres, ' ', e.apellidos) AS EmpleadoNombre,
                       a.tipo_ausencia_id AS TipoAusenciaId,
                       t.nombre          AS TipoAusenciaNombre,
                       a.aprobador_id    AS AprobadorId,
                       CONCAT(u.nombre, ' ', u.apellido) AS AprobadorNombre,
                       a.fecha_inicio    AS FechaInicio,
                       a.fecha_fin       AS FechaFin,
                       a.dias_solicitados AS DiasSolicitados,
                       a.motivo          AS Motivo,
                       a.estado          AS Estado
                FROM hr_ausencia a
                INNER JOIN hr_employee    e ON a.empleado_id      = e.id
                INNER JOIN hr_tipo_ausencia t ON a.tipo_ausencia_id = t.id
                LEFT  JOIN usuario        u ON a.aprobador_id     = u.id
                WHERE a.empleado_id = @empleadoId
                  AND e.empresa_id  = @empresaId
                ORDER BY a.fecha_inicio DESC",
                new { empleadoId, empresaId });
            return Ok(data);
        }

        /// <summary>
        /// Solicita una ausencia.
        /// Reglas: empleado activo, no auto-aprobarse, días ≤ dias_maximos del tipo.
        /// </summary>
        [HttpPost("ausencias")]
        public async Task<IActionResult> SolicitarAusencia([FromBody] HrAusencia dto)
        {
            var empresaId = GetEmpresaId();
            if (empresaId == 0) return Unauthorized("Sesión no válida.");

            if (dto.EmpleadoId    <= 0) return BadRequest("Debe indicar el empleado.");
            if (dto.TipoAusenciaId <= 0) return BadRequest("Debe indicar el tipo de ausencia.");
            if (dto.AprobadorId   <= 0) return BadRequest("Debe indicar el aprobador.");
            if (dto.FechaInicio == default || dto.FechaFin == default)
                return BadRequest("Las fechas de inicio y fin son obligatorias.");
            if (dto.FechaFin < dto.FechaInicio)
                return BadRequest("La fecha fin no puede ser anterior a la fecha inicio.");

            using var db = new MySqlConnection(_conn);

            // Verificar empleado activo y de la empresa
            var empleado = await db.QueryFirstOrDefaultAsync<HrEmployee>(@"
                SELECT id AS Id, nombres AS Nombres, apellidos AS Apellidos, activo AS Activo
                FROM hr_employee
                WHERE id = @EmpleadoId AND empresa_id = @empresaId",
                new { dto.EmpleadoId, empresaId });

            if (empleado is null)  return NotFound("Empleado no encontrado.");
            if (!empleado.Activo)  return BadRequest("El empleado está inactivo y no puede solicitar ausencias.");

            // Regla: el aprobador no puede ser el mismo empleado
            // Comparamos por usuario_id si existe, y también por id de empleado
            if (dto.AprobadorId == dto.EmpleadoId)
                return BadRequest("El empleado no puede ser su propio aprobador.");

            // Verificar que el aprobador existe en la tabla usuario
            var aprobadorExiste = await db.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM usuario WHERE id = @AprobadorId",
                new { dto.AprobadorId });
            if (aprobadorExiste == 0)
                return BadRequest("El aprobador indicado no existe.");

            // Validar tipo de ausencia y días máximos
            var tipoAusencia = await db.QueryFirstOrDefaultAsync<HrTipoAusencia>(@"
                SELECT id AS Id, nombre AS Nombre, dias_maximos AS DiasMaximos
                FROM hr_tipo_ausencia
                WHERE id = @TipoAusenciaId AND empresa_id = @empresaId AND activo = 1",
                new { dto.TipoAusenciaId, empresaId });

            if (tipoAusencia is null)
                return BadRequest("El tipo de ausencia no existe o no pertenece a esta empresa.");

            // Calcular días solicitados
            var diasSolicitados = (int)(dto.FechaFin - dto.FechaInicio).TotalDays + 1;

            // Regla: días solicitados no pueden exceder días máximos
            if (diasSolicitados > tipoAusencia.DiasMaximos)
                return BadRequest($"Los días solicitados ({diasSolicitados}) superan el máximo permitido " +
                                  $"({tipoAusencia.DiasMaximos}) para '{tipoAusencia.Nombre}'.");

            var id = await db.ExecuteScalarAsync<int>(@"
                INSERT INTO hr_ausencia
                    (empleado_id, tipo_ausencia_id, aprobador_id,
                     fecha_inicio, fecha_fin, dias_solicitados, motivo, estado)
                VALUES
                    (@EmpleadoId, @TipoAusenciaId, @AprobadorId,
                     @FechaInicio, @FechaFin, @DiasSolicitados, @Motivo, 'PENDIENTE');
                SELECT LAST_INSERT_ID();",
                new
                {
                    dto.EmpleadoId,
                    dto.TipoAusenciaId,
                    dto.AprobadorId,
                    dto.FechaInicio,
                    dto.FechaFin,
                    DiasSolicitados = diasSolicitados,
                    dto.Motivo
                });

            dto.Id              = id;
            dto.DiasSolicitados = diasSolicitados;
            dto.EmpleadoNombre  = empleado.NombreCompleto;
            dto.TipoAusenciaNombre = tipoAusencia.Nombre;
            dto.Estado          = "PENDIENTE";

            return Ok(dto);
        }

        /// <summary>
        /// Aprueba o rechaza una ausencia.
        /// Regla: solo el aprobador_id asignado puede resolver.
        /// </summary>
        [HttpPut("ausencias/{id}/resolver")]
        public async Task<IActionResult> ResolverAusencia(int id, [FromBody] ResolverAusenciaDto dto)
        {
            var empresaId = GetEmpresaId();
            if (empresaId == 0) return Unauthorized("Sesión no válida.");

            if (dto.AprobadorId <= 0)
                return BadRequest("Debe indicar el aprobador.");
            if (dto.Decision != "APROBADA" && dto.Decision != "RECHAZADA")
                return BadRequest("La decisión debe ser APROBADA o RECHAZADA.");

            using var db = new MySqlConnection(_conn);

            var ausencia = await db.QueryFirstOrDefaultAsync<HrAusencia>(@"
                SELECT a.id           AS Id,
                       a.estado       AS Estado,
                       a.aprobador_id AS AprobadorId
                FROM hr_ausencia a
                INNER JOIN hr_employee e ON a.empleado_id = e.id
                WHERE a.id = @id AND e.empresa_id = @empresaId",
                new { id, empresaId });

            if (ausencia is null)
                return NotFound("Ausencia no encontrada.");
            if (ausencia.Estado != "PENDIENTE")
                return BadRequest("Solo se pueden resolver ausencias en estado PENDIENTE.");

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
        public int    AprobadorId { get; set; }
        public string Decision    { get; set; } = ""; // APROBADA / RECHAZADA
    }
}