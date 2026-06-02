using Microsoft.AspNetCore.Mvc;
using Hevelab2026.Models;

namespace Hevelab2026.Controllers
{
    public class CotizacionesController : Controller
    {

        // LISTA GLOBAL
        private static List<Cotizacion> lista = new List<Cotizacion>()
        {
            new Cotizacion() { Id = 1, NumeroPedido = "COT-001", FechaCreacion = DateTime.Now.AddDays(-5), Cliente = "Juan Perez", RazonSocial = "Tecnologia SAC", RUC = "20123456789", Subtotal = 1000, Total = 1180, Estado = "Pendiente", Moneda = "PEN", CondicionPago = "Contado", Vendedor = "Carlos Ruiz", Descuento = 0, Impuestos = 180, Notas = "Entrega en 3 días" },
            new Cotizacion() { Id = 2, NumeroPedido = "COT-002", FechaCreacion = DateTime.Now.AddDays(-4), Cliente = "Maria Lopez", RazonSocial = "Importaciones Perú", RUC = "20987654321", Subtotal = 1500, Total = 1770, Estado = "Aprobado", Moneda = "PEN", CondicionPago = "Crédito 30 días", Vendedor = "Ana Gomez", Descuento = 0, Impuestos = 270, Notas = "Incluye instalación" },
            new Cotizacion() { Id = 3, NumeroPedido = "COT-003", FechaCreacion = DateTime.Now.AddDays(-3), Cliente = "Roberto Silva", RazonSocial = "Construcciones R&S", RUC = "20445566778", Subtotal = 3000, Total = 3540, Estado = "Rechazado", Moneda = "USD", CondicionPago = "Crédito 15 días", Vendedor = "Luis Torres", Descuento = 100, Impuestos = 540, Notas = "Cliente pidió rebaja extra" },
            new Cotizacion() { Id = 4, NumeroPedido = "COT-004", FechaCreacion = DateTime.Now.AddDays(-2), Cliente = "Elena Vargas", RazonSocial = "Vargas Consultores", RUC = "20601122334", Subtotal = 800, Total = 944, Estado = "Aprobado", Moneda = "PEN", CondicionPago = "Contado", Vendedor = "Carlos Ruiz", Descuento = 0, Impuestos = 144, Notas = "" },
            new Cotizacion() { Id = 5, NumeroPedido = "COT-005", FechaCreacion = DateTime.Now.AddDays(-1), Cliente = "Fernando Rios", RazonSocial = "Rios & Hnos", RUC = "20102030405", Subtotal = 4500, Total = 5310, Estado = "Pendiente", Moneda = "USD", CondicionPago = "Crédito 60 días", Vendedor = "Ana Gomez", Descuento = 200, Impuestos = 810, Notas = "Proyecto a largo plazo" },
            new Cotizacion() { Id = 6, NumeroPedido = "COT-006", FechaCreacion = DateTime.Now, Cliente = "Carla Ponce", RazonSocial = "Comercializadora CP", RUC = "20887766554", Subtotal = 1200, Total = 1416, Estado = "Pendiente", Moneda = "PEN", CondicionPago = "Contado", Vendedor = "Luis Torres", Descuento = 50, Impuestos = 216, Notas = "Urgente" },
            new Cotizacion() { Id = 7, NumeroPedido = "COT-007", FechaCreacion = DateTime.Now, Cliente = "Pedro Alva", RazonSocial = "Alva Inversiones", RUC = "20776655443", Subtotal = 550, Total = 649, Estado = "Aprobado", Moneda = "PEN", CondicionPago = "Contado", Vendedor = "Carlos Ruiz", Descuento = 0, Impuestos = 99, Notas = "" },
            new Cotizacion() { Id = 8, NumeroPedido = "COT-008", FechaCreacion = DateTime.Now.AddDays(-6), Cliente = "Sofia Luna", RazonSocial = "Luna Diseño y Moda", RUC = "20334455667", Subtotal = 2100, Total = 2478, Estado = "Rechazado", Moneda = "PEN", CondicionPago = "Crédito 30 días", Vendedor = "Ana Gomez", Descuento = 0, Impuestos = 378, Notas = "Cotización expirada" },
            new Cotizacion() { Id = 9, NumeroPedido = "COT-009", FechaCreacion = DateTime.Now.AddDays(-7), Cliente = "Jorge Martinez", RazonSocial = "Transportes J&M", RUC = "20998877665", Subtotal = 8000, Total = 9440, Estado = "Aprobado", Moneda = "USD", CondicionPago = "Crédito 45 días", Vendedor = "Luis Torres", Descuento = 500, Impuestos = 1440, Notas = "Aprobación sujeta a contrato" },
            new Cotizacion() { Id = 10, NumeroPedido = "COT-010", FechaCreacion = DateTime.Now, Cliente = "Mariana Diaz", RazonSocial = "Diaz Abogados", RUC = "20112233445", Subtotal = 300, Total = 354, Estado = "Pendiente", Moneda = "PEN", CondicionPago = "Contado", Vendedor = "Carlos Ruiz", Descuento = 0, Impuestos = 54, Notas = "Servicio exprés" }
        };

        // INDEX + FILTRO
        public IActionResult Index(string cliente, string estado)
        {
            var datos = lista.AsQueryable();

            // FILTRO CLIENTE
            if (!string.IsNullOrEmpty(cliente))
            {
                datos = datos.Where(x =>
                    x.Cliente.ToLower().Contains(cliente.ToLower()) ||
                    x.RazonSocial.ToLower().Contains(cliente.ToLower())
                );
            }

            // FILTRO ESTADO
            if (!string.IsNullOrEmpty(estado) &&
                estado != "Todos los estados")
            {
                datos = datos.Where(x => x.Estado == estado);
            }

            return View(datos.ToList());
        }

        // VER DETALLE
        public IActionResult Details(int id)
        {
            var cotizacion = lista.FirstOrDefault(x => x.Id == id);

            return View(cotizacion);
        }

        // FORMULARIO EDITAR
        public IActionResult Edit(int id)
        {
            var cotizacion = lista.FirstOrDefault(x => x.Id == id);

            return View(cotizacion);
        }

        // GUARDAR EDICION
        [HttpPost]
        public IActionResult Edit(Cotizacion model)
        {
            var cotizacion = lista.FirstOrDefault(x => x.Id == model.Id);

            if (cotizacion != null)
            {
                cotizacion.Cliente = model.Cliente;
                cotizacion.RazonSocial = model.RazonSocial;
                cotizacion.RUC = model.RUC;
                cotizacion.Subtotal = model.Subtotal;
                cotizacion.Total = model.Total;
                cotizacion.Estado = model.Estado;
                cotizacion.Moneda = model.Moneda;
                cotizacion.CondicionPago = model.CondicionPago;
                cotizacion.Vendedor = model.Vendedor;
                cotizacion.Descuento = model.Descuento;
                cotizacion.Impuestos = model.Impuestos;
                cotizacion.Notas = model.Notas;
            }

            return RedirectToAction("Index");
        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(Cotizacion model)
        {
            model.Id = lista.Max(x => x.Id) + 1;

            model.NumeroPedido = "COT-00" + model.Id;

            model.FechaCreacion = DateTime.Now;

            lista.Add(model);

            return RedirectToAction("Index");
        }

    }
}