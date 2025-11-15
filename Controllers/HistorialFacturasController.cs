using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tienda.Data;
using Tienda.Models;

namespace Tienda.Controllers
{
    [Authorize(Roles = "ADMINISTRADOR,CAJERO")]
    public class HistorialFacturasController : Controller
  {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HistorialFacturasController> _logger;

    public HistorialFacturasController(ApplicationDbContext context, ILogger<HistorialFacturasController> logger)
        {
        _context = context;
            _logger = logger;
        }

   // GET: HistorialFacturas - Vista general de clientes con compras
        public async Task<IActionResult> Index()
      {
var clientesConCompras = await _context.Clientes
   .Include(c => c.Venta)
    .ThenInclude(v => v.DetallesVenta)
   .ThenInclude(d => d.Producto)
         .Include(c => c.Venta)
   .ThenInclude(v => v.Facturas)
 .Where(c => c.Venta.Any())
        .OrderByDescending(c => c.Venta.Count())
     .ToListAsync();

    // Crear un resumen para cada cliente
var resumenClientes = clientesConCompras.Select(cliente => new
        {
      Cliente = cliente,
    TotalCompras = cliente.Venta.Count(),
  TotalProductos = cliente.Venta
   .SelectMany(v => v.DetallesVenta)
 .Sum(d => d.Cantidad),
      TotalGastado = cliente.Venta.Sum(v => v.Total ?? 0),
 UltimaCompra = cliente.Venta.Max(v => v.Fecha),
TotalFacturas = cliente.Venta.SelectMany(v => v.Facturas).Count()
    }).ToList();

            return View(resumenClientes);
  }

        // GET: HistorialFacturas/DetalleCliente/5 - Detalle completo de compras de un cliente
        public async Task<IActionResult> DetalleCliente(int? id)
   {
if (id == null)
       {
  return NotFound();
   }

  var cliente = await _context.Clientes
.Include(c => c.Venta)
     .ThenInclude(v => v.DetallesVenta)
          .ThenInclude(d => d.Producto)
  .ThenInclude(p => p.Categoria)
 .Include(c => c.Venta)
           .ThenInclude(v => v.Facturas)
  .FirstOrDefaultAsync(c => c.ClienteId == id);

       if (cliente == null)
          {
 return NotFound();
         }

   // Ordenar ventas por fecha descendente
  cliente.Venta = cliente.Venta.OrderByDescending(v => v.Fecha).ToList();

  // Calcular estadísticas
  ViewBag.TotalCompras = cliente.Venta.Count();
    ViewBag.TotalProductos = cliente.Venta
     .SelectMany(v => v.DetallesVenta)
    .Sum(d => d.Cantidad);
  ViewBag.TotalGastado = cliente.Venta.Sum(v => v.Total ?? 0);
         ViewBag.PromedioCompra = cliente.Venta.Any() 
          ? cliente.Venta.Average(v => v.Total ?? 0) 
    : 0;
     ViewBag.ProductoMasComprado = cliente.Venta
  .SelectMany(v => v.DetallesVenta)
  .GroupBy(d => d.Producto.Nombre)
      .OrderByDescending(g => g.Sum(d => d.Cantidad))
 .Select(g => new { Nombre = g.Key, Cantidad = g.Sum(d => d.Cantidad) })
      .FirstOrDefault();

  return View(cliente);
   }

        // GET: HistorialFacturas/FacturasCliente/5 - Lista de facturas de un cliente específico
public async Task<IActionResult> FacturasCliente(int? id)
        {
      if (id == null)
 {
 return NotFound();
   }

  var cliente = await _context.Clientes
    .Include(c => c.Venta)
     .ThenInclude(v => v.Facturas)
   .FirstOrDefaultAsync(c => c.ClienteId == id);

 if (cliente == null)
     {
       return NotFound();
 }

            var facturas = cliente.Venta
         .SelectMany(v => v.Facturas)
       .OrderByDescending(f => f.FechaEmision)
   .ToList();

  ViewBag.Cliente = cliente;
    ViewBag.TotalFacturado = facturas.Sum(f => f.Venta?.Total ?? 0);

     return View(facturas);
        }

        // GET: HistorialFacturas/ProductosCliente/5 - Productos más comprados por el cliente
        public async Task<IActionResult> ProductosCliente(int? id)
     {
     if (id == null)
  {
     return NotFound();
      }

   var cliente = await _context.Clientes
    .Include(c => c.Venta)
   .ThenInclude(v => v.DetallesVenta)
  .ThenInclude(d => d.Producto)
    .ThenInclude(p => p.Categoria)
  .FirstOrDefaultAsync(c => c.ClienteId == id);

            if (cliente == null)
  {
   return NotFound();
 }

       var productosComprados = cliente.Venta
       .SelectMany(v => v.DetallesVenta)
           .GroupBy(d => new { d.Producto.ProductoId, NombreProducto = d.Producto.Nombre, NombreCategoria = d.Producto.Categoria.Nombre })
  .Select(g => new
       {
    ProductoId = g.Key.ProductoId,
Nombre = g.Key.NombreProducto,
 Categoria = g.Key.NombreCategoria,
    CantidadTotal = g.Sum(d => d.Cantidad),
         TotalGastado = g.Sum(d => d.Cantidad * d.PrecioUnitario),
          VecesComprado = g.Count()
     })
  .OrderByDescending(p => p.CantidadTotal)
  .ToList();

  ViewBag.Cliente = cliente;

     return View(productosComprados);
        }

// GET: HistorialFacturas/Estadisticas - Estadísticas globales
        public async Task<IActionResult> Estadisticas()
  {
   var totalClientes = await _context.Clientes.CountAsync();
        var clientesConCompras = await _context.Clientes
    .Include(c => c.Venta)
         .Where(c => c.Venta.Any())
   .CountAsync();

   var totalVentas = await _context.Ventas.CountAsync();
    var totalFacturado = await _context.Ventas.SumAsync(v => v.Total ?? 0);
    var promedioVenta = totalVentas > 0 ? totalFacturado / totalVentas : 0;

     var clienteTopGastador = await _context.Clientes
    .Include(c => c.Venta)
   .Where(c => c.Venta.Any())
   .Select(c => new
          {
 Cliente = c,
 TotalGastado = c.Venta.Sum(v => v.Total ?? 0)
  })
        .OrderByDescending(x => x.TotalGastado)
 .FirstOrDefaultAsync();

var clienteTopComprador = await _context.Clientes
     .Include(c => c.Venta)
.Where(c => c.Venta.Any())
    .Select(c => new
     {
  Cliente = c,
        TotalCompras = c.Venta.Count()
   })
 .OrderByDescending(x => x.TotalCompras)
   .FirstOrDefaultAsync();

       var productoMasVendido = await _context.DetallesVenta
           .Include(d => d.Producto)
   .GroupBy(d => new { d.Producto.ProductoId, d.Producto.Nombre })
.Select(g => new
          {
      Nombre = g.Key.Nombre,
    CantidadVendida = g.Sum(d => d.Cantidad)
})
       .OrderByDescending(p => p.CantidadVendida)
   .FirstOrDefaultAsync();

   ViewBag.TotalClientes = totalClientes;
       ViewBag.ClientesConCompras = clientesConCompras;
ViewBag.TotalVentas = totalVentas;
      ViewBag.TotalFacturado = totalFacturado;
    ViewBag.PromedioVenta = promedioVenta;
  ViewBag.ClienteTopGastador = clienteTopGastador;
 ViewBag.ClienteTopComprador = clienteTopComprador;
 ViewBag.ProductoMasVendido = productoMasVendido;

 return View();
    }
  }
}
