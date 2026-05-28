using Microsoft.AspNetCore.Mvc;
using Hevelab2026.Models;
using System;
using System.Collections.Generic;

namespace Hevelab2026.Controllers
{
    public class RecepcionesViewController : Controller
    {
        // GET: /RecepcionesView/Index
        public IActionResult Index()
        {
            var listaRecepciones = new List<RecepcionModel>
            {
                new RecepcionModel { Referencia = "WH/IN/00124", Desde = "Almacén Central", A = "Sede Norte", Contacto = "Juan Pérez", FechaProgramada = new DateTime(2023, 10, 24, 10, 30, 0), DocumentoOrigen = "OC-2023-089", Estado = "BORRADOR" },
                new RecepcionModel { Referencia = "WH/IN/00125", Desde = "Proveedor Logístico", A = "Almacén Principal", Contacto = "María García", FechaProgramada = new DateTime(2023, 10, 23, 15, 45, 0), DocumentoOrigen = "OC-2023-092", Estado = "ENVIADO" }
            };
            return View(listaRecepciones);
        }

        // 🛠️ NUEVA ACCIÓN: GET /RecepcionesView/Crear
        [HttpGet]
        public IActionResult Crear()
        {
            // Inicializamos un modelo vacío listo para rellenar en el formulario
            var nuevoModelo = new RecepcionModel
            {
                Referencia = "REQ-NUEVO", // Se puede autogenerar en base de datos después
                FechaProgramada = DateTime.Now,
                FechaLimite = DateTime.Now.AddDays(2)
            };
            return View(nuevoModelo);
        }

        // 🛠️ NUEVA ACCIÓN: POST /RecepcionesView/Crear (Guardar)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Crear(RecepcionModel modelo)
        {
            if (ModelState.IsValid)
            {
                // Aquí guardarías el objeto en tu base de datos con Entity Framework
                // _context.Recepciones.Add(modelo);
                // _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return View(modelo);
        }

        // GET: /RecepcionesView/Detalle?id=WH/IN/00124
        [HttpGet]
        public IActionResult Detalle(string id)
        {
            var detalleRecepcion = new RecepcionModel
            {
                Referencia = string.IsNullOrEmpty(id) ? "ALM01/IN/00010" : id,
                Desde = "AC DC SUPPLY",
                TipoOperacion = "ALMACEN INTERWORLD: Recepciones",
                UbicacionDestino = "ALM01/Existencias",
                FechaProgramada = new DateTime(2026, 2, 13, 14, 46, 0),
                DocumentoOrigen = "P00002",
                Estado = "LISTO",
                Detalles = new List<RecepcionDetalleModel>
                {
                    new RecepcionDetalleModel { CodigoProducto = "E-COM11", NombreProducto = "Cable Ethernet 10m", DescripcionProducto = "Cable de red blindado", Demanda = 150.00m, Cantidad = 0.00m }
                }
            };
            return View(detalleRecepcion);
        }
    }
}