using System.Linq;
using Tienda.Models;

namespace Tienda.Services
{
    public static class TotalesService
    {
        public static decimal CalcularTotalVenta(Venta v)
            => v?.DetallesVenta?.Sum(d => d.Cantidad * d.PrecioUnitario) ?? 0m;
    }
}
