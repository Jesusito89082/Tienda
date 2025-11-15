using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tienda.Data;
using Tienda.Models;
using System.Text.Json;

namespace Tienda.Controllers
{
    [AllowAnonymous]
    public class CarritoController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const string CarritoSessionKey = "Carrito";

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

          // Obtener lista de clientes para el dropdown
ViewBag.Clientes = await _context.Clientes
     .Select(c => new { c.ClienteId, c.Nombre })
 .ToListAsync();

        return View(carrito);
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
        public IActionResult ObtenerTotal()
        {
            var carrito = ObtenerCarrito();
        var total = carrito.Sum(i => i.Cantidad * i.PrecioUnitario);
            var totalItems = carrito.Sum(i => i.Cantidad);

            return Json(new
      {
        total = total,
      totalItems = totalItems,
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
