using Hevelab2026.Models;
using Microsoft.AspNetCore.Mvc;

namespace Hevelab2026.Controllers
{
    public class ProductoController : Controller
    {
        private static List<Producto> productos = new List<Producto>
        {
            new Producto
            {
                Sku = "PR-001",
                CodigoBarras = "7501234567890",
                Nombre = "Alcohol en Gel 1L 70%",
                Precio = 15.00m,
                Stock = 250,
                Unidad = "Botellas",
                Categoria = "Bienestar y Cuidado Personal",
                Marca = "Genérico",
                FechaCaducidad = new DateTime(2026, 12, 31),
                Lote = "LOTE-2026-01",
                Proveedor = "Proveedor A",
                Presentacion = "Botella 1L",
                PesoVolumen = "1L",
                UnidadMedida = "Litros",
                UbicacionAlmacen = "Pasillo 1 - Estante A2",
                StockMinimo = 20,
                Disponible = true,
                Descripcion = "Producto para higiene y desinfección."
            },
            new Producto
            {
                Sku = "PR-002",
                CodigoBarras = "7509876543210",
                Nombre = "Mascarillas KN95 Pack x 20",
                Precio = 45.00m,
                Stock = 1000,
                Unidad = "Packs",
                Categoria = "Productos Farmacéuticos y de Salud",
                Marca = "Medifarma",
                FechaCaducidad = new DateTime(2027, 6, 15),
                Lote = "LOTE-2027-02",
                Proveedor = "Proveedor B",
                Presentacion = "Pack x 20",
                PesoVolumen = "20 unidades",
                UnidadMedida = "Unidad",
                UbicacionAlmacen = "Pasillo 2 - Estante B1",
                StockMinimo = 100,
                Disponible = true,
                Descripcion = "Mascarillas de protección personal."
            },
            new Producto
            {
                Sku = "PR-003",
                CodigoBarras = "7501112223334",
                Nombre = "Termómetro Digital Infrarrojo",
                Precio = 125.00m,
                Stock = 15,
                Unidad = "Unidades",
                Categoria = "Productos Farmacéuticos y de Salud",
                Marca = "Genérico",
                FechaCaducidad = null,
                Lote = "LOTE-2026-03",
                Proveedor = "Proveedor C",
                Presentacion = "Unidad",
                PesoVolumen = "1 unidad",
                UnidadMedida = "Unidad",
                UbicacionAlmacen = "Pasillo 3 - Estante C1",
                StockMinimo = 5,
                Disponible = true,
                Descripcion = "Equipo para medición de temperatura."
            },
            new Producto
            {
                Sku = "PR-004",
                CodigoBarras = "7504445556667",
                Nombre = "Panadol Antigripal 100 Tabletas",
                Precio = 12.00m,
                Stock = -105,
                Unidad = "Unidades",
                Categoria = "Productos Farmacéuticos y de Salud",
                Marca = "Laboratorio Portugal",
                FechaCaducidad = new DateTime(2026, 9, 10),
                Lote = "LOTE-2026-04",
                Proveedor = "Proveedor A",
                Presentacion = "Caja x 100 tabletas",
                PesoVolumen = "100 tabletas",
                UnidadMedida = "Unidad",
                UbicacionAlmacen = "Pasillo 4 - Estante D3",
                StockMinimo = 30,
                Disponible = false,
                Descripcion = "Producto antigripal en tabletas."
            },
            new Producto
            {
                Sku = "PR-005",
                CodigoBarras = "7507778889991",
                Nombre = "Premium Skincare Kit",
                Precio = 249.00m,
                Stock = 5,
                Unidad = "Unidades",
                Categoria = "Bienestar y Cuidado Personal",
                Marca = "Genérico",
                FechaCaducidad = new DateTime(2027, 1, 20),
                Lote = "LOTE-2027-05",
                Proveedor = "Proveedor B",
                Presentacion = "Kit completo",
                PesoVolumen = "Varios productos",
                UnidadMedida = "Unidad",
                UbicacionAlmacen = "Pasillo 5 - Estante E1",
                StockMinimo = 3,
                Disponible = true,
                Descripcion = "Kit de cuidado personal."
            }
        };

        public IActionResult Index()
        {
            return View(productos);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Producto producto)
        {
            productos.Add(producto);

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Edit(string sku)
        {
            var producto = productos.FirstOrDefault(p => p.Sku == sku);

            if (producto == null)
            {
                return NotFound();
            }

            return View(producto);
        }

        [HttpPost]
        public IActionResult Edit(Producto productoEditado)
        {
            var producto = productos.FirstOrDefault(p => p.Sku == productoEditado.Sku);

            if (producto == null)
            {
                return NotFound();
            }

            producto.CodigoBarras = productoEditado.CodigoBarras;
            producto.Nombre = productoEditado.Nombre;
            producto.Precio = productoEditado.Precio;
            producto.Stock = productoEditado.Stock;
            producto.Unidad = productoEditado.Unidad;
            producto.Categoria = productoEditado.Categoria;
            producto.Marca = productoEditado.Marca;
            producto.FechaCaducidad = productoEditado.FechaCaducidad;
            producto.Lote = productoEditado.Lote;
            producto.Proveedor = productoEditado.Proveedor;
            producto.Presentacion = productoEditado.Presentacion;
            producto.PesoVolumen = productoEditado.PesoVolumen;
            producto.UnidadMedida = productoEditado.UnidadMedida;
            producto.UbicacionAlmacen = productoEditado.UbicacionAlmacen;
            producto.StockMinimo = productoEditado.StockMinimo;
            producto.Disponible = productoEditado.Disponible;
            producto.Descripcion = productoEditado.Descripcion;

            return RedirectToAction("Index");
        }
    }
}
