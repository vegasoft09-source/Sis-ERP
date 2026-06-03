using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hevelab2026.Models;
using Dapper;
using MySqlConnector;

namespace Hevelab2026.Controllers
{
    public class DocumentosController : Controller
    {
        private readonly string _connectionString;

        public DocumentosController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("SisErp")
                ?? configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("No se encontró la cadena de conexión.");
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Breadcrumbs"] = new List<(string Label, string Url)>
            {
                ("Dashboard", "/"),
                ("Contabilidad", "#"),
                ("Documentos", "/Documentos")
            };

            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            const string sql = @"
                SELECT 
                    id, 
                    nombre, 
                    descripcion, 
                    tipo_documento AS TipoDocumento, 
                    categoria, 
                    DATE_FORMAT(fecha_subida, '%Y-%m-%d') AS FechaSubida, 
                    tamano_kb AS TamanoKB, 
                    extension, 
                    subido_por AS SubidoPor, 
                    etiquetas, 
                    estado 
                FROM documento_contable 
                ORDER BY id DESC;";

            var docsRaw = await conn.QueryAsync(sql);
            var documentos = docsRaw.Select(d => {
                var row = (IDictionary<string, object>)d;
                string tagsStr = row.TryGetValue("etiquetas", out var tagVal) ? tagVal?.ToString() ?? "" : "";
                string[] tagsArray = string.IsNullOrWhiteSpace(tagsStr)
                    ? Array.Empty<string>()
                    : tagsStr.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(t => t.Trim()).ToArray();

                return new DocumentoContable
                {
                    Id = row.TryGetValue("id", out var idVal) && idVal != null ? Convert.ToInt32(idVal) : 0,
                    Nombre = row.TryGetValue("nombre", out var nameVal) ? nameVal?.ToString() ?? "" : "",
                    Descripcion = row.TryGetValue("descripcion", out var descVal) ? descVal?.ToString() ?? "" : "",
                    TipoDocumento = row.TryGetValue("TipoDocumento", out var tdVal) ? tdVal?.ToString() ?? "" : "",
                    Categoria = row.TryGetValue("categoria", out var catVal) ? catVal?.ToString() ?? "" : "",
                    FechaSubida = row.TryGetValue("FechaSubida", out var fsVal) ? fsVal?.ToString() ?? "" : "",
                    TamanoKB = row.TryGetValue("TamanoKB", out var szVal) && szVal != null ? Convert.ToInt32(szVal) : 0,
                    Extension = row.TryGetValue("extension", out var extVal) ? extVal?.ToString() ?? "" : "",
                    SubidoPor = row.TryGetValue("SubidoPor", out var spVal) ? spVal?.ToString() ?? "" : "",
                    Etiquetas = tagsArray,
                    Estado = row.TryGetValue("estado", out var stVal) ? stVal?.ToString() ?? "Activo" : "Activo"
                };
            }).ToList();

            return View(documentos);
        }

        [HttpPost]
        public async Task<IActionResult> SubirDocumento(string nombre, string? descripcion, string tipoDocumento, string categoria, int tamanoKb, string extension, string? etiquetas)
        {
            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            const string sql = @"
                INSERT INTO documento_contable (nombre, descripcion, tipo_documento, categoria, fecha_subida, tamano_kb, extension, subido_por, etiquetas, estado)
                VALUES (@Nombre, @Descripcion, @TipoDoc, @Categoria, CURDATE(), @TamanoKB, @Extension, 'Usuario Admin', @Etiquetas, 'Activo');
                SELECT LAST_INSERT_ID();";

            string cleanDesc = string.IsNullOrWhiteSpace(descripcion) ? "Documento contable digital cargado." : descripcion;
            string cleanEtiquetas = etiquetas ?? "";

            int newId = await conn.ExecuteScalarAsync<int>(sql, new {
                Nombre = nombre,
                Descripcion = cleanDesc,
                TipoDoc = tipoDocumento,
                Categoria = categoria,
                TamanoKB = tamanoKb,
                Extension = extension,
                Etiquetas = cleanEtiquetas
            });

            return Json(new {
                id = newId,
                nombre = nombre,
                descripcion = cleanDesc,
                tipoDoc = tipoDocumento,
                categoria = categoria,
                fechaSubida = DateTime.Now.ToString("yyyy-MM-dd"),
                tamanoKB = tamanoKb,
                extension = extension,
                subidoPor = "Usuario Admin",
                etiquetas = string.IsNullOrWhiteSpace(cleanEtiquetas) ? Array.Empty<string>() : cleanEtiquetas.Split(',').Select(t => t.Trim()).ToArray(),
                estado = "Activo"
            });
        }

        [HttpPost]
        public async Task<IActionResult> ArchivarDocumento(int id)
        {
            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            const string selectSql = "SELECT estado FROM documento_contable WHERE id = @Id;";
            var currentEstado = await conn.QueryFirstOrDefaultAsync<string>(selectSql, new { Id = id });

            if (currentEstado == null)
            {
                return NotFound();
            }

            string nuevoEstado = currentEstado == "Activo" ? "Archivado" : "Activo";

            const string updateSql = "UPDATE documento_contable SET estado = @Estado WHERE id = @Id;";
            await conn.ExecuteAsync(updateSql, new { Estado = nuevoEstado, Id = id });

            return Json(new { success = true, nuevoEstado = nuevoEstado });
        }
    }
}
