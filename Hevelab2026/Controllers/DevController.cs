using Microsoft.AspNetCore.Mvc;

namespace Hevelab2026.Controllers
{
    /// <summary>
    /// SOLO PARA DESARROLLO — genera hashes BCrypt.
    /// Accede a: /dev/hash?pwd=tuContraseña
    /// ELIMINAR o deshabilitar antes de pasar a producción.
    /// </summary>
    public class DevController : Controller
    {
        private readonly IWebHostEnvironment _env;

        public DevController(IWebHostEnvironment env)
        {
            _env = env;
        }

        [HttpGet("/dev/hash")]
        public IActionResult Hash(string? pwd)
        {
            if (!_env.IsDevelopment())
                return NotFound();

            if (string.IsNullOrWhiteSpace(pwd))
            {
                var form = "<h2>Generador de Hash BCrypt</h2>" +
                           "<p>Uso: <code>/dev/hash?pwd=tuContraseña</code></p>" +
                           "<form>" +
                           "<input name='pwd' placeholder='Contraseña' style='padding:8px;font-size:16px;width:300px' />" +
                           "<button type='submit' style='padding:8px 16px;margin-left:8px'>Generar</button>" +
                           "</form>";
                return Content(form, "text/html");
            }

            var hash = BCrypt.Net.BCrypt.HashPassword(pwd, workFactor: 11);

            var sql1 = $"UPDATE usuario SET contrasena = '{hash}' WHERE nombre_usuario = 'admin';";

            var html = "<!DOCTYPE html><html><head><title>Hash – Dev</title>" +
                       "<style>body{font-family:monospace;padding:2rem;background:#0d0f1a;color:#a5b4fc;}" +
                       ".box{background:#1e1b4b;padding:1.5rem;border-radius:8px;border:1px solid #4f46e5;}" +
                       ".hash{word-break:break-all;color:#34d399;font-size:1rem;margin:1rem 0;padding:1rem;" +
                       "background:#0f172a;border-radius:6px;}" +
                       ".sql{background:#0f172a;padding:1rem;border-radius:6px;color:#fbbf24;margin-top:1rem;}" +
                       "button{background:#4f46e5;color:white;border:none;padding:.5rem 1rem;" +
                       "border-radius:6px;cursor:pointer;margin-top:.5rem;}" +
                       "a{color:#818cf8;}</style></head><body>" +
                       "<div class='box'>" +
                       "<h2>🔐 Hash BCrypt generado</h2>" +
                       $"<p>Contraseña: <strong style='color:#f9a8d4'>'{pwd}'</strong></p>" +
                       $"<div class='hash' id='h'>{hash}</div>" +
                       "<button onclick=\"navigator.clipboard.writeText(document.getElementById('h').innerText)\">📋 Copiar hash</button>" +
                       "<div class='sql'><strong>SQL UPDATE listo:</strong><br/><br/>" +
                       $"<span id='sql'>{sql1}</span></div>" +
                       "<button onclick=\"navigator.clipboard.writeText(document.getElementById('sql').innerText)\" style='margin-top:.5rem'>📋 Copiar SQL</button>" +
                       "</div><br/><a href='/dev/hash'>← Generar otro</a>" +
                       "</body></html>";

            return Content(html, "text/html");
        }
    }
}
