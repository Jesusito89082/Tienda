using System;
using System.Collections.Generic;

namespace Tienda.Models;

public partial class Venta
{
    public int VentaId { get; set; }

    public int? ClienteId { get; set; }

    public DateTime? Fecha { get; set; }

    public decimal? Total { get; set; }

    public virtual Cliente? Cliente { get; set; }

    public virtual ICollection<DetallesVentum> DetallesVenta { get; set; } = new List<DetallesVentum>();
}
