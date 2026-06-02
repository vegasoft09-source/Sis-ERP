using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Hevelab2026.Models;

namespace Hevelab2026.Controllers
{
    public class ClientesController : Controller
    {
        // Lista estática en memoria que simula la base de datos
        private static readonly List<Cliente> Clientes = new List<Cliente>
        {
            new Cliente
            {
                Id = 1,
                Codigo = "CLI-001",
                RazonSocial = "Corporación Aceros SAC",
                TipoDocumento = "RUC",
                NumeroDocumento = "20512345678",
                TipoCliente = "Persona Jurídica",
                Telefono = "987654321",
                Email = "contacto@aceros.com",
                Direccion = "Av. Argentina 1200",
                Ciudad = "Lima",
                LimiteCredito = 45000m,
                Activo = true,
                FechaRegistro = new DateTime(2023, 5, 12)
            },
            new Cliente
            {
                Id = 2,
                Codigo = "CLI-002",
                RazonSocial = "Juan Pérez Soluciones",
                TipoDocumento = "DNI",
                NumeroDocumento = "45678912",
                TipoCliente = "Persona Natural",
                Telefono = "912345678",
                Email = "juan.perez@gmail.com",
                Direccion = "Calle Las Flores 456",
                Ciudad = "Arequipa",
                LimiteCredito = 15000m,
                Activo = true,
                FechaRegistro = new DateTime(2024, 2, 20)
            },
            new Cliente
            {
                Id = 3,
                Codigo = "CLI-003",
                RazonSocial = "Constructora del Sur E.I.R.L.",
                TipoDocumento = "RUC",
                NumeroDocumento = "20601234567",
                TipoCliente = "Persona Jurídica",
                Telefono = "955667788",
                Email = "ventas@constructorasur.pe",
                Direccion = "Urb. La Angostura B-12",
                Ciudad = "Cusco",
                LimiteCredito = 50000m,
                Activo = true,
                FechaRegistro = new DateTime(2023, 11, 5)
            },
            new Cliente
            {
                Id = 4,
                Codigo = "CLI-004",
                RazonSocial = "Maria Gomez Garcia",
                TipoDocumento = "DNI",
                NumeroDocumento = "70123456",
                TipoCliente = "Persona Natural",
                Telefono = "944883322",
                Email = "maria.gomez@hotmail.com",
                Direccion = "Av. El Sol 789",
                Ciudad = "Trujillo",
                LimiteCredito = 5000m,
                Activo = false,
                FechaRegistro = new DateTime(2024, 8, 15)
            },
            new Cliente
            {
                Id = 5,
                Codigo = "CLI-005",
                RazonSocial = "Inversiones El Pacífico S.A.",
                TipoDocumento = "RUC",
                NumeroDocumento = "20405060708",
                TipoCliente = "Persona Jurídica",
                Telefono = "933221100",
                Email = "info@inversionespacifico.com",
                Direccion = "Av. Larco 450",
                Ciudad = "Lima",
                LimiteCredito = 30000m,
                Activo = true,
                FechaRegistro = new DateTime(2025, 1, 10)
            },
            new Cliente
            {
                Id = 6,
                Codigo = "CLI-006",
                RazonSocial = "Carlos Smith (Extranjero)",
                TipoDocumento = "CE",
                NumeroDocumento = "00124578",
                TipoCliente = "Persona Natural",
                Telefono = "999888777",
                Email = "carlos.smith@outlook.com",
                Direccion = "Av. Pardo 1020",
                Ciudad = "Lima",
                LimiteCredito = 25000m,
                Activo = true,
                FechaRegistro = new DateTime(2023, 9, 18)
            },
            new Cliente
            {
                Id = 7,
                Codigo = "CLI-007",
                RazonSocial = "Tecnología Avanzada S.A.C.",
                TipoDocumento = "RUC",
                NumeroDocumento = "20102030405",
                TipoCliente = "Persona Jurídica",
                Telefono = "911222333",
                Email = "soporte@tecavanzada.com",
                Direccion = "Jr. Junin 320",
                Ciudad = "Piura",
                LimiteCredito = 40000m,
                Activo = true,
                FechaRegistro = new DateTime(2024, 12, 1)
            },
            new Cliente
            {
                Id = 8,
                Codigo = "CLI-008",
                RazonSocial = "Ana Rodriguez Lopez",
                TipoDocumento = "DNI",
                NumeroDocumento = "41235678",
                TipoCliente = "Persona Natural",
                Telefono = "922334455",
                Email = "ana.rodriguez@yahoo.com",
                Direccion = "Calle Mercaderes 115",
                Ciudad = "Arequipa",
                LimiteCredito = 8000m,
                Activo = false,
                FechaRegistro = new DateTime(2023, 4, 30)
            },
            new Cliente
            {
                Id = 9,
                Codigo = "CLI-009",
                RazonSocial = "Distribuidora del Norte S.R.L.",
                TipoDocumento = "RUC",
                NumeroDocumento = "20304050607",
                TipoCliente = "Persona Jurídica",
                Telefono = "988776655",
                Email = "logistica@distribuidoranorte.com",
                Direccion = "Av. Mansiche 1500",
                Ciudad = "Trujillo",
                LimiteCredito = 35000m,
                Activo = true,
                FechaRegistro = new DateTime(2024, 6, 25)
            },
            new Cliente
            {
                Id = 10,
                Codigo = "CLI-010",
                RazonSocial = "Hans Müller (Turista)",
                TipoDocumento = "Pasaporte",
                NumeroDocumento = "N8877665",
                TipoCliente = "Persona Natural",
                Telefono = "977665544",
                Email = "hans.muller@web.de",
                Direccion = "Hotel Plaza de Armas",
                Ciudad = "Cusco",
                LimiteCredito = 0m,
                Activo = true,
                FechaRegistro = new DateTime(2025, 3, 14)
            },
            new Cliente
            {
                Id = 11,
                Codigo = "CLI-011",
                RazonSocial = "Pedro Infante Diaz",
                TipoDocumento = "DNI",
                NumeroDocumento = "48965412",
                TipoCliente = "Persona Natural",
                Telefono = "966554433",
                Email = "pedro.infante@outlook.com",
                Direccion = "Av. Grau 410",
                Ciudad = "Piura",
                LimiteCredito = 12000m,
                Activo = true,
                FechaRegistro = new DateTime(2024, 10, 8)
            },
            new Cliente
            {
                Id = 12,
                Codigo = "CLI-012",
                RazonSocial = "Consultores Asociados del Centro",
                TipoDocumento = "RUC",
                NumeroDocumento = "20204060809",
                TipoCliente = "Persona Jurídica",
                Telefono = "955443322",
                Email = "director@consultorescentro.com",
                Direccion = "Av. Giraldez 300",
                Ciudad = "Lima",
                LimiteCredito = 20000m,
                Activo = false,
                FechaRegistro = new DateTime(2023, 8, 22)
            }
        };

        // GET: /Clientes
        public IActionResult Index(string? busqueda, string? estado)
        {
            var query = Clientes.AsEnumerable();

            // Filtro por búsqueda (RazonSocial o NumeroDocumento, case-insensitive)
            if (!string.IsNullOrEmpty(busqueda))
            {
                string searchLower = busqueda.ToLower().Trim();
                query = query.Where(c => c.RazonSocial.ToLower().Contains(searchLower) || 
                                         c.NumeroDocumento.ToLower().Contains(searchLower));
            }

            // Filtro por estado
            if (!string.IsNullOrEmpty(estado))
            {
                if (estado.Equals("activo", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(c => c.Activo);
                }
                else if (estado.Equals("inactivo", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(c => !c.Activo);
                }
            }

            ViewBag.Clientes = query.ToList();
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

        // GET: /Clientes/Detalle/{id}
        public IActionResult Detalle(int id)
        {
            var cliente = Clientes.FirstOrDefault(c => c.Id == id);
            if (cliente == null)
            {
                return NotFound();
            }

            ViewBag.Cliente = cliente;
            ViewData["Title"] = cliente.RazonSocial;
            ViewData["Breadcrumbs"] = new List<(string Label, string Url)>
            {
                ("Dashboard", "/"),
                ("Ventas", "#"),
                ("Clientes", "/Clientes"),
                (cliente.RazonSocial, "")
            };

            return View();
        }

        // GET: /Clientes/Formulario/{id?}
        public IActionResult Formulario(int? id)
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
                var cliente = Clientes.FirstOrDefault(c => c.Id == id.Value);
                if (cliente == null)
                {
                    return NotFound();
                }

                ViewBag.Modo = "Editar";
                ViewBag.Cliente = cliente;
                ViewData["Title"] = "Editar Cliente";
                ViewData["Breadcrumbs"] = new List<(string Label, string Url)>
                {
                    ("Dashboard", "/"),
                    ("Ventas", "#"),
                    ("Clientes", "/Clientes"),
                    ($"Editar: {cliente.RazonSocial}", "")
                };
            }

            return View();
        }

        // POST: /Clientes/GuardarCliente
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult GuardarCliente(Cliente cliente, int? id)
        {
            if (id == null)
            {
                // Modo Crear
                int nuevoId = Clientes.Any() ? Clientes.Max(c => c.Id) + 1 : 1;
                cliente.Id = nuevoId;
                cliente.Codigo = $"CLI-{nuevoId:D3}";
                cliente.FechaRegistro = DateTime.Now;
                cliente.Activo = true; // Por defecto activo al crear
                Clientes.Add(cliente);
                return RedirectToAction(nameof(Index));
            }
            else
            {
                // Modo Editar
                var clienteExistente = Clientes.FirstOrDefault(c => c.Id == id.Value);
                if (clienteExistente == null)
                {
                    return NotFound();
                }

                clienteExistente.RazonSocial = cliente.RazonSocial;
                clienteExistente.TipoCliente = cliente.TipoCliente;
                clienteExistente.TipoDocumento = cliente.TipoDocumento;
                clienteExistente.NumeroDocumento = cliente.NumeroDocumento;
                clienteExistente.Telefono = cliente.Telefono;
                clienteExistente.Email = cliente.Email;
                clienteExistente.Direccion = cliente.Direccion;
                clienteExistente.Ciudad = cliente.Ciudad;
                clienteExistente.LimiteCredito = cliente.LimiteCredito;
                clienteExistente.Activo = cliente.Activo;

                return RedirectToAction(nameof(Detalle), new { id = clienteExistente.Id });
            }
        }

        // POST: /Clientes/ToggleEstado
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleEstado(int id)
        {
            var cliente = Clientes.FirstOrDefault(c => c.Id == id);
            if (cliente != null)
            {
                cliente.Activo = !cliente.Activo;
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
