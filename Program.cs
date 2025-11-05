using System.Globalization;
using DinkToPdf;
using DinkToPdf.Contracts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Server.Kestrel.Core; // AllowSynchronousIO (DinkToPdf)
using Tienda.Data;
using Tienda.Services;

var builder = WebApplication.CreateBuilder(args);

// ---------------- DI / Servicios ----------------
builder.Services.AddTransient<EmailService>();           // Servicio de correo
builder.Services.AddScoped<FacturaService>();            // Servicio de factura

// DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Identity (si usas login)
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>();

// MVC + Razor
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// DinkToPdf (wkhtmltopdf)
builder.Services.AddSingleton<IConverter>(new SynchronizedConverter(new PdfTools()));

// ?? Compatibilidad con librerías que usan IO síncrona (DinkToPdf a veces lo requiere)
builder.Services.Configure<IISServerOptions>(opt => opt.AllowSynchronousIO = true);
builder.Services.Configure<KestrelServerOptions>(opt => opt.AllowSynchronousIO = true);

// ?? Localización/Cultura por defecto: es-CR (moneda y fechas)
var culturaCR = new CultureInfo("es-CR");
CultureInfo.DefaultThreadCurrentCulture = culturaCR;
CultureInfo.DefaultThreadCurrentUICulture = culturaCR;
builder.Services.Configure<Microsoft.AspNetCore.Builder.RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[] { culturaCR };
    options.DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture(culturaCR);
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});

var app = builder.Build();

// ---------------- Pipeline ----------------
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); // primero autenticación
app.UseAuthorization();  // luego autorización

// Localización (debe ir después de Routing y antes de Map*)
var locOptions = app.Services.GetRequiredService<Microsoft.Extensions.Options.IOptions<Microsoft.AspNetCore.Builder.RequestLocalizationOptions>>();
app.UseRequestLocalization(locOptions.Value);

// Rutas MVC convencionales
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Páginas de Identity/Razor (si usas /Identity/Account/…)
app.MapRazorPages();

app.Run();
