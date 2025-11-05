// Services/EmailService.cs
using SendGrid.Helpers.Mail;
using SendGrid;
using Microsoft.Extensions.Configuration;

namespace Tienda.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task EnviarFacturaAsync(string destinatario, string asunto, string mensajeHtml, string pdfPath)
        {
            var apiKey = _config["SendGrid:ApiKey"];
            var fromEmail = _config["SendGrid:FromEmail"];
            var fromName = _config["SendGrid:FromName"];

            var client = new SendGridClient(apiKey);
            var from = new EmailAddress(fromEmail, fromName);
            var to = new EmailAddress(destinatario);
            var msg = MailHelper.CreateSingleEmail(from, to, asunto, null, mensajeHtml);

            // Adjuntar PDF
            if (!string.IsNullOrEmpty(pdfPath) && File.Exists(pdfPath))
            {
                var bytes = await File.ReadAllBytesAsync(pdfPath);
                var file = Convert.ToBase64String(bytes);
                msg.AddAttachment(Path.GetFileName(pdfPath), file, "application/pdf");
            }

            await client.SendEmailAsync(msg);
        }
    }
}