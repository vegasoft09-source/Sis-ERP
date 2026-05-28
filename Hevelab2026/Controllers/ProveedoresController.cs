using Microsoft.AspNetCore.Mvc;
using Hevelab2026.Models;

namespace Hevelab2026.Controllers
{
    public class ProveedoresController : Controller
    {
        // Datos mock en memoria (reemplazar con BD luego)
        private static List<Proveedor> _proveedores = GetMockProveedores();

        public IActionResult Index(string search = "", string estado = "", int page = 1)
        {
            var proveedoresFiltrados = _proveedores.AsEnumerable();

            // Filtro por búsqueda (nombre, RUC, correo)
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower();
                proveedoresFiltrados = proveedoresFiltrados.Where(p =>
                    p.NombreRazonSocial.ToLower().Contains(search) ||
                    p.RUC.Contains(search) ||
                    p.CorreoElectronico.ToLower().Contains(search)
                );
            }

            // Filtro por estado
            if (!string.IsNullOrWhiteSpace(estado) && estado != "Todos")
            {
                proveedoresFiltrados = proveedoresFiltrados.Where(p => p.Estado == estado);
            }

            // Paginación
            int pageSize = 10;
            var total = proveedoresFiltrados.Count();
            var proveedoresPaginados = proveedoresFiltrados
                .OrderByDescending(p => p.FechaAlta)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Pasar datos a la vista
            ViewData["Proveedores"] = proveedoresPaginados;
            ViewData["TotalRegistros"] = total;
            ViewData["PaginaActual"] = page;
            ViewData["TotalPaginas"] = (total + pageSize - 1) / pageSize;
            ViewData["SearchTerm"] = search;
            ViewData["EstadoFiltro"] = estado;

            return View();
        }

        private static List<Proveedor> GetMockProveedores()
        {
            return new List<Proveedor>
            {
                new Proveedor { Id = 1, NombreRazonSocial = "Corporación Industrial S.A.", RUC = "20123456789", Direccion = "Av. Central 456, Lima", Telefono = "+51 987 654 321", CorreoElectronico = "contacto@corpindustrial.pe", Estado = "ACTIVO", FechaAlta = DateTime.Now.AddMonths(-6), Categoria = "Manufacturero" },
                new Proveedor { Id = 2, NombreRazonSocial = "Tech Solutions Inc.", RUC = "20987654321", Direccion = "Calle Principal 123, Lima", Telefono = "+51 912 345 678", CorreoElectronico = "info@techsolutions.pe", Estado = "ACTIVO", FechaAlta = DateTime.Now.AddMonths(-4), Categoria = "Distribuidor" },
                new Proveedor { Id = 3, NombreRazonSocial = "Global Manufacturing S.A.", RUC = "20456789123", Direccion = "Av. Industrial 789, Arequipa", Telefono = "+51 954 321 098", CorreoElectronico = "ventas@globalmanufacturing.com", Estado = "ACTIVO", FechaAlta = DateTime.Now.AddMonths(-8), Categoria = "Manufacturero" },
                new Proveedor { Id = 4, NombreRazonSocial = "Imports & Exports Ltd.", RUC = "20789123456", Direccion = "Zona Franca, Callao", Telefono = "+51 978 654 123", CorreoElectronico = "logistica@importsexports.com", Estado = "INACTIVO", FechaAlta = DateTime.Now.AddMonths(-12), Categoria = "Distribuidor" },
                new Proveedor { Id = 5, NombreRazonSocial = "Local Supplies Co.", RUC = "20654321987", Direccion = "Calle Comercio 456, Trujillo", Telefono = "+51 945 678 234", CorreoElectronico = "pedidos@localsupplies.pe", Estado = "ACTIVO", FechaAlta = DateTime.Now.AddMonths(-3), Categoria = "Distribuidor" },
                new Proveedor { Id = 6, NombreRazonSocial = "Premium Goods Trading", RUC = "20321654987", Direccion = "Av. Reforma 234, Lima", Telefono = "+51 923 456 789", CorreoElectronico = "contacto@premiumgoods.pe", Estado = "ACTIVO", FechaAlta = DateTime.Now.AddMonths(-2), Categoria = "Distribuidor" },
                new Proveedor { Id = 7, NombreRazonSocial = "Industrial Components Ltd.", RUC = "20876543210", Direccion = "Parque Industrial, Ica", Telefono = "+51 956 789 123", CorreoElectronico = "componentes@industrial.com", Estado = "ACTIVO", FechaAlta = DateTime.Now.AddMonths(-7), Categoria = "Manufacturero" },
                new Proveedor { Id = 8, NombreRazonSocial = "Warehouse Solutions S.A.", RUC = "20543210876", Direccion = "Centro Logístico, Lima", Telefono = "+51 932 123 456", CorreoElectronico = "almacenes@warehouse.pe", Estado = "INACTIVO", FechaAlta = DateTime.Now.AddMonths(-10), Categoria = "Distribuidor" },
                new Proveedor { Id = 9, NombreRazonSocial = "Quality Materials Inc.", RUC = "20210987654", Direccion = "Km 25 Panamericana, Lima", Telefono = "+51 967 234 567", CorreoElectronico = "calidad@qualitymaterials.com", Estado = "ACTIVO", FechaAlta = DateTime.Now.AddMonths(-5), Categoria = "Manufacturero" },
                new Proveedor { Id = 10, NombreRazonSocial = "Express Delivery Corp.", RUC = "20098765432", Direccion = "Av. Logística 999, Callao", Telefono = "+51 989 876 543", CorreoElectronico = "entregas@expressdelivery.pe", Estado = "ACTIVO", FechaAlta = DateTime.Now.AddMonths(-1), Categoria = "Distribuidor" },
                new Proveedor { Id = 11, NombreRazonSocial = "Advanced Systems Ltd.", RUC = "20765432109", Direccion = "Av. Tecnológica 345, Lima", Telefono = "+51 941 567 890", CorreoElectronico = "soporte@advancedsystems.com", Estado = "ACTIVO", FechaAlta = DateTime.Now.AddMonths(-9), Categoria = "Manufacturero" },
                new Proveedor { Id = 12, NombreRazonSocial = "Trade Partners Group", RUC = "20432109876", Direccion = "Calle Exportación 567, Lima", Telefono = "+51 912 890 123", CorreoElectronico = "asociados@tradepartners.pe", Estado = "ACTIVO", FechaAlta = DateTime.Now.AddMonths(-11), Categoria = "Distribuidor" }
            };
        }
    }
}
