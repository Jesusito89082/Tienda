using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Tienda.Data;
using Tienda.Models;

namespace Tienda.Controllers
{
    public class ProductoesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductoesController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Productoes - PÚBLICO
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var productos = _context.Productos
                .Include(p => p.Categoria);
            return View(await productos.ToListAsync());
        }

        // GET: Productoes/Details/5 - PÚBLICO
        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var producto = await _context.Productos
                .Include(p => p.Categoria)
                .Include(p => p.Comentarios)   // 👈 CARGA TAMBIÉN LOS COMENTARIOS
                .FirstOrDefaultAsync(m => m.ProductoId == id);

            if (producto == null)
                return NotFound();

            return View(producto);
        }

        // GET: Productoes/Create - SOLO ADMIN
        [Authorize(Roles = "ADMINISTRADOR")]
        public IActionResult Create()
        {
            ViewData["CategoriaId"] = new SelectList(_context.Categorias, "CategoriaId", "Nombre");
            return View();
        }

        // POST: Productoes/Create - SOLO ADMIN
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "ADMINISTRADOR")]
        public async Task<IActionResult> Create(
            [Bind("ProductoId,Nombre,Talla,Color,Precio,Stock,CategoriaId,ImagenArchivo")]
            Producto producto)
        {
            if (ModelState.IsValid)
            {
                // Procesar la imagen si existe
                if (producto.ImagenArchivo != null)
                {
                    producto.ImagenUrl = await GuardarImagen(producto.ImagenArchivo);
                }

                _context.Add(producto);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["CategoriaId"] = new SelectList(_context.Categorias, "CategoriaId", "Nombre", producto.CategoriaId);
            return View(producto);
        }

        // GET: Productoes/Edit/5 - SOLO ADMIN
        [Authorize(Roles = "ADMINISTRADOR")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var producto = await _context.Productos.FindAsync(id);
            if (producto == null)
                return NotFound();

            ViewData["CategoriaId"] = new SelectList(_context.Categorias, "CategoriaId", "Nombre", producto.CategoriaId);
            return View(producto);
        }

        // POST: Productoes/Edit/5 - SOLO ADMIN
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "ADMINISTRADOR")]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("ProductoId,Nombre,Talla,Color,Precio,Stock,CategoriaId,ImagenArchivo,ImagenUrl")]
            Producto producto)
        {
            if (id != producto.ProductoId)
                return NotFound();

            if (!ModelState.IsValid)
            {
                ViewData["CategoriaId"] = new SelectList(_context.Categorias, "CategoriaId", "Nombre", producto.CategoriaId);
                return View(producto);
            }

            try
            {
                // Si hay una nueva imagen, guardarla y eliminar la anterior
                if (producto.ImagenArchivo != null)
                {
                    if (!string.IsNullOrEmpty(producto.ImagenUrl))
                    {
                        EliminarImagen(producto.ImagenUrl);
                    }

                    producto.ImagenUrl = await GuardarImagen(producto.ImagenArchivo);
                }

                _context.Update(producto);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductoExists(producto.ProductoId))
                    return NotFound();
                else
                    throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Productoes/Delete/5 - SOLO ADMIN
        [Authorize(Roles = "ADMINISTRADOR")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var producto = await _context.Productos
                .Include(p => p.Categoria)
                .FirstOrDefaultAsync(m => m.ProductoId == id);

            if (producto == null)
                return NotFound();

            return View(producto);
        }

        // POST: Productoes/Delete/5 - SOLO ADMIN
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "ADMINISTRADOR")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto != null)
            {
                // Eliminar la imagen asociada si existe
                if (!string.IsNullOrEmpty(producto.ImagenUrl))
                {
                    EliminarImagen(producto.ImagenUrl);
                }

                _context.Productos.Remove(producto);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ProductoExists(int id)
        {
            return _context.Productos.Any(e => e.ProductoId == id);
        }

        // Método para guardar la imagen
        private async Task<string> GuardarImagen(IFormFile archivo)
        {
            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "productos");

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            string uniqueFileName = Guid.NewGuid().ToString() + "_" + archivo.FileName;
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await archivo.CopyToAsync(fileStream);
            }

            return "/images/productos/" + uniqueFileName;
        }

        // Método para eliminar la imagen
        private void EliminarImagen(string imagenUrl)
        {
            if (!string.IsNullOrEmpty(imagenUrl))
            {
                string filePath = Path.Combine(_webHostEnvironment.WebRootPath, imagenUrl.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }
        }
    }
}
