using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Tienda.Data;
using Tienda.Models;

namespace Tienda.Controllers
{
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

        // GET: DetallesVentums/Details/5  ←  AQUÍ CARGAMOS CLIENTE
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var detallesVentum = await _context.DetallesVenta
                .Include(d => d.Producto)
                .Include(d => d.Venta)
                    .ThenInclude(v => v.Cliente) // 👈 cliente disponible
                .FirstOrDefaultAsync(m => m.DetalleId == id);

            if (detallesVentum == null) return NotFound();

            return View(detallesVentum);
        }

        // ---------- RESTO DE ACCIONES (sin cambios) ----------

        // GET: DetallesVentums/Create
        public IActionResult Create()
        {
            ViewData["ProductoId"] = new SelectList(_context.Productos, "ProductoId", "ProductoId");
            ViewData["VentaId"] = new SelectList(_context.Ventas, "VentaId", "VentaId");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("DetalleId,VentaId,ProductoId,Cantidad,PrecioUnitario")] DetallesVentum detallesVentum)
        {
            if (ModelState.IsValid)
            {
                _context.Add(detallesVentum);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ProductoId"] = new SelectList(_context.Productos, "ProductoId", "ProductoId", detallesVentum.ProductoId);
            ViewData["VentaId"] = new SelectList(_context.Ventas, "VentaId", "VentaId", detallesVentum.VentaId);
            return View(detallesVentum);
        }

        // GET: DetallesVentums/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var detallesVentum = await _context.DetallesVenta.FindAsync(id);
            if (detallesVentum == null) return NotFound();

            ViewData["ProductoId"] = new SelectList(_context.Productos, "ProductoId", "ProductoId", detallesVentum.ProductoId);
            ViewData["VentaId"] = new SelectList(_context.Ventas, "VentaId", "VentaId", detallesVentum.VentaId);
            return View(detallesVentum);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("DetalleId,VentaId,ProductoId,Cantidad,PrecioUnitario")] DetallesVentum detallesVentum)
        {
            if (id != detallesVentum.DetalleId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
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
            ViewData["ProductoId"] = new SelectList(_context.Productos, "ProductoId", "ProductoId", detallesVentum.ProductoId);
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

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var detallesVentum = await _context.DetallesVenta.FindAsync(id);
            if (detallesVentum != null) _context.DetallesVenta.Remove(detallesVentum);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DetallesVentumExists(int id)
        {
            return _context.DetallesVenta.Any(e => e.DetalleId == id);
        }
    }
}