using System;
using System.Collections.Generic;

namespace Tienda.Models;

public partial class Venta
{
    public int VentaId { get; set; }

    public int? ClienteId { get; set; }

    public DateTime? Fecha { get; set; }
    public decimal Subtotal { get; set; }          // Suma de líneas sin descuento ni impuesto
    public decimal Descuento { get; set; }         // Monto de descuento aplicado
    public decimal Impuesto { get; set; }          // IVA calculado sobre (Subtotal - Descuento)

  
    public decimal? Total { get; set; }

    public virtual Cliente? Cliente { get; set; }

    public virtual ICollection<DetallesVentum> DetallesVenta { get; set; } = new List<DetallesVentum>();
}
