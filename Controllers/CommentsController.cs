using Microsoft.AspNetCore.Mvc;
using Tienda.Data;
using Tienda.Models;

public class CommentsController : Controller
{
    private readonly ApplicationDbContext _context;

    public CommentsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public IActionResult Crear(int productoId, string texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
        {
            // Volver al detalle del producto sin hacer nada
            return RedirectToAction("Details", "Productoes", new { id = productoId });
        }

        var comment = new Comment
        {
            ProductoId = productoId,
            Texto = texto
        };

        _context.Comments.Add(comment);
        _context.SaveChanges();

        // 👇 IMPORTANTE: usar "Productoes" porque así se llama tu controlador
        return RedirectToAction("Details", "Productoes", new { id = productoId });
    }
}
