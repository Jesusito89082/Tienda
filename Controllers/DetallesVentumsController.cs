using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Tienda.Data;
using Tienda.Models;

namespace Tienda.Controllers
{
    [Authorize(Roles = "ADMINISTRADOR,CAJERO")]
    public class DetallesVentumsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DetallesVentumsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: DetallesVentums
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.DetallesVenta
                .Include(d => d.Producto)
                .Include(d => d.Venta);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: DetallesVentums/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var detallesVentum = await _context.DetallesVenta
                .Include(d => d.Producto)
                .Include(d => d.Venta)
                    .ThenInclude(v => v.Cliente)
                .FirstOrDefaultAsync(m => m.DetalleId == id);

            if (detallesVentum == null) return NotFound();

            return View(detallesVentum);
        }

        // GET: DetallesVentums/Create
        public IActionResult Create()
        {
            ViewData["ProductoId"] = new SelectList(_context.Productos, "ProductoId", "Nombre");
            ViewData["VentaId"] = new SelectList(_context.Ventas, "VentaId", "VentaId");
            return View();
        }

        // POST: DetallesVentums/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("DetalleId,VentaId,ProductoId,Cantidad")]
            DetallesVentum detallesVentum)
        {
            if (ModelState.IsValid)
            {
                var producto = await _context.Productos
                    .FirstOrDefaultAsync(p => p.ProductoId == detallesVentum.ProductoId);

                if (producto == null)
                {
                    ModelState.AddModelError("ProductoId", "El producto seleccionado no es válido.");
                }
                else
                {
                    // Verificar stock suficiente
                    if (producto.Stock < detallesVentum.Cantidad)
                    {
                        ModelState.AddModelError("Cantidad", "No hay stock suficiente para esta cantidad.");
                    }
                    else
                    {
                        // Tomar precio del producto
                        detallesVentum.PrecioUnitario = producto.Precio;

                        // Descontar del stock
                        producto.Stock -= detallesVentum.Cantidad;

                        _context.Update(producto);
                        _context.Add(detallesVentum);
                        await _context.SaveChangesAsync();
                        return RedirectToAction(nameof(Index));
                    }
                }
            }

            ViewData["ProductoId"] = new SelectList(_context.Productos, "ProductoId", "Nombre", detallesVentum.ProductoId);
            ViewData["VentaId"] = new SelectList(_context.Ventas, "VentaId", "VentaId", detallesVentum.VentaId);
            return View(detallesVentum);
        }

        // GET: DetallesVentums/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var detallesVentum = await _context.DetallesVenta.FindAsync(id);
            if (detallesVentum == null) return NotFound();

            ViewData["ProductoId"] = new SelectList(_context.Productos, "ProductoId", "Nombre", detallesVentum.ProductoId);
            ViewData["VentaId"] = new SelectList(_context.Ventas, "VentaId", "VentaId", detallesVentum.VentaId);
            return View(detallesVentum);
        }

        // POST: DetallesVentums/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("DetalleId,VentaId,ProductoId,Cantidad")]
            DetallesVentum detallesVentum)
        {
            if (id != detallesVentum.DetalleId) return NotFound();

            if (ModelState.IsValid)
            {
                // Traer el detalle original para saber la cantidad previa
                var detalleOriginal = await _context.DetallesVenta
                    .AsNoTracking()
                    .FirstOrDefaultAsync(d => d.DetalleId == id);

                if (detalleOriginal == null) return NotFound();

                var producto = await _context.Productos
                    .FirstOrDefaultAsync(p => p.ProductoId == detallesVentum.ProductoId);

                if (producto == null)
                {
                    ModelState.AddModelError("ProductoId", "El producto seleccionado no es válido.");
                }
                else
                {
                    // Diferencia de cantidades (positiva = se venden más, negativa = se devuelven)
                    var diferencia = detallesVentum.Cantidad - detalleOriginal.Cantidad;

                    if (diferencia > 0 && producto.Stock < diferencia)
                    {
                        ModelState.AddModelError("Cantidad", "No hay stock suficiente para aumentar esta cantidad.");
                    }
                    else
                    {
                        // Ajustar stock según diferencia
                        producto.Stock -= diferencia;

                        // Reasignar precio unitario al actual del producto
                        detallesVentum.PrecioUnitario = producto.Precio;

                        try
                        {
                            _context.Update(producto);
                            _context.Update(detallesVentum);
                            await _context.SaveChangesAsync();
                        }
                        catch (DbUpdateConcurrencyException)
                        {
                            if (!DetallesVentumExists(detallesVentum.DetalleId)) return NotFound();
                            else throw;
                        }

                        return RedirectToAction(nameof(Index));
                    }
                }
            }

            ViewData["ProductoId"] = new SelectList(_context.Productos, "ProductoId", "Nombre", detallesVentum.ProductoId);
            ViewData["VentaId"] = new SelectList(_context.Ventas, "VentaId", "VentaId", detallesVentum.VentaId);
            return View(detallesVentum);
        }

        // GET: DetallesVentums/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var detallesVentum = await _context.DetallesVenta
                .Include(d => d.Producto)
                .Include(d => d.Venta)
                .FirstOrDefaultAsync(m => m.DetalleId == id);

            if (detallesVentum == null) return NotFound();

            return View(detallesVentum);
        }

        // POST: DetallesVentums/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var detallesVentum = await _context.DetallesVenta
                .Include(d => d.Producto)
                .FirstOrDefaultAsync(d => d.DetalleId == id);

            if (detallesVentum != null)
            {
                // Devolver stock al eliminar el detalle
                if (detallesVentum.Producto != null)
                {
                    detallesVentum.Producto.Stock += detallesVentum.Cantidad;
                    _context.Update(detallesVentum.Producto);
                }

                _context.DetallesVenta.Remove(detallesVentum);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool DetallesVentumExists(int id)
        {
            return _context.DetallesVenta.Any(e => e.DetalleId == id);
        }
    }
}
