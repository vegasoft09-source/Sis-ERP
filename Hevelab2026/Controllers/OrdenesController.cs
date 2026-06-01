using Microsoft.AspNetCore.Mvc;
using Hevelab2026.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hevelab2026.Controllers
{
    public class OrdenesController : Controller
    {
        private static List<OrdenVenta> ordenes = new List<OrdenVenta>
        {
            new OrdenVenta { NumeroPedido = "S0010", FechaCreacion = new DateTime(2023, 10, 24), Cliente = "Juan Pérez", RazonSocial = "Construcciones S.A.C.", Ruc = "20123456789", Subtotal = 1200.00m, Total = 1416.00m, Estado = "BORRADOR", Moneda = "PEN", CondicionPago = "Contado", Vendedor = "Carlos Ruiz", Descuento = 0, Impuestos = 216, Notas = "Requiere confirmación" },
            new OrdenVenta { NumeroPedido = "S0011", FechaCreacion = new DateTime(2023, 10, 23), Cliente = "María García", RazonSocial = "Logística Global E.I.R.L.", Ruc = "20987654321", Subtotal = 2500.00m, Total = 2950.00m, Estado = "POR FACTURAR", Moneda = "PEN", CondicionPago = "Crédito 30 días", Vendedor = "Ana Gomez", Descuento = 0, Impuestos = 450, Notas = "Mercadería en tránsito" },
            new OrdenVenta { NumeroPedido = "S0012", FechaCreacion = new DateTime(2023, 10, 22), Cliente = "Roberto Gómez", RazonSocial = "Tecnología Avanzada S.A.", Ruc = "20556677889", Subtotal = 850.00m, Total = 1003.00m, Estado = "FACTURADO POR COMPLETO", Moneda = "USD", CondicionPago = "Contado", Vendedor = "Luis Torres", Descuento = 50, Impuestos = 153, Notas = "Entregado" },
            new OrdenVenta { NumeroPedido = "S0013", FechaCreacion = new DateTime(2023, 10, 21), Cliente = "Elena Ruiz", RazonSocial = "Inversiones del Sur", Ruc = "20443322110", Subtotal = 3200.00m, Total = 3776.00m, Estado = "BORRADOR", Moneda = "PEN", CondicionPago = "Crédito 15 días", Vendedor = "Ana Gomez", Descuento = 200, Impuestos = 576, Notas = "Revisar stock" },
            new OrdenVenta { NumeroPedido = "S0014", FechaCreacion = new DateTime(2023, 10, 20), Cliente = "Fernando Torres", RazonSocial = "Suministros Industriales", Ruc = "20667788900", Subtotal = 1800.00m, Total = 2124.00m, Estado = "POR FACTURAR", Moneda = "USD", CondicionPago = "Contado", Vendedor = "Carlos Ruiz", Descuento = 0, Impuestos = 324, Notas = "Prioridad Alta" },
            new OrdenVenta { NumeroPedido = "S0015", FechaCreacion = new DateTime(2023, 10, 25), Cliente = "Sofía Vargas", RazonSocial = "Vargas Import SAC", Ruc = "20112233441", Subtotal = 4500.00m, Total = 5310.00m, Estado = "FACTURADO POR COMPLETO", Moneda = "PEN", CondicionPago = "Crédito 60 días", Vendedor = "Luis Torres", Descuento = 500, Impuestos = 810, Notas = "Proyecto A" },
            new OrdenVenta { NumeroPedido = "S0016", FechaCreacion = new DateTime(2023, 10, 26), Cliente = "Ricardo Palma", RazonSocial = "Letras y Servicios EIRL", Ruc = "20223344552", Subtotal = 700.00m, Total = 826.00m, Estado = "BORRADOR", Moneda = "PEN", CondicionPago = "Contado", Vendedor = "Ana Gomez", Descuento = 0, Impuestos = 126, Notas = "Cliente nuevo" },
            new OrdenVenta { NumeroPedido = "S0017", FechaCreacion = new DateTime(2023, 10, 27), Cliente = "Andrea Luna", RazonSocial = "Luna & Soluciones", Ruc = "20334455663", Subtotal = 1200.00m, Total = 1416.00m, Estado = "POR FACTURAR", Moneda = "USD", CondicionPago = "Contado", Vendedor = "Carlos Ruiz", Descuento = 100, Impuestos = 216, Notas = "" },
            new OrdenVenta { NumeroPedido = "S0018", FechaCreacion = new DateTime(2023, 10, 28), Cliente = "Miguel Flores", RazonSocial = "Flores Hnos S.A.", Ruc = "20445566774", Subtotal = 5200.00m, Total = 6136.00m, Estado = "FACTURADO POR COMPLETO", Moneda = "PEN", CondicionPago = "Crédito 30 días", Vendedor = "Luis Torres", Descuento = 0, Impuestos = 936, Notas = "Renovación" },
            new OrdenVenta { NumeroPedido = "S0019", FechaCreacion = new DateTime(2023, 10, 29), Cliente = "Patricia Robles", RazonSocial = "Agroexportadora Robles", Ruc = "20556677885", Subtotal = 9500.00m, Total = 11210.00m, Estado = "POR FACTURAR", Moneda = "USD", CondicionPago = "Crédito 90 días", Vendedor = "Ana Gomez", Descuento = 1000, Impuestos = 1710, Notas = "Campaña navideña" }
        };

        public IActionResult Index(string fecha, string busqueda, string estado)
        {
            var listaFiltrada = ordenes.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(fecha))
            {
                var partesFecha = fecha.Split(" - ");
                if (partesFecha.Length == 2 && DateTime.TryParse(partesFecha[0], out DateTime desde) && DateTime.TryParse(partesFecha[1], out DateTime hasta))
                {
                    listaFiltrada = listaFiltrada.Where(x => x.FechaCreacion.Date >= desde.Date && x.FechaCreacion.Date <= hasta.Date);
                }
                else if (DateTime.TryParse(fecha, out DateTime fechaFiltro))
                {
                    listaFiltrada = listaFiltrada.Where(x => x.FechaCreacion.Date == fechaFiltro.Date);
                }
            }

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                listaFiltrada = listaFiltrada.Where(x =>
                    x.Cliente.Contains(busqueda, StringComparison.OrdinalIgnoreCase) ||
                    x.RazonSocial.Contains(busqueda, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(estado) && estado != "TODOS")
            {
                listaFiltrada = listaFiltrada.Where(x =>
                    x.Estado == estado);
            }

            ViewBag.Fecha = fecha;
            ViewBag.Busqueda = busqueda;
            ViewBag.Estado = estado;

            return View(listaFiltrada.ToList());
        }

        public IActionResult Details(string id)
        {
            var orden = ordenes.FirstOrDefault(x => x.NumeroPedido == id);

            if (orden == null)
            {
                return NotFound();
            }

            return View(orden);
        }

        public IActionResult Create()
        {
            var modelo = new OrdenVenta();
            modelo.FechaCreacion = DateTime.Now;
            
            // Si viene de convertir una cotización
            if (TempData["CotizacionIdOrigen"] != null)
            {
                modelo.Cliente = TempData["Cliente"] as string ?? "";
                modelo.RazonSocial = TempData["RazonSocial"] as string ?? "";
                modelo.Ruc = TempData["Ruc"] as string ?? "";
                modelo.Moneda = TempData["Moneda"] as string ?? "PEN";
                modelo.CondicionPago = TempData["CondicionPago"] as string ?? "";
                modelo.Vendedor = TempData["Vendedor"] as string ?? "";
                
                decimal.TryParse(TempData["Descuento"]?.ToString(), out decimal desc);
                decimal.TryParse(TempData["Impuestos"]?.ToString(), out decimal imp);
                decimal.TryParse(TempData["Subtotal"]?.ToString(), out decimal sub);
                decimal.TryParse(TempData["Total"]?.ToString(), out decimal tot);
                
                modelo.Descuento = desc;
                modelo.Impuestos = imp;
                modelo.Subtotal = sub;
                modelo.Total = tot;
            }

            return View(modelo);
        }

        [HttpPost]
        public IActionResult Create(OrdenVenta orden)
        {
            if (orden == null)
            {
                return BadRequest();
            }

            if (string.IsNullOrEmpty(orden.NumeroPedido))
            {
                int maxNum = 0;
                foreach(var o in ordenes)
                {
                    if (o.NumeroPedido.StartsWith("S") && int.TryParse(o.NumeroPedido.Substring(1), out int num))
                    {
                        if(num > maxNum) maxNum = num;
                    }
                }
                orden.NumeroPedido = $"S{(maxNum + 1).ToString("D4")}";
            }

            if (orden.FechaCreacion == DateTime.MinValue)
            {
                orden.FechaCreacion = DateTime.Now;
            }

            bool existe = ordenes.Any(x => x.NumeroPedido == orden.NumeroPedido);

            if (existe)
            {
                ModelState.AddModelError("NumeroPedido", "Ya existe una orden con ese número de pedido.");
                return View(orden);
            }

            ordenes.Add(orden);

            return RedirectToAction("Index");
        }

        public IActionResult Edit(string id)
        {
            var orden = ordenes.FirstOrDefault(x => x.NumeroPedido == id);

            if (orden == null)
            {
                return NotFound();
            }

            return View(orden);
        }

        [HttpPost]
        public IActionResult Edit(OrdenVenta orden)
        {
            var ordenExistente = ordenes.FirstOrDefault(x => x.NumeroPedido == orden.NumeroPedido);

            if (ordenExistente == null)
            {
                return NotFound();
            }

            ordenExistente.FechaCreacion = orden.FechaCreacion;
            ordenExistente.Cliente = orden.Cliente;
            ordenExistente.RazonSocial = orden.RazonSocial;
            ordenExistente.Ruc = orden.Ruc;
            ordenExistente.Subtotal = orden.Subtotal;
            ordenExistente.Total = orden.Total;
            ordenExistente.Estado = orden.Estado;
            ordenExistente.Moneda = orden.Moneda;
            ordenExistente.CondicionPago = orden.CondicionPago;
            ordenExistente.Vendedor = orden.Vendedor;
            ordenExistente.Descuento = orden.Descuento;
            ordenExistente.Impuestos = orden.Impuestos;
            ordenExistente.Notas = orden.Notas;

            return RedirectToAction("Index");
        }
    }
}