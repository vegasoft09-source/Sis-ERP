using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Dapper;
using Sis_ERP.Models;

namespace Sis_ERP.Controllers
{
    // =========================================================
    // Controlador CRM (Vista + API)
    // =========================================================
    [Route("api/[controller]")]
    [ApiController]
    public class CrmController : Controller
    {
        // Vista principal
        [HttpGet("/CRM")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult Index()
        {
            return View("~/Views/CRM/Index.cshtml");
        }

        // API CRM
        private readonly string _conn;
        public CrmController(IConfiguration config)
            => _conn = config.GetConnectionString("DefaultConnection")!;

        // ════════════════════════════════════════════════════════════════
        //  ETAPAS
        // ════════════════════════════════════════════════════════════════

        /// <summary>Lista todas las etapas ordenadas por secuencia.</summary>
        [HttpGet("etapas")]
        public async Task<IActionResult> GetEtapas()
        {
            using var db = new MySqlConnection(_conn);
            var data = await db.QueryAsync<CrmEtapa>(
                "SELECT id AS Id, nombre AS Nombre, secuencia AS Secuencia, " +
                "probabilidad AS Probabilidad, es_ganado AS EsGanado, es_perdido AS EsPerdido " +
                "FROM crm_etapa ORDER BY secuencia");
            return Ok(data);
        }

        /// <summary>Crea una nueva etapa del embudo.</summary>
        [HttpPost("etapas")]
        public async Task<IActionResult> CrearEtapa([FromBody] CrmEtapa dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Nombre))
                return BadRequest("El nombre de la etapa es obligatorio.");

            using var db = new MySqlConnection(_conn);
            var id = await db.ExecuteScalarAsync<int>(
                "INSERT INTO crm_etapa (nombre, secuencia, probabilidad, es_ganado, es_perdido) " +
                "VALUES (@Nombre, @Secuencia, @Probabilidad, @EsGanado, @EsPerdido); " +
                "SELECT LAST_INSERT_ID();", dto);
            dto.Id = id;
            return Ok(dto);
        }

        // ════════════════════════════════════════════════════════════════
        //  ETIQUETAS
        // ════════════════════════════════════════════════════════════════

        /// <summary>Lista todas las etiquetas (tags).</summary>
        [HttpGet("etiquetas")]
        public async Task<IActionResult> GetEtiquetas()
        {
            using var db = new MySqlConnection(_conn);
            var data = await db.QueryAsync<CrmEtiqueta>(
                "SELECT id AS Id, nombre AS Nombre, color AS Color FROM crm_etiqueta ORDER BY nombre");
            return Ok(data);
        }

        /// <summary>Crea una etiqueta nueva.</summary>
        [HttpPost("etiquetas")]
        public async Task<IActionResult> CrearEtiqueta([FromBody] CrmEtiqueta dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Nombre))
                return BadRequest("El nombre de la etiqueta es obligatorio.");

            using var db = new MySqlConnection(_conn);
            var id = await db.ExecuteScalarAsync<int>(
                "INSERT INTO crm_etiqueta (nombre, color) VALUES (@Nombre, @Color); " +
                "SELECT LAST_INSERT_ID();", dto);
            dto.Id = id;
            return Ok(dto);
        }

        // ════════════════════════════════════════════════════════════════
        //  LEADS / OPORTUNIDADES
        // ════════════════════════════════════════════════════════════════

        /// <summary>Lista todos los leads activos con su etapa.</summary>
        [HttpGet("leads")]
        public async Task<IActionResult> GetLeads([FromQuery] bool soloOportunidades = false)
        {
            using var db = new MySqlConnection(_conn);
            var sql = @"
                SELECT l.id AS Id, l.nombre_negocio AS NombreNegocio,
                       l.etapa_id AS EtapaId, e.nombre AS EtapaNombre,
                       l.socio_id AS SocioId, l.es_oportunidad AS EsOportunidad,
                       l.contacto_nombre AS ContactoNombre, l.contacto_correo AS ContactoCorreo,
                       l.contacto_telefono AS ContactoTelefono,
                       l.vendedor_id AS VendedorId, l.vendedor_nombre AS VendedorNombre,
                       l.origen AS Origen, l.prioridad AS Prioridad,
                       l.ingreso_esperado AS IngresoEsperado, l.fecha_cierre AS FechaCierre,
                       l.motivo_perdida AS MotivoPerdida, l.activo AS Activo,
                       l.pedido_venta_id AS PedidoVentaId, e.probabilidad AS ProbabilidadEtapa
                FROM crm_lead l
                INNER JOIN crm_etapa e ON l.etapa_id = e.id
                WHERE l.activo = 1";

            if (soloOportunidades) sql += " AND l.es_oportunidad = 1";
            sql += " ORDER BY l.id DESC";

            var leads = (await db.QueryAsync<CrmLead>(sql)).ToList();

            // Cargar etiquetas de cada lead
            foreach (var lead in leads)
            {
                lead.EtiquetaIds = (await db.QueryAsync<int>(
                    "SELECT etiqueta_id FROM crm_lead_etiqueta WHERE lead_id = @Id",
                    new { lead.Id })).ToList();
            }

            return Ok(leads);
        }

        /// <summary>Obtiene un lead por ID.</summary>
        [HttpGet("leads/{id}")]
        public async Task<IActionResult> GetLead(int id)
        {
            using var db = new MySqlConnection(_conn);
            var lead = await db.QueryFirstOrDefaultAsync<CrmLead>(
                @"SELECT l.id AS Id, l.nombre_negocio AS NombreNegocio,
                         l.etapa_id AS EtapaId, e.nombre AS EtapaNombre,
                         l.socio_id AS SocioId, l.es_oportunidad AS EsOportunidad,
                         l.contacto_nombre AS ContactoNombre, l.contacto_correo AS ContactoCorreo,
                         l.contacto_telefono AS ContactoTelefono,
                         l.vendedor_id AS VendedorId, l.vendedor_nombre AS VendedorNombre,
                         l.origen AS Origen, l.prioridad AS Prioridad,
                         l.ingreso_esperado AS IngresoEsperado, l.fecha_cierre AS FechaCierre,
                         l.motivo_perdida AS MotivoPerdida, l.activo AS Activo,
                         l.pedido_venta_id AS PedidoVentaId, e.probabilidad AS ProbabilidadEtapa
                  FROM crm_lead l
                  INNER JOIN crm_etapa e ON l.etapa_id = e.id
                  WHERE l.id = @id", new { id });

            if (lead is null) return NotFound("Lead no encontrado.");

            lead.EtiquetaIds = (await db.QueryAsync<int>(
                "SELECT etiqueta_id FROM crm_lead_etiqueta WHERE lead_id = @id",
                new { id })).ToList();

            return Ok(lead);
        }

        /// <summary>Registra un nuevo lead (prospecto).</summary>
        [HttpPost("leads")]
        public async Task<IActionResult> CrearLead([FromBody] CrmLead dto)
        {
            if (string.IsNullOrWhiteSpace(dto.NombreNegocio))
                return BadRequest("El nombre del negocio es obligatorio.");
            if (dto.VendedorId <= 0)
                return BadRequest("Debe indicar un vendedor.");

            using var db = new MySqlConnection(_conn);

            // Validar que la etapa exista
            var etapa = await db.QueryFirstOrDefaultAsync<CrmEtapa>(
                "SELECT id AS Id, nombre AS Nombre, es_ganado AS EsGanado, " +
                "es_perdido AS EsPerdido, probabilidad AS Probabilidad " +
                "FROM crm_etapa WHERE id = @EtapaId", dto);
            if (etapa is null) return BadRequest("La etapa indicada no existe.");

            dto.ProbabilidadEtapa = etapa.Probabilidad;

            var id = await db.ExecuteScalarAsync<int>(@"
                INSERT INTO crm_lead
                    (nombre_negocio, etapa_id, socio_id, es_oportunidad,
                     contacto_nombre, contacto_correo, contacto_telefono,
                     vendedor_id, vendedor_nombre, origen, prioridad,
                     ingreso_esperado, fecha_cierre, activo)
                VALUES
                    (@NombreNegocio, @EtapaId, @SocioId, 0,
                     @ContactoNombre, @ContactoCorreo, @ContactoTelefono,
                     @VendedorId, @VendedorNombre, @Origen, @Prioridad,
                     @IngresoEsperado, @FechaCierre, 1);
                SELECT LAST_INSERT_ID();", dto);

            // Asignar etiquetas si vienen
            foreach (var etqId in dto.EtiquetaIds)
                await db.ExecuteAsync(
                    "INSERT IGNORE INTO crm_lead_etiqueta (lead_id, etiqueta_id) VALUES (@lead, @etq)",
                    new { lead = id, etq = etqId });

            dto.Id = id;
            dto.EtapaNombre = etapa.Nombre;
            return Ok(dto);
        }

        /// <summary>
        /// Convierte un Lead en Oportunidad.
        /// Regla: requiere ingreso_esperado y fecha_cierre.
        /// </summary>
        [HttpPut("leads/{id}/convertir-oportunidad")]
        public async Task<IActionResult> ConvertirOportunidad(int id,
            [FromBody] ConvertirOportunidadDto dto)
        {
            using var db = new MySqlConnection(_conn);

            var lead = await db.QueryFirstOrDefaultAsync<CrmLead>(
                "SELECT id AS Id, es_oportunidad AS EsOportunidad, activo AS Activo " +
                "FROM crm_lead WHERE id = @id", new { id });

            if (lead is null) return NotFound("Lead no encontrado.");
            if (!lead.Activo) return BadRequest("El lead está inactivo.");
            if (lead.EsOportunidad) return BadRequest("El lead ya es una oportunidad.");
            if (dto.IngresoEsperado <= 0) return BadRequest("Debe indicar el ingreso esperado.");
            if (dto.FechaCierre == null) return BadRequest("Debe indicar la fecha de cierre estimada.");

            await db.ExecuteAsync(
                "UPDATE crm_lead SET es_oportunidad = 1, ingreso_esperado = @IngresoEsperado, " +
                "fecha_cierre = @FechaCierre, socio_id = @SocioId WHERE id = @id",
                new { dto.IngresoEsperado, dto.FechaCierre, dto.SocioId, id });

            return Ok(new { mensaje = "Lead convertido a oportunidad correctamente." });
        }

        /// <summary>
        /// Avanza el lead a otra etapa.
        /// Regla: si la etapa es_perdido=TRUE, se bloquea avanzar a etapas anteriores.
        /// </summary>
        [HttpPut("leads/{id}/cambiar-etapa")]
        public async Task<IActionResult> CambiarEtapa(int id, [FromBody] CambiarEtapaDto dto)
        {
            using var db = new MySqlConnection(_conn);

            var lead = await db.QueryFirstOrDefaultAsync<dynamic>(
                "SELECT l.id, l.activo, l.etapa_id, " +
                "ep.es_perdido AS etapaActualEsPerdida, ep.secuencia AS secuenciaActual " +
                "FROM crm_lead l " +
                "INNER JOIN crm_etapa ep ON l.etapa_id = ep.id " +
                "WHERE l.id = @id", new { id });

            if (lead is null) return NotFound("Lead no encontrado.");
            if (!(bool)lead.activo) return BadRequest("El lead está inactivo.");

            var nuevaEtapa = await db.QueryFirstOrDefaultAsync<CrmEtapa>(
                "SELECT id AS Id, nombre AS Nombre, secuencia AS Secuencia, " +
                "probabilidad AS Probabilidad, es_ganado AS EsGanado, es_perdido AS EsPerdido " +
                "FROM crm_etapa WHERE id = @NuevaEtapaId", dto);
            if (nuevaEtapa is null) return BadRequest("La nueva etapa no existe.");

            // Regla: lead en etapa perdido no puede retroceder sin aprobación
            if ((bool)lead.etapaActualEsPerdida && nuevaEtapa.Secuencia < (int)lead.secuenciaActual)
                return BadRequest("Un lead en etapa PERDIDO no puede retroceder sin aprobación.");

            await db.ExecuteAsync(
                "UPDATE crm_lead SET etapa_id = @NuevaEtapaId WHERE id = @id",
                new { dto.NuevaEtapaId, id });

            return Ok(new
            {
                mensaje = $"Etapa actualizada a '{nuevaEtapa.Nombre}'.",
                probabilidad = nuevaEtapa.Probabilidad
            });
        }

        /// <summary>
        /// Marca el lead como GANADO y registra pedido de venta.
        /// Regla: el lead debe ser oportunidad y tener socio_id con es_cliente=TRUE.
        /// </summary>
        [HttpPut("leads/{id}/marcar-ganado")]
        public async Task<IActionResult> MarcarGanado(int id, [FromBody] MarcarGanadoDto dto)
        {
            using var db = new MySqlConnection(_conn);

            var lead = await db.QueryFirstOrDefaultAsync<CrmLead>(
                "SELECT id AS Id, es_oportunidad AS EsOportunidad, socio_id AS SocioId, activo AS Activo " +
                "FROM crm_lead WHERE id = @id", new { id });

            if (lead is null) return NotFound("Lead no encontrado.");
            if (!lead.Activo) return BadRequest("El lead está inactivo.");

            // Regla: solo oportunidades pueden generar venta
            if (!lead.EsOportunidad)
                return BadRequest("El lead debe ser convertido a oportunidad antes de marcar como ganado.");

            // Regla: debe tener socio vinculado con es_cliente = TRUE
            if (lead.SocioId is null)
                return BadRequest("Debe vincular un socio al lead antes de convertir a venta.");

            var esCliente = await db.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM socio WHERE id = @SocioId AND es_cliente = 1",
                new { lead.SocioId });
            if (esCliente == 0)
                return BadRequest("El socio vinculado no tiene el flag es_cliente=TRUE. Actualice el socio primero.");

            // Obtener etapa ganada
            var etapaGanada = await db.QueryFirstOrDefaultAsync<CrmEtapa>(
                "SELECT id AS Id FROM crm_etapa WHERE es_ganado = 1 LIMIT 1");
            if (etapaGanada is null)
                return BadRequest("No existe una etapa configurada como GANADO en el sistema.");

            using var tx = await db.BeginTransactionAsync();
            try
            {
                // Actualizar lead
                await db.ExecuteAsync(
                    "UPDATE crm_lead SET etapa_id = @EtapaId, pedido_venta_id = @PedidoId WHERE id = @id",
                    new { EtapaId = etapaGanada.Id, dto.PedidoId, id }, tx);

                await tx.CommitAsync();
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }

            return Ok(new { mensaje = "Lead marcado como GANADO. Pedido de venta vinculado.", pedidoVentaId = dto.PedidoId });
        }

        /// <summary>
        /// Marca el lead como PERDIDO.
        /// Regla: requiere motivo_perdida, pone activo=FALSE.
        /// </summary>
        [HttpPut("leads/{id}/marcar-perdido")]
        public async Task<IActionResult> MarcarPerdido(int id, [FromBody] MarcarPerdidoDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.MotivoPerdida))
                return BadRequest("El motivo de pérdida es obligatorio.");

            using var db = new MySqlConnection(_conn);

            var lead = await db.QueryFirstOrDefaultAsync<CrmLead>(
                "SELECT id AS Id, activo AS Activo FROM crm_lead WHERE id = @id", new { id });
            if (lead is null) return NotFound("Lead no encontrado.");
            if (!lead.Activo) return BadRequest("El lead ya está inactivo.");

            var etapaPerdida = await db.QueryFirstOrDefaultAsync<CrmEtapa>(
                "SELECT id AS Id FROM crm_etapa WHERE es_perdido = 1 LIMIT 1");
            if (etapaPerdida is null)
                return BadRequest("No existe una etapa configurada como PERDIDO en el sistema.");

            await db.ExecuteAsync(
                "UPDATE crm_lead SET etapa_id = @EtapaId, motivo_perdida = @Motivo, activo = 0 WHERE id = @id",
                new { EtapaId = etapaPerdida.Id, Motivo = dto.MotivoPerdida, id });

            return Ok(new { mensaje = "Lead marcado como PERDIDO." });
        }

        // ════════════════════════════════════════════════════════════════
        //  ACTIVIDADES
        // ════════════════════════════════════════════════════════════════

        /// <summary>Lista actividades de un lead.</summary>
        [HttpGet("leads/{leadId}/actividades")]
        public async Task<IActionResult> GetActividades(int leadId)
        {
            using var db = new MySqlConnection(_conn);
            var data = await db.QueryAsync<CrmActividad>(
                @"SELECT id AS Id, lead_id AS LeadId, usuario_id AS UsuarioId,
                         usuario_nombre AS UsuarioNombre, tipo AS Tipo,
                         titulo AS Titulo, descripcion AS Descripcion,
                         fecha_programada AS FechaProgramada, estado AS Estado,
                         fecha_realizada AS FechaRealizada
                  FROM crm_actividad WHERE lead_id = @leadId ORDER BY fecha_programada",
                new { leadId });
            return Ok(data);
        }

        /// <summary>
        /// Registra una actividad de seguimiento.
        /// Regla: fecha_programada es obligatoria.
        /// </summary>
        [HttpPost("actividades")]
        public async Task<IActionResult> CrearActividad([FromBody] CrmActividad dto)
        {
            if (dto.LeadId <= 0) return BadRequest("Debe indicar el lead.");
            if (string.IsNullOrWhiteSpace(dto.Titulo)) return BadRequest("El título es obligatorio.");
            if (dto.FechaProgramada == default) return BadRequest("La fecha programada es obligatoria.");
            if (string.IsNullOrWhiteSpace(dto.Tipo)) return BadRequest("El tipo es obligatorio (LLAMADA/CORREO/REUNION/TAREA).");

            using var db = new MySqlConnection(_conn);

            // Verificar que el lead existe y está activo
            var existe = await db.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM crm_lead WHERE id = @LeadId AND activo = 1", dto);
            if (existe == 0) return BadRequest("El lead no existe o está inactivo.");

            var id = await db.ExecuteScalarAsync<int>(@"
                INSERT INTO crm_actividad
                    (lead_id, usuario_id, usuario_nombre, tipo, titulo, descripcion, fecha_programada, estado)
                VALUES
                    (@LeadId, @UsuarioId, @UsuarioNombre, @Tipo, @Titulo, @Descripcion, @FechaProgramada, 'PENDIENTE');
                SELECT LAST_INSERT_ID();", dto);

            dto.Id = id;
            dto.Estado = "PENDIENTE";
            return Ok(dto);
        }

        /// <summary>
        /// Marca una actividad como REALIZADA.
        /// Regla: registra fecha_realizada = NOW().
        /// </summary>
        [HttpPut("actividades/{id}/realizar")]
        public async Task<IActionResult> RealizarActividad(int id)
        {
            using var db = new MySqlConnection(_conn);

            var actividad = await db.QueryFirstOrDefaultAsync<CrmActividad>(
                "SELECT id AS Id, estado AS Estado FROM crm_actividad WHERE id = @id", new { id });
            if (actividad is null) return NotFound("Actividad no encontrada.");
            if (actividad.Estado == "REALIZADA") return BadRequest("La actividad ya fue realizada.");

            await db.ExecuteAsync(
                "UPDATE crm_actividad SET estado = 'REALIZADA', fecha_realizada = NOW() WHERE id = @id",
                new { id });

            return Ok(new { mensaje = "Actividad marcada como REALIZADA.", fechaRealizada = DateTime.Now });
        }
    }

    // ════════════════════════════════════════════════════════════════
    //  DTOs auxiliares CRM
    // ════════════════════════════════════════════════════════════════
    public class ConvertirOportunidadDto
    {
        public decimal IngresoEsperado { get; set; }
        public DateTime? FechaCierre { get; set; }
        public int? SocioId { get; set; }
    }

    public class CambiarEtapaDto
    {
        public int NuevaEtapaId { get; set; }
    }

    public class MarcarGanadoDto
    {
        public int? PedidoId { get; set; }
    }

    public class MarcarPerdidoDto
    {
        public string MotivoPerdida { get; set; } = "";
    }
}