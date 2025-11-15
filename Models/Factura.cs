namespace Tienda.Models
{
    public class Factura
    {
        public int FacturaId { get; set; }
        public int VentaId { get; set; }

        public string NumeroConsecutivo { get; set; } = string.Empty;
        public string Clave { get; set; } = string.Empty;
        public DateTime FechaEmision { get; set; }
        public string Estado { get; set; } = "Generada";
        public string? XmlFirmado { get; set; }
        public string? PdfPath { get; set; }

        public Venta? Venta { get; set; }
    }
}
