using Hevelab2026.Common;
using Hevelab2026.DTOs.Socios;
using Hevelab2026.Models;
using Hevelab2026.Services.Socios;
using Microsoft.AspNetCore.Mvc;

namespace Hevelab2026.Controllers
{
    public class ClientesController : Controller
    {
        private readonly ISocioService _socioService;

        public ClientesController(ISocioService socioService)
        {
            _socioService = socioService;
        }

        public async Task<IActionResult> Index(string? busqueda, string? estado, CancellationToken ct)
        {
            var query = new PagedQuery { Page = 1, PageSize = 500, Search = busqueda };
            var result = await _socioService.GetClientesAsync(query, estado, ct);

            ViewBag.Clientes = result.Items.Select(_socioService.ToViewModel).ToList();
            ViewBag.Busqueda = busqueda;
            ViewBag.Estado = estado;
            ViewData["Title"] = "Clientes";
            ViewData["Breadcrumbs"] = new List<(string Label, string Url)>
            {
                ("Dashboard", "/"),
                ("Ventas", "#"),
                ("Clientes", "/Clientes")
            };

            return View();
        }

        public async Task<IActionResult> Detalle(int id, CancellationToken ct)
        {
            var dto = await _socioService.GetByIdAsync(id, ct);
            if (dto == null) return NotFound();

            ViewBag.Cliente = _socioService.ToViewModel(dto);
            ViewData["Title"] = dto.RazonSocial;
            ViewData["Breadcrumbs"] = new List<(string Label, string Url)>
            {
                ("Dashboard", "/"),
                ("Ventas", "#"),
                ("Clientes", "/Clientes"),
                (dto.RazonSocial, "")
            };

            return View();
        }

        public async Task<IActionResult> Formulario(int? id, CancellationToken ct)
        {
            if (id == null)
            {
                ViewBag.Modo = "Crear";
                ViewBag.Cliente = null;
                ViewData["Title"] = "Nuevo Cliente";
                ViewData["Breadcrumbs"] = new List<(string Label, string Url)>
                {
                    ("Dashboard", "/"),
                    ("Ventas", "#"),
                    ("Clientes", "/Clientes"),
                    ("Nuevo Cliente", "")
                };
            }
            else
            {
                var dto = await _socioService.GetByIdAsync(id.Value, ct);
                if (dto == null) return NotFound();

                ViewBag.Modo = "Editar";
                ViewBag.Cliente = _socioService.ToViewModel(dto);
                ViewData["Title"] = "Editar Cliente";
                ViewData["Breadcrumbs"] = new List<(string Label, string Url)>
                {
                    ("Dashboard", "/"),
                    ("Ventas", "#"),
                    ("Clientes", "/Clientes"),
                    ($"Editar: {dto.RazonSocial}", "")
                };
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuardarCliente(Cliente cliente, int? id, CancellationToken ct)
        {
            if (id == null)
            {
                var createDto = new SocioCreateDto
                {
                    RazonSocial = cliente.RazonSocial,
                    TipoDocumento = cliente.TipoDocumento,
                    NumeroDocumento = cliente.NumeroDocumento,
                    Telefono = cliente.Telefono,
                    Email = cliente.Email,
                    Direccion = cliente.Direccion,
                    Ciudad = cliente.Ciudad,
                    LimiteCredito = cliente.LimiteCredito,
                    PuestoTrabajo = cliente.PuestoTrabajo,
                    GrupoClientes = cliente.GrupoClientes
                };
                await _socioService.CreateClienteAsync(createDto, ct: ct);
                return RedirectToAction(nameof(Index));
            }

            var updateDto = new SocioUpdateDto
            {
                RazonSocial = cliente.RazonSocial,
                TipoDocumento = cliente.TipoDocumento,
                NumeroDocumento = cliente.NumeroDocumento,
                Telefono = cliente.Telefono,
                Email = cliente.Email,
                Direccion = cliente.Direccion,
                Ciudad = cliente.Ciudad,
                LimiteCredito = cliente.LimiteCredito,
                PuestoTrabajo = cliente.PuestoTrabajo,
                GrupoClientes = cliente.GrupoClientes,
                Activo = cliente.Activo
            };
            var updated = await _socioService.UpdateClienteAsync(id.Value, updateDto, ct);
            return RedirectToAction(nameof(Detalle), new { id = updated.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleEstado(int id, CancellationToken ct)
        {
            await _socioService.ToggleActivoAsync(id, ct);
            return RedirectToAction(nameof(Index));
        }
    }
}
