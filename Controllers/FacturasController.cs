using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Tienda.Data;
using Tienda.Models;
using Tienda.Services;

namespace Tienda.Controllers
{
    public class FacturasController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly FacturaService _facturaService;
        private readonly EmailService _emailService;
        private readonly IWebHostEnvironment _env;

        public FacturasController(
            ApplicationDbContext context,
            FacturaService facturaService,
            EmailService emailService,
            IWebHostEnvironment env)
        {
            _context = context;
            _facturaService = facturaService;
            _emailService = emailService;
            _env = env;
        }

        public async Task<IActionResult> Index()
        {
            var lista = await _context.Facturas
                .AsNoTracking()
                .Include(f => f.Venta).ThenInclude(v => v.Cliente)
                .OrderByDescending(f => f.FechaEmision)
                .ToListAsync();
            return View(lista);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var factura = await _context.Facturas
                .AsNoTracking()
                .Include(f => f.Venta).ThenInclude(v => v.Cliente)
                .FirstOrDefaultAsync(m => m.FacturaId == id);

            if (factura == null) return NotFound();

            return View(factura);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Generar(int ventaId)
        {
            try
            {
                // una venta válida
                var ventaExiste = await _context.Ventas.AsNoTracking()
                    .AnyAsync(v => v.VentaId == ventaId);
                if (!ventaExiste)
                {
                    TempData["Error"] = "La venta especificada no existe.";
                    return RedirectToAction("Index", "Ventas");
                }

                // evitar duplicado
                var yaTiene = await _context.Facturas.AsNoTracking()
                    .FirstOrDefaultAsync(f => f.VentaId == ventaId);
                if (yaTiene != null)
                {
                    TempData["Mensaje"] = "La venta ya tiene una factura creada.";
                    return RedirectToAction(nameof(Details), new { id = yaTiene.FacturaId });
                }

                var factura = await _facturaService.GenerarFacturaAsync(ventaId);
                return RedirectToAction(nameof(Details), new { id = factura.FacturaId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"No se pudo generar la factura: {ex.Message}";
                return RedirectToAction("Details", "Ventas", new { id = ventaId });
            }
        }

        [HttpGet]
        public IActionResult Descargar(int id)
        {
            var factura = _context.Facturas.AsNoTracking().FirstOrDefault(f => f.FacturaId == id);
            if (factura == null) return NotFound();

            var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var pdfRel = (factura.PdfPath ?? string.Empty)
                .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            var pdfAbs = Path.Combine(webRoot, pdfRel);

            if (!System.IO.File.Exists(pdfAbs)) return NotFound();

            var fileName = string.IsNullOrWhiteSpace(factura.Clave) ? $"factura_{id}.pdf" : $"{factura.Clave}.pdf";
            return PhysicalFile(pdfAbs, "application/pdf", fileName);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnviarPorEmail(int id)
        {
            var factura = await _context.Facturas
                .Include(f => f.Venta).ThenInclude(v => v.Cliente)
                .FirstOrDefaultAsync(f => f.FacturaId == id);

            if (factura == null) return NotFound();

            var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var pdfRel = (factura.PdfPath ?? string.Empty)
                .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            var pdfAbs = Path.Combine(webRoot, pdfRel);

            if (!System.IO.File.Exists(pdfAbs))
            {
                TempData["Error"] = "No se encontró el archivo PDF de la factura.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var cultura = new CultureInfo("es-CR");
            var clienteNombre = factura.Venta?.Cliente?.Nombre ?? "cliente";
            var clienteEmail = factura.Venta?.Cliente?.Email;

            if (string.IsNullOrWhiteSpace(clienteEmail))
            {
                TempData["Error"] = "El cliente no tiene un correo registrado.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var consecutivo = string.IsNullOrWhiteSpace(factura.NumeroConsecutivo)
                ? factura.FacturaId.ToString()
                : factura.NumeroConsecutivo;

            var totalTxt = string.Format(cultura, "{0:C}", factura.Venta?.Total ?? 0m);

            var asunto = $"Factura #{consecutivo} - Aura";
            var mensaje = $@"
                <h2>Gracias por tu compra, <strong>{clienteNombre}</strong>!</h2>
                <p>Adjuntamos tu factura electrónica.</p>
                <p><strong>Fecha:</strong> {factura.FechaEmision:dd/MM/yyyy}</p>
                <p><strong>Total:</strong> {totalTxt}</p>
                <hr>
                <small>Este correo fue generado automáticamente por Aura.</small>
            ";

            try
            {
                await _emailService.EnviarFacturaAsync(
                    destinatario: clienteEmail,
                    asunto: asunto,
                    mensajeHtml: mensaje,
                    pdfPath: pdfAbs);

                TempData["Mensaje"] = "Factura enviada por correo.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al enviar: {ex.Message}";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        // Scaffold opcional (igual al tuyo) ...

        private bool FacturaExists(int id) =>
            _context.Facturas.Any(e => e.FacturaId == id);
    }
}
