using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tienda.Data;
using Tienda.ViewModels;

namespace Tienda.Controllers
{
    [Authorize(Roles = "ADMINISTRADOR")]
    public class ReportesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Pantalla principal de reportes
        public IActionResult Index()
        {
            return View();
        }

        // Reporte de ventas: diario, semanal o mensual
        public async Task<IActionResult> Ventas(string periodo = "diario")
        {
            // Solo ventas con Fecha no nula
            var ventasQuery = _context.Ventas
                .Where(v => v.Fecha.HasValue);

            var ventas = await ventasQuery.ToListAsync();

            var periodoNormalizado = (periodo ?? "diario").ToLower();

            var modelo = new ReporteVentasViewModel
            {
                Periodo = periodoNormalizado,
                Titulo = periodoNormalizado switch
                {
                    "semanal" => "Reporte de ventas semanales",
                    "mensual" => "Reporte de ventas mensuales",
                    _ => "Reporte de ventas diarias"
                }
            };

            if (!ventas.Any())
            {
                return View(modelo);
            }

            switch (periodoNormalizado)
            {
                case "semanal":
                    modelo.Items = ventas
                        .GroupBy(v =>
                            CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(
                                v.Fecha!.Value,
                                CalendarWeekRule.FirstFourDayWeek,
                                DayOfWeek.Monday)
                        )
                        .Select(g => new ReporteVentasItemViewModel
                        {
                            Fecha = g.Min(v => v.Fecha!.Value),
                            CantidadVentas = g.Count(),
                            MontoTotal = g.Sum(v => v.Total ?? 0m)
                        })
                        .OrderBy(r => r.Fecha)
                        .ToList();
                    break;

                case "mensual":
                    modelo.Items = ventas
                        .GroupBy(v => new { v.Fecha!.Value.Year, v.Fecha!.Value.Month })
                        .Select(g => new ReporteVentasItemViewModel
                        {
                            Fecha = new DateTime(g.Key.Year, g.Key.Month, 1),
                            CantidadVentas = g.Count(),
                            MontoTotal = g.Sum(v => v.Total ?? 0m)
                        })
                        .OrderBy(r => r.Fecha)
                        .ToList();
                    break;

                default: // diario
                    modelo.Items = ventas
                        .GroupBy(v => v.Fecha!.Value.Date)
                        .Select(g => new ReporteVentasItemViewModel
                        {
                            Fecha = g.Key,
                            CantidadVentas = g.Count(),
                            MontoTotal = g.Sum(v => v.Total ?? 0m)
                        })
                        .OrderBy(r => r.Fecha)
                        .ToList();
                    break;
            }

            return View(modelo);
        }

        // Reporte de inventario simple
        public async Task<IActionResult> Inventario()
        {
            var productos = await _context.Productos
                .OrderBy(p => p.Nombre)
                .Select(p => new InventarioProductoViewModel
                {
                    ProductoId = p.ProductoId,
                    Nombre = p.Nombre,
                    Stock = p.Stock,
                    Precio = p.Precio
                })
                .ToListAsync();

            return View(productos);
        }

        // Ranking de productos más vendidos
        public async Task<IActionResult> ProductosMasVendidos()
        {
            var detalles = await _context.DetallesVenta
                .Include(d => d.Producto)
                .ToListAsync();

            var modelo = detalles
                .Where(d => d.Producto != null)
                .GroupBy(d => new { d.ProductoId, d.Producto!.Nombre })
                .Select(g => new ProductoMasVendidoViewModel
                {
                    ProductoId = g.Key.ProductoId ?? 0,
                    Nombre = g.Key.Nombre,
                    CantidadVendida = g.Sum(d => d.Cantidad),
                    TotalVendido = g.Sum(d => d.Cantidad * d.PrecioUnitario)
                })
                .OrderByDescending(x => x.CantidadVendida)
                .ToList();

            return View(modelo);
        }
    }
}
