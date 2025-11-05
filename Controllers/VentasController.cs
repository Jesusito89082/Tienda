using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Tienda.Data;
using Tienda.Models;
using Tienda.Services;

namespace Tienda.Controllers
{
    public class VentasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VentasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Ventas
        public async Task<IActionResult> Index()
        {
            var ventas = await _context.Ventas
                .AsNoTracking()
                .Include(v => v.Cliente)
                .OrderByDescending(v => v.Fecha)
                .ToListAsync();

            return View(ventas);
        }

        // GET: Ventas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var venta = await _context.Ventas
                .Include(v => v.Cliente)
                .Include(v => v.DetallesVenta).ThenInclude(d => d.Producto)
                .FirstOrDefaultAsync(m => m.VentaId == id);

            if (venta == null) return NotFound();

            // 🔢 Fuente de verdad: suma de detalles
            var totalReal = TotalesService.CalcularTotalVenta(venta);
            if (venta.Total != totalReal)
            {
                venta.Total = totalReal;
                _context.Update(venta);
                await _context.SaveChangesAsync();
            }

            return View(venta);
        }

        // GET: Ventas/Create
        public IActionResult Create()
        {
            ViewData["ClienteId"] = new SelectList(_context.Clientes.AsNoTracking(), "ClienteId", "Nombre");
            return View();
        }

        // POST: Ventas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("VentaId,ClienteId,Fecha,Total")] Venta venta)
        {
            if (!ModelState.IsValid)
            {
                ViewData["ClienteId"] = new SelectList(_context.Clientes.AsNoTracking(), "ClienteId", "Nombre", venta.ClienteId);
                return View(venta);
            }

            _context.Add(venta);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Ventas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var venta = await _context.Ventas.FindAsync(id);
            if (venta == null) return NotFound();

            ViewData["ClienteId"] = new SelectList(_context.Clientes.AsNoTracking(), "ClienteId", "Nombre", venta.ClienteId);
            return View(venta);
        }

        // POST: Ventas/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("VentaId,ClienteId,Fecha,Total")] Venta venta)
        {
            if (id != venta.VentaId) return NotFound();

            if (!ModelState.IsValid)
            {
                ViewData["ClienteId"] = new SelectList(_context.Clientes.AsNoTracking(), "ClienteId", "Nombre", venta.ClienteId);
                return View(venta);
            }

            try
            {
                _context.Update(venta);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!VentaExists(venta.VentaId)) return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Ventas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var venta = await _context.Ventas
                .AsNoTracking()
                .Include(v => v.Cliente)
                .FirstOrDefaultAsync(m => m.VentaId == id);

            if (venta == null) return NotFound();
            return View(venta);
        }

        // POST: Ventas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var venta = await _context.Ventas.FindAsync(id);
            if (venta != null) _context.Ventas.Remove(venta);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // POST: Finalizar venta con detalles (desde carrito)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FinalizarVenta(int clienteId, List<CarritoItem> carrito)
        {
            if (carrito == null || !carrito.Any())
            {
                TempData["Error"] = "El carrito está vacío.";
                return RedirectToAction("Index", "Carrito");
            }

            // Validar cliente
            var clienteExiste = await _context.Clientes.AsNoTracking()
                .AnyAsync(c => c.ClienteId == clienteId);
            if (!clienteExiste)
            {
                TempData["Error"] = "El cliente indicado no existe.";
                return RedirectToAction("Index", "Carrito");
            }

            // Normaliza y valida items
            carrito = carrito.Where(c => c.Cantidad > 0).ToList();
            if (!carrito.Any())
            {
                TempData["Error"] = "No hay líneas válidas en el carrito.";
                return RedirectToAction("Index", "Carrito");
            }

            // Verifica existencia/stock
            var productoIds = carrito.Select(c => c.ProductoId).Distinct().ToList();
            var productos = await _context.Productos
                .Where(p => productoIds.Contains(p.ProductoId))
                .ToListAsync();

            foreach (var item in carrito)
            {
                var prod = productos.FirstOrDefault(p => p.ProductoId == item.ProductoId);
                if (prod == null)
                {
                    TempData["Error"] = $"Producto {item.ProductoId} no existe.";
                    return RedirectToAction("Index", "Carrito");
                }
                if (prod.Stock < item.Cantidad)
                {
                    TempData["Error"] = $"Stock insuficiente para {prod.Nombre}. Disponible: {prod.Stock}.";
                    return RedirectToAction("Index", "Carrito");
                }
            }

            // Total desde carrito (fuente de verdad)
            var total = carrito.Sum(c => c.Cantidad * c.PrecioUnitario);

            using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                var venta = new Venta
                {
                    ClienteId = clienteId,
                    Fecha = DateTime.Now,
                    Total = total
                };

                _context.Ventas.Add(venta);
                await _context.SaveChangesAsync(); // genera VentaId

                // Detalles + descuento de stock
                foreach (var item in carrito)
                {
                    _context.DetallesVenta.Add(new DetallesVentum
                    {
                        VentaId = venta.VentaId,
                        ProductoId = item.ProductoId,
                        Cantidad = item.Cantidad,
                        PrecioUnitario = item.PrecioUnitario
                    });

                    var prod = productos.First(p => p.ProductoId == item.ProductoId);
                    prod.Stock -= item.Cantidad;
                    _context.Productos.Update(prod);
                }

                await _context.SaveChangesAsync();

                await tx.CommitAsync();
                return RedirectToAction("Details", new { id = venta.VentaId });
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                TempData["Error"] = $"No se pudo finalizar la venta: {ex.Message}";
                return RedirectToAction("Index", "Carrito");
            }
        }

        private bool VentaExists(int id) =>
            _context.Ventas.Any(e => e.VentaId == id);
    }
}
