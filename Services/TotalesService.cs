using System;
using System.Collections.Generic;
using System.Linq;
using Tienda.Models;

namespace Tienda.Services
{
    public class TotalesService
    {
        private const decimal IVA = 0.13m;

        public static void CalcularTotalesVenta(
            Venta venta,
            IEnumerable<DetallesVentum> detalles,
            decimal descuentoPorcentaje)
        {
            var lista = detalles.ToList();

            // 1. Subtotal sin impuestos ni descuentos
            var subtotal = lista.Sum(d => d.Cantidad * d.PrecioUnitario);

            // 2. Descuento (porcentaje sobre subtotal)
            var descuento = 0m;
            if (descuentoPorcentaje > 0)
            {
                descuento = Math.Round(subtotal * (descuentoPorcentaje / 100m), 2);
            }

            var baseGravable = subtotal - descuento;

            // 3. Impuesto (IVA) sobre la base gravable
            var impuesto = Math.Round(baseGravable * IVA, 2);

            // 4. Total final
            var total = baseGravable + impuesto;

            venta.Subtotal = subtotal;
            venta.Descuento = descuento;
            venta.Impuesto = impuesto;
            venta.Total = total;
        }
    }
}
