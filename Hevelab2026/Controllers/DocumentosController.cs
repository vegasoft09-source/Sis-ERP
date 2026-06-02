using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Hevelab2026.Models;

namespace Hevelab2026.Controllers
{
    public class DocumentosController : Controller
    {
        public IActionResult Index()
        {
            var documentos = new List<DocumentoContable>
            {
                new DocumentoContable { Id = 1, Nombre = "Factura de Ventas F001-0008542.pdf", Descripcion = "Factura emitida a Corporación Logística por servicios de consultoría.", TipoDocumento = "Factura", Categoria = "Ventas", FechaSubida = "2026-05-10", TamanoKB = 245, Extension = "pdf", SubidoPor = "Admin. Contable", Etiquetas = new[] { "Ventas", "Factura", "Mayo" }, Estado = "Activo" },
                new DocumentoContable { Id = 2, Nombre = "Conciliación Bancaria Mayo 2026.xlsx", Descripcion = "Reporte de conciliación de la cuenta corriente principal del BCP.", TipoDocumento = "Estado de Cuenta", Categoria = "Cierre", FechaSubida = "2026-05-24", TamanoKB = 1520, Extension = "xlsx", SubidoPor = "Luis Torres", Etiquetas = new[] { "BCP", "Conciliación", "Mayo" }, Estado = "Activo" },
                new DocumentoContable { Id = 3, Nombre = "Comprobante de Compras F301-0451298.xml", Descripcion = "XML SUNAT de factura de compras por adquisición de hardware de oficina.", TipoDocumento = "Factura", Categoria = "Compras", FechaSubida = "2026-05-02", TamanoKB = 45, Extension = "xml", SubidoPor = "Sistema Contable", Etiquetas = new[] { "Compras", "SUNAT", "XML" }, Estado = "Activo" },
                new DocumentoContable { Id = 4, Nombre = "Declaración PDT 621 Mayo 2026.pdf", Descripcion = "Formulario virtual 621 presentado ante SUNAT correspondiente al periodo Mayo.", TipoDocumento = "Declaración", Categoria = "Impuestos", FechaSubida = "2026-05-18", TamanoKB = 412, Extension = "pdf", SubidoPor = "Luis Torres", Etiquetas = new[] { "SUNAT", "PDT621", "Impuestos" }, Estado = "Activo" },
                new DocumentoContable { Id = 5, Nombre = "Contrato de Alquiler Oficina Administrativa.pdf", Descripcion = "Copia legalizada del contrato de arrendamiento de la oficina central.", TipoDocumento = "Contrato", Categoria = "Cierre", FechaSubida = "2026-01-15", TamanoKB = 3450, Extension = "pdf", SubidoPor = "Legal Contable", Etiquetas = new[] { "Contrato", "Oficinas", "Legal" }, Estado = "Activo" },
                new DocumentoContable { Id = 6, Nombre = "Planilla de Sueldos Mayo 2026.pdf", Descripcion = "Detalle de remuneraciones y aportes de ley del periodo actual.", TipoDocumento = "Reporte", Categoria = "Cierre", FechaSubida = "2026-05-25", TamanoKB = 890, Extension = "pdf", SubidoPor = "Recursos Humanos", Etiquetas = new[] { "Planilla", "Sueldos", "Cierre" }, Estado = "Activo" },
                new DocumentoContable { Id = 7, Nombre = "Reporte Inventarios Fisicos Cierre.xlsx", Descripcion = "Kardex valorizado conciliado tras inventario físico de almacenes.", TipoDocumento = "Reporte", Categoria = "Compras", FechaSubida = "2026-05-20", TamanoKB = 2105, Extension = "xlsx", SubidoPor = "Supervisor Almacén", Etiquetas = new[] { "Inventario", "Reporte", "Kardex" }, Estado = "Activo" },
                new DocumentoContable { Id = 8, Nombre = "Factura Proveedor Luz del Sur F402-0984512.pdf", Descripcion = "Comprobante de servicios de energía eléctrica de la sede central.", TipoDocumento = "Factura", Categoria = "Compras", FechaSubida = "2026-05-04", TamanoKB = 185, Extension = "pdf", SubidoPor = "Mesa de Partes", Etiquetas = new[] { "Servicios", "Luz", "Compras" }, Estado = "Activo" },
                new DocumentoContable { Id = 9, Nombre = "Boleta de Ventas Electronica B001-0000421.xml", Descripcion = "Comprobante XML SUNAT emitido por venta menor de mercaderías.", TipoDocumento = "Factura", Categoria = "Ventas", FechaSubida = "2026-05-12", TamanoKB = 35, Extension = "xml", SubidoPor = "Caja Facturación", Etiquetas = new[] { "Ventas", "Boleta", "XML" }, Estado = "Activo" },
                new DocumentoContable { Id = 10, Nombre = "Balance de Comprobación Mayo 2026.xlsx", Descripcion = "Balance preliminar de saldos y cuentas de contabilidad analítica.", TipoDocumento = "Reporte", Categoria = "Cierre", FechaSubida = "2026-05-26", TamanoKB = 920, Extension = "xlsx", SubidoPor = "Luis Torres", Etiquetas = new[] { "Balance", "Mayo", "Cierre" }, Estado = "Activo" },
                new DocumentoContable { Id = 11, Nombre = "Declaracion Anual Impuesto Renta 2025.pdf", Descripcion = "Formulario virtual presentativo anual para el ejercicio gravable anterior.", TipoDocumento = "Declaración", Categoria = "Impuestos", FechaSubida = "2026-03-28", TamanoKB = 1880, Extension = "pdf", SubidoPor = "Gerente Contabilidad", Etiquetas = new[] { "Anual", "Impuestos", "SUNAT" }, Estado = "Activo" },
                new DocumentoContable { Id = 12, Nombre = "Acta de Directorio Cierre de Ejercicio.pdf", Descripcion = "Acta firmada aprobando estados financieros y distribución de dividendos.", TipoDocumento = "Contrato", Categoria = "Cierre", FechaSubida = "2026-03-12", TamanoKB = 1205, Extension = "pdf", SubidoPor = "Gerente General", Etiquetas = new[] { "Acta", "Directorio", "Legal" }, Estado = "Activo" },
                new DocumentoContable { Id = 13, Nombre = "Factura de Ventas F001-0008543.pdf", Descripcion = "Factura duplicada o anulada correspondiente al mes de Abril.", TipoDocumento = "Factura", Categoria = "Ventas", FechaSubida = "2026-04-15", TamanoKB = 230, Extension = "pdf", SubidoPor = "Luis Torres", Etiquetas = new[] { "Ventas", "Archivado", "Anulado" }, Estado = "Archivado" },
                new DocumentoContable { Id = 14, Nombre = "Conciliación Bancaria Abril 2026.xlsx", Descripcion = "Reporte final y aprobado de conciliaciones bancarias del mes de Abril.", TipoDocumento = "Estado de Cuenta", Categoria = "Cierre", FechaSubida = "2026-04-30", TamanoKB = 1450, Extension = "xlsx", SubidoPor = "Gerente Contabilidad", Etiquetas = new[] { "BCP", "Conciliación", "Archivado" }, Estado = "Archivado" }
            };

            return View(documentos);
        }
    }
}
