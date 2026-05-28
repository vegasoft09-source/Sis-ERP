using System;
using System.Collections.Generic;

namespace Hevelab2026.Models
{
    public class RecepcionModel
    {
        public string Referencia { get; set; } // Ej: WH/IN/00124
        public string Desde { get; set; }
        public string A { get; set; }
        public string Contacto { get; set; }
        public DateTime FechaProgramada { get; set; }
        public DateTime? FechaLimite { get; set; }
        public string DocumentoOrigen { get; set; }
        public string Estado { get; set; } // BORRADOR, ENVIADO, ACEPTADO, RECHAZADO
        public string TipoOperacion { get; set; }
        public string UbicacionDestino { get; set; }
        public string Nota { get; set; }

        // Relación con el detalle de productos
        public List<RecepcionDetalleModel> Detalles { get; set; } = new List<RecepcionDetalleModel>();
    }

    public class RecepcionDetalleModel
    {
        public string CodigoProducto { get; set; } // Ej: [E-COM11]
        public string NombreProducto { get; set; }
        public string DescripcionProducto { get; set; }
        public decimal Demanda { get; set; }
        public decimal Cantidad { get; set; }
    }
}