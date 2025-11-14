using System.Globalization;
using DinkToPdf;
using DinkToPdf.Contracts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Server.Kestrel.Core; // AllowSynchronousIO (DinkToPdf)
using Tienda.Data;
using Tienda.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ---------------- DI / Servicios ----------------
builder.Services.AddTransient<EmailService>();           // Servicio de correo
builder.Services.AddScoped<FacturaService>();            // Servicio de factura
builder.Services.AddScoped<JwtService>(); // Servicio JWT

// Session para el carrito de compras
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Identity con Roles
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Configurar Cookie de Identity
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Auth/Login";
    options.LogoutPath = "/Auth/Logout";
    options.AccessDeniedPath = "/Auth/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

// JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "SuperSecretKey12345678901234567890"))
    };
});

// MVC + Razor
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Configurar Anti-Forgery para aceptar token en headers
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "RequestVerificationToken";
});

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

// ---------------- Seed Data (Admin User & Roles) ----------------
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await DataSeeder.SeedRolesAndAdminUser(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error al crear roles y usuario admin.");
    }
}

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

app.UseSession(); // Habilitar sesiones para el carrito

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
