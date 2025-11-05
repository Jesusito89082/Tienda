using System.ComponentModel.DataAnnotations;
using Tienda.Models;

namespace Tienda.Models
{
    public class Factura
    {
        [Key]
        public int FacturaId { get; set; }

        public int VentaId { get; set; }
        public Venta Venta { get; set; }

        public string NumeroConsecutivo { get; set; }
        public string Clave { get; set; } = string.Empty;
        public string XmlFirmado { get; set; }
        public string PdfPath { get; set; }
        public DateTime FechaEmision { get; set; } = DateTime.Now;
        public string Estado { get; set; } = "Pendiente";
    }
}