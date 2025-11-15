using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Tienda.Data;
using Tienda.Models;
using Tienda.Services;
using Tienda.ViewModels;

namespace Tienda.Controllers
{
    [AllowAnonymous]
    public class CarritoController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const string CarritoSessionKey = "Carrito";
        private const decimal IVA = 0.13m; // 13%

        public CarritoController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Carrito/Index
        public IActionResult Index()
        {
            var carrito = ObtenerCarrito();
            return View(carrito);
        }

        // GET: Carrito/Checkout
        public async Task<IActionResult> Checkout()
        {
            var carrito = ObtenerCarrito();

            if (!carrito.Any())
            {
                TempData["Error"] = "El carrito está vacío";
                return RedirectToAction(nameof(Index));
            }

            var clientes = await _context.Clientes
                .OrderBy(c => c.Nombre)
                .ToListAsync();

            var subtotal = carrito.Sum(i => i.Cantidad * i.PrecioUnitario);
            var descuentoPorcentaje = 0m; // por defecto sin descuento
            var descuentoMonto = 0m;
            var baseGravable = subtotal - descuentoMonto;
            var impuesto = Math.Round(baseGravable * IVA, 2);
            var total = baseGravable + impuesto;

            var modelo = new CheckoutViewModel
            {
                ClienteId = null,
                DescuentoPorcentaje = descuentoPorcentaje,
                Subtotal = subtotal,
                DescuentoMonto = descuentoMonto,
                Impuesto = impuesto,
                Total = total,
                Items = carrito,
                Clientes = clientes
            };

            return View(modelo);
        }

        // POST: Carrito/Checkout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(CheckoutViewModel model)
        {
            var carrito = ObtenerCarrito();

            if (!carrito.Any())
            {
                TempData["Error"] = "El carrito está vacío";
                return RedirectToAction(nameof(Index));
            }

            // Si algo viene mal, volvemos a mostrar el resumen
            if (!ModelState.IsValid)
            {
                var clientes = await _context.Clientes
                    .OrderBy(c => c.Nombre)
                    .ToListAsync();

                var subtotalTmp = carrito.Sum(i => i.Cantidad * i.PrecioUnitario);
                var descPorcTmp = model.DescuentoPorcentaje;
                var descMontoTmp = descPorcTmp > 0
                    ? Math.Round(subtotalTmp * (descPorcTmp / 100m), 2)
                    : 0m;
                var baseGravableTmp = subtotalTmp - descMontoTmp;
                var impuestoTmp = Math.Round(baseGravableTmp * IVA, 2);
                var totalTmp = baseGravableTmp + impuestoTmp;

                model.Items = carrito;
                model.Clientes = clientes;
                model.Subtotal = subtotalTmp;
                model.DescuentoMonto = descMontoTmp;
                model.Impuesto = impuestoTmp;
                model.Total = totalTmp;

                return View(model);
            }

            // Crear la venta
            var venta = new Venta
            {
                ClienteId = model.ClienteId,
                Fecha = DateTime.Now
            };

            var detalles = new List<DetallesVentum>();

            foreach (var item in carrito)
            {
                var detalle = new DetallesVentum
                {
                    Venta = venta,
                    ProductoId = item.ProductoId,
                    Cantidad = item.Cantidad,
                    PrecioUnitario = item.PrecioUnitario
                };

                detalles.Add(detalle);
                _context.DetallesVenta.Add(detalle);

                // Actualizar stock
                var producto = await _context.Productos.FindAsync(item.ProductoId);
                if (producto != null)
                {
                    producto.Stock -= item.Cantidad;
                    _context.Productos.Update(producto);
                }
            }

            // Calcular totales (Subtotal, Descuento, Impuesto, Total)
            TotalesService.CalcularTotalesVenta(
                venta,
                detalles,
                model.DescuentoPorcentaje
            );

            _context.Ventas.Add(venta);
            await _context.SaveChangesAsync();

            // Limpiar carrito
            HttpContext.Session.Remove(CarritoSessionKey);

            TempData["Success"] = "La venta se ha registrado correctamente.";

            // Redirigir al detalle de la venta (o Index si prefieres)
            return RedirectToAction("Details", "Ventas", new { id = venta.VentaId });
        }

        // POST: Carrito/AgregarProducto
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AgregarProducto([FromBody] AgregarProductoRequest request)
        {
            if (request == null || request.ProductoId <= 0)
            {
                return Json(new { success = false, message = "Datos inválidos" });
            }

            var producto = await _context.Productos
                .Include(p => p.Categoria)
                .FirstOrDefaultAsync(p => p.ProductoId == request.ProductoId);

            if (producto == null)
            {
                return Json(new { success = false, message = "Producto no encontrado" });
            }

            if (producto.Stock < request.Cantidad)
            {
                return Json(new { success = false, message = "Stock insuficiente" });
            }

            var carrito = ObtenerCarrito();
            var itemExistente = carrito.FirstOrDefault(i => i.ProductoId == request.ProductoId);

            if (itemExistente != null)
            {
                // Si ya existe, aumentar la cantidad
                itemExistente.Cantidad += request.Cantidad;
            }
            else
            {
                // Si no existe, agregar nuevo item
                carrito.Add(new CarritoItem
                {
                    ProductoId = producto.ProductoId,
                    Nombre = producto.Nombre,
                    Cantidad = request.Cantidad,
                    PrecioUnitario = producto.Precio
                });
            }

            GuardarCarrito(carrito);

            var totalItems = carrito.Sum(i => i.Cantidad);
            return Json(new
            {
                success = true,
                message = "Producto agregado al carrito",
                totalItems = totalItems
            });
        }

        // POST: Carrito/ActualizarCantidad
        [HttpPost]
        public IActionResult ActualizarCantidad(int productoId, int cantidad)
        {
            var carrito = ObtenerCarrito();
            var item = carrito.FirstOrDefault(i => i.ProductoId == productoId);

            if (item != null)
            {
                if (cantidad <= 0)
                {
                    carrito.Remove(item);
                }
                else
                {
                    item.Cantidad = cantidad;
                }

                GuardarCarrito(carrito);
                return Json(new { success = true });
            }

            return Json(new { success = false, message = "Producto no encontrado en el carrito" });
        }

        // POST: Carrito/EliminarProducto
        [HttpPost]
        public IActionResult EliminarProducto(int productoId)
        {
            var carrito = ObtenerCarrito();
            var item = carrito.FirstOrDefault(i => i.ProductoId == productoId);

            if (item != null)
            {
                carrito.Remove(item);
                GuardarCarrito(carrito);
                return Json(new { success = true, message = "Producto eliminado del carrito" });
            }

            return Json(new { success = false, message = "Producto no encontrado en el carrito" });
        }

        // POST: Carrito/Limpiar
        [HttpPost]
        public IActionResult Limpiar()
        {
            HttpContext.Session.Remove(CarritoSessionKey);
            return Json(new { success = true, message = "Carrito vaciado" });
        }

        // GET: Carrito/ObtenerTotal
        [HttpGet]
        public IActionResult ObtenerTotal(decimal descuentoPorcentaje = 0)
        {
            var carrito = ObtenerCarrito();

            var subtotal = carrito.Sum(i => i.Cantidad * i.PrecioUnitario);

            var descuentoMonto = 0m;
            if (descuentoPorcentaje > 0)
            {
                descuentoMonto = Math.Round(subtotal * (descuentoPorcentaje / 100m), 2);
            }

            var baseGravable = subtotal - descuentoMonto;
            var impuesto = Math.Round(baseGravable * IVA, 2);
            var total = baseGravable + impuesto;

            var totalItems = carrito.Sum(i => i.Cantidad);

            return Json(new
            {
                subtotal,
                descuentoPorcentaje,
                descuentoMonto,
                impuesto,
                total,
                totalItems,
                items = carrito
            });
        }

        // Métodos privados auxiliares
        private List<CarritoItem> ObtenerCarrito()
        {
            var carritoJson = HttpContext.Session.GetString(CarritoSessionKey);

            if (string.IsNullOrEmpty(carritoJson))
            {
                return new List<CarritoItem>();
            }

            return JsonSerializer.Deserialize<List<CarritoItem>>(carritoJson) ?? new List<CarritoItem>();
        }

        private void GuardarCarrito(List<CarritoItem> carrito)
        {
            var carritoJson = JsonSerializer.Serialize(carrito);
            HttpContext.Session.SetString(CarritoSessionKey, carritoJson);
        }
    }

    // Clase auxiliar para recibir datos del request
    public class AgregarProductoRequest
    {
        public int ProductoId { get; set; }
        public int Cantidad { get; set; } = 1;
    }
}
