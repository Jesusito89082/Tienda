using System.Collections.Generic;
using Tienda.Models;

namespace Tienda.ViewModels
{
    public class CheckoutViewModel
    {
        // === CLIENTE EXISTENTE ===
        public int? ClienteId { get; set; }


        // === NUEVO CLIENTE ===
        public string? NuevoNombre { get; set; }
        public string? NuevoEmail { get; set; }
        public string? NuevoTelefono { get; set; }
        public string? NuevaDireccion { get; set; }


        // === DESCUENTO ===
        // Porcentaje (ej: 10 = 10%)
        public decimal DescuentoPorcentaje { get; set; }


        // === TOTALES ===
        public decimal Subtotal { get; set; }
        public decimal DescuentoMonto { get; set; }
        public decimal Impuesto { get; set; }
        public decimal Total { get; set; }


        // === LISTADO ===
        public List<CarritoItem> Items { get; set; } = new();
        public List<Cliente> Clientes { get; set; } = new();
    }
}
