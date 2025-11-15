using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tienda.Data;
using Tienda.Models;

namespace Tienda.Controllers
{
    [AllowAnonymous]
    public class BusquedaController : Controller
    {
    private readonly ApplicationDbContext _context;
        private readonly ILogger<BusquedaController> _logger;

        public BusquedaController(ApplicationDbContext context, ILogger<BusquedaController> logger)
      {
   _context = context;
      _logger = logger;
        }

     // GET: Busqueda/Index
     public async Task<IActionResult> Index(string q, string filtro = "todos")
        {
   if (string.IsNullOrWhiteSpace(q))
            {
         return RedirectToAction("Index", "Home");
  }

 ViewBag.Query = q;
   ViewBag.Filtro = filtro;

            var productos = new List<Producto>();
       var categorias = new List<Categoria>();

            var query = q.ToLower().Trim();

            // Buscar según el filtro
if (filtro == "todos" || filtro == "productos")
    {
            productos = await _context.Productos
        .Include(p => p.Categoria)
       .Where(p => 
                 p.Nombre.ToLower().Contains(query) ||
        p.Color.ToLower().Contains(query) ||
          p.Talla.ToLower().Contains(query) ||
      p.Categoria.Nombre.ToLower().Contains(query))
   .OrderBy(p => p.Nombre)
       .ToListAsync();
    }

            if (filtro == "todos" || filtro == "categorias")
  {
                categorias = await _context.Categorias
           .Include(c => c.Productos)
 .Where(c => c.Nombre.ToLower().Contains(query))
   .OrderBy(c => c.Nombre)
  .ToListAsync();
    }

            ViewBag.TotalProductos = productos.Count;
     ViewBag.TotalCategorias = categorias.Count;
            ViewBag.TotalResultados = productos.Count + categorias.Count;

   var resultado = new Tuple<List<Producto>, List<Categoria>>(productos, categorias);
     return View(resultado);
        }

        // API: Busqueda/Sugerencias - Para autocompletar
    [HttpGet]
 public async Task<IActionResult> Sugerencias(string term)
        {
      if (string.IsNullOrWhiteSpace(term) || term.Length < 2)
  {
  return Json(new List<object>());
       }

            var query = term.ToLower().Trim();

   // Buscar productos
    var productos = await _context.Productos
.Include(p => p.Categoria)
   .Where(p => 
    p.Nombre.ToLower().Contains(query) ||
   p.Categoria.Nombre.ToLower().Contains(query))
  .Take(5)
     .Select(p => new
      {
  tipo = "producto",
  id = p.ProductoId,
       nombre = p.Nombre,
categoria = p.Categoria.Nombre,
   precio = p.Precio,
    imagen = p.ImagenUrl,
     url = Url.Action("Details", "Productoes", new { id = p.ProductoId })
})
    .ToListAsync();

      // Buscar categorías
      var categorias = await _context.Categorias
     .Where(c => c.Nombre.ToLower().Contains(query))
 .Take(3)
   .Select(c => new
      {
  tipo = "categoria",
   id = c.CategoriaId,
   nombre = c.Nombre,
        totalProductos = c.Productos.Count,
 url = Url.Action("VerCategoria", "Categorias", new { id = c.CategoriaId })
    })
      .ToListAsync();

  var resultados = new List<object>();
   resultados.AddRange(categorias);
   resultados.AddRange(productos);

return Json(resultados);
        }
    }
}
