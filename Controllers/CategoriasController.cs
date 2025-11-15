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
    public class CategoriasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoriasController(ApplicationDbContext context) => _context = context;

        // GET: Categorias - Vista pública con productos
        [AllowAnonymous]
        public async Task<IActionResult> Index(int? id)
        {
            // Si se especifica un ID de categoría, mostrar solo esa categoría
            if (id.HasValue)
            {
                return await VerCategoria(id.Value);
            }

            // Si es admin, mostrar vista de gestión
            if (User.Identity?.IsAuthenticated == true && User.IsInRole("ADMINISTRADOR"))
            {
                return View("AdminIndex", await _context.Categorias.ToListAsync());
            }

            // Si es público, mostrar categorías con productos
            var categorias = await _context.Categorias
                .Include(c => c.Productos)
                .OrderBy(c => c.Nombre)
                .ToListAsync();

            return View("PublicIndex", categorias);
        }

        // GET: Categorias/Ver/5 - Vista pública de una categoría con sus productos
        [AllowAnonymous]
        [Route("Categorias/Ver/{id}")]
        public async Task<IActionResult> VerCategoria(int id)
        {
            var categoria = await _context.Categorias
                .Include(c => c.Productos)
                .FirstOrDefaultAsync(c => c.CategoriaId == id);

            if (categoria == null)
            {
                return NotFound();
            }

            return View("VerCategoria", categoria);
        }

        // GET: Categorias/Details/5
        [Authorize(Roles = "ADMINISTRADOR")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var categoria = await _context.Categorias
                .FirstOrDefaultAsync(m => m.CategoriaId == id);
            if (categoria == null)
            {
                return NotFound();
            }

            return View(categoria);
        }

        // GET: Categorias/Create
        [Authorize(Roles = "ADMINISTRADOR")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Categorias/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CategoriaId,Nombre")] Categoria categoria)
        {
            if (ModelState.IsValid)
            {
                _context.Add(categoria);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(categoria);
        }

        // GET: Categorias/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var categoria = await _context.Categorias.FindAsync(id);
            if (categoria == null)
            {
                return NotFound();
            }
            return View(categoria);
        }

        // POST: Categorias/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CategoriaId,Nombre")] Categoria categoria)
        {
            if (id != categoria.CategoriaId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(categoria);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoriaExists(categoria.CategoriaId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(categoria);
        }

        // GET: Categorias/Delete/5
        [Authorize(Roles = "ADMINISTRADOR")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var categoria = await _context.Categorias
                .Include(c => c.Productos)
                .FirstOrDefaultAsync(m => m.CategoriaId == id);

            if (categoria == null)
            {
                return NotFound();
            }

            // Verificar si tiene productos asociados
            ViewBag.TieneProductos = categoria.Productos.Any();
            ViewBag.CantidadProductos = categoria.Productos.Count;

            return View(categoria);
        }

        // POST: Categorias/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "ADMINISTRADOR")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var categoria = await _context.Categorias
                .Include(c => c.Productos)
                .FirstOrDefaultAsync(c => c.CategoriaId == id);

            if (categoria == null)
            {
                return NotFound();
            }

            // Validar si tiene productos asociados
            if (categoria.Productos.Any())
            {
                TempData["Error"] = $"No se puede eliminar la categoría '{categoria.Nombre}' porque tiene {categoria.Productos.Count} producto(s) asociado(s). Elimine o reasigne los productos primero.";
                return RedirectToAction(nameof(Index));
            }

            _context.Categorias.Remove(categoria);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"La categoría '{categoria.Nombre}' fue eliminada exitosamente.";
            return RedirectToAction(nameof(Index));
        }

        private bool CategoriaExists(int id)
        {
            return _context.Categorias.Any(e => e.CategoriaId == id);
        }
    }
}
