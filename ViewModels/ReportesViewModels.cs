// Carpeta: ViewModels/ReportesViewModels.cs
using System;
using System.Collections.Generic;

namespace Tienda.ViewModels
{
    public class ReporteVentasItemViewModel
    {
        public DateTime Fecha { get; set; }
        public int CantidadVentas { get; set; }
        public decimal MontoTotal { get; set; }
    }

    public class ReporteVentasViewModel
    {
        public string? Titulo { get; set; }
        public string? Periodo { get; set; } // "diario", "semanal", "mensual"
        public List<ReporteVentasItemViewModel> Items { get; set; } = new();
    }

    public class InventarioProductoViewModel
    {
        public int ProductoId { get; set; }
        public string? Nombre { get; set; }
        public int Stock { get; set; }
        public decimal Precio { get; set; }
    }

    public class ProductoMasVendidoViewModel
    {
        public int ProductoId { get; set; }
        public string? Nombre { get; set; }
        public int CantidadVendida { get; set; }
        public decimal TotalVendido { get; set; }
    }
}
