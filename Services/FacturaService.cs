using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using DinkToPdf;
using DinkToPdf.Contracts;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Tienda.Data;
using Tienda.Models;

namespace Tienda.Services
{
    public class FacturaService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConverter _pdfConverter;
        private readonly IWebHostEnvironment _env;

        public FacturaService(ApplicationDbContext context, IConverter pdfConverter, IWebHostEnvironment env)
        {
            _context = context;
            _pdfConverter = pdfConverter;
            _env = env;
        }

        public async Task<Factura> GenerarFacturaAsync(int ventaId)
        {
            var venta = await _context.Ventas
                .Include(v => v.Cliente)
                .Include(v => v.DetallesVenta).ThenInclude(d => d.Producto)
                .FirstOrDefaultAsync(v => v.VentaId == ventaId);

            if (venta == null) throw new Exception("Venta no encontrada.");

            // Total consistente
            var total = TotalesService.CalcularTotalVenta(venta);

            if (venta.Total != total)
            {
                venta.Total = total;
                _context.Update(venta);
                await _context.SaveChangesAsync();
            }

            // Generar consecutivo/clave simples (adapta a tu lógica real)
            var consecutivo = $"AURA-{ventaId:D8}";
            var clave = $"{DateTime.UtcNow:yyyyMMddHHmmss}-{ventaId:D8}";

            var factura = new Factura
            {
                VentaId = ventaId,
                NumeroConsecutivo = consecutivo,
                Clave = clave,
                FechaEmision = DateTime.Now,
                Estado = "Generada"
                // Si tu modelo Factura tiene Total: Total = total
            };

            _context.Facturas.Add(factura);
            await _context.SaveChangesAsync(); // Necesitamos FacturaId

            // Generar PDF
            var pdfBytes = GenerarPdfBytes(factura, venta, total);

            // Guardar PDF bajo wwwroot/facturas/yyyy/MM
            var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var dirRel = Path.Combine("facturas", DateTime.Now.ToString("yyyy"), DateTime.Now.ToString("MM"));
            var dirAbs = Path.Combine(webRoot, dirRel);
            Directory.CreateDirectory(dirAbs);

            var fileName = $"{factura.Clave}.pdf";
            var fileAbs = Path.Combine(dirAbs, fileName);
            await File.WriteAllBytesAsync(fileAbs, pdfBytes);

            // Guardar ruta relativa en Factura
            factura.PdfPath = Path.Combine(dirRel, fileName).Replace("\\", "/");
            _context.Update(factura);
            await _context.SaveChangesAsync();

            return factura;
        }

        private byte[] GenerarPdfBytes(Factura factura, Venta venta, decimal total)
        {
            var cultura = new CultureInfo("es-CR");
            var sb = new StringBuilder();

            sb.Append($@"
<!DOCTYPE html>
<html lang='es'>
<head>
    <meta charset='utf-8'>
    <title>Factura {factura.NumeroConsecutivo}</title>
    <style>
        body {{ font-family: Arial, Helvetica, sans-serif; font-size: 12px; }}
        h1 {{ font-size: 18px; }}
        table {{ width: 100%; border-collapse: collapse; margin-top: 12px; }}
        th, td {{ border: 1px solid #ccc; padding: 6px; text-align: left; }}
        .right {{ text-align: right; }}
        .totals {{ margin-top: 10px; }}
    </style>
</head>
<body>
    <h1>Factura #{factura.NumeroConsecutivo}</h1>
    <p><strong>Clave:</strong> {factura.Clave}</p>
    <p><strong>Fecha:</strong> {factura.FechaEmision:dd/MM/yyyy HH:mm}</p>
    <p><strong>Cliente:</strong> {venta?.Cliente?.Nombre ?? "—"}</p>

    <table>
        <thead>
            <tr>
                <th>Producto</th>
                <th class='right'>Cantidad</th>
                <th class='right'>Precio Unitario</th>
                <th class='right'>Subtotal</th>
            </tr>
        </thead>
        <tbody>");

            if (venta?.DetallesVenta != null)
            {
                foreach (var d in venta.DetallesVenta)
                {
                    var pu = string.Format(cultura, "{0:C}", d?.PrecioUnitario ?? 0);
                    var sub = string.Format(cultura, "{0:C}", (d?.Cantidad ?? 0) * (d?.PrecioUnitario ?? 0));
                    sb.Append($@"
        <tr>
            <td>{d?.Producto?.Nombre ?? "Producto"}</td>
            <td class='right'>{d?.Cantidad ?? 0}</td>
            <td class='right'>{pu}</td>
            <td class='right'>{sub}</td>
        </tr>");
                }
            }

            var totalTxt = string.Format(cultura, "{0:C}", total);

            sb.Append($@"
        </tbody>
    </table>

    <div class='totals right'>
        <p><strong>Total:</strong> {totalTxt}</p>
    </div>
</body>
</html>");

            var doc = new HtmlToPdfDocument
            {
                GlobalSettings = new GlobalSettings
                {
                    PaperSize = PaperKind.A4,
                    Orientation = Orientation.Portrait,
                    Margins = new MarginSettings { Top = 10, Right = 10, Bottom = 10, Left = 10 }
                },
                Objects =
                {
                    new ObjectSettings
                    {
                        HtmlContent = sb.ToString(),
                        WebSettings = new WebSettings { DefaultEncoding = "utf-8" }
                    }
                }
            };

            return _pdfConverter.Convert(doc);
        }
    }
}
