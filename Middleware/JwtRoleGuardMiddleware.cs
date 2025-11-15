using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Tienda.Middleware
{
    public class JwtRoleGuardMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<JwtRoleGuardMiddleware> _logger;

     public JwtRoleGuardMiddleware(RequestDelegate next, ILogger<JwtRoleGuardMiddleware> logger)
    {
    _next = next;
            _logger = logger;
 }

  public async Task InvokeAsync(HttpContext context)
        {
  var path = context.Request.Path.Value?.ToLower() ?? "";

            // Rutas protegidas que requieren rol ADMINISTRADOR
            var adminRoutes = new[]
      {
     "/categorias",
          "/ventas",
   "/detallesventa",
  "/detallesventums",
     "/clientes",
      "/facturas",
        "/admin"
            };

            // Rutas que siempre son públicas
 var publicRoutes = new[]
            {
      "/auth/login",
     "/auth/logout",
                "/home",
                "/productoes/index",
                "/productoes/details",
"/carrito",
          "/",
       "/_framework",
    "/lib",
 "/css",
       "/js",
  "/images"
     };

          // Si es una ruta pública, permitir acceso
            if (publicRoutes.Any(route => path.StartsWith(route)))
            {
                await _next(context);
     return;
            }

    // Si es una ruta de admin, verificar token y rol
          if (adminRoutes.Any(route => path.StartsWith(route)))
    {
                var token = GetTokenFromRequest(context);

           if (string.IsNullOrEmpty(token))
 {
       _logger.LogWarning($"Acceso denegado a {path}: Token no encontrado");
context.Response.Redirect("/Auth/AccessDenied");
 return;
 }

         // Validar que el token contenga el rol ADMINISTRADOR
     if (!HasAdminRole(token))
   {
                 _logger.LogWarning($"Acceso denegado a {path}: Usuario sin rol ADMINISTRADOR");
    context.Response.Redirect("/Auth/AccessDenied");
        return;
  }

 _logger.LogInformation($"Acceso permitido a {path}: Usuario con rol ADMINISTRADOR");
  }

await _next(context);
        }

    private string? GetTokenFromRequest(HttpContext context)
        {
  // Intentar obtener desde cookie
         var token = context.Request.Cookies["JWTToken"];

            if (string.IsNullOrEmpty(token))
            {
            // Intentar desde header Authorization
      var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
         if (authHeader?.StartsWith("Bearer ") == true)
     {
           token = authHeader.Substring("Bearer ".Length).Trim();
  }
            }

   if (string.IsNullOrEmpty(token))
    {
     // Intentar desde session (fallback)
  token = context.Session.GetString("JWTToken");
     }

        return token;
        }

    private bool HasAdminRole(string token)
        {
         try
            {
  var handler = new JwtSecurityTokenHandler();
    var jsonToken = handler.ReadToken(token) as JwtSecurityToken;

       if (jsonToken == null)
 return false;

          // Buscar el claim de rol
         var roleClaims = jsonToken.Claims
     .Where(c => c.Type == ClaimTypes.Role || c.Type == "role")
    .Select(c => c.Value)
   .ToList();

                return roleClaims.Any(r => r.Equals("ADMINISTRADOR", StringComparison.OrdinalIgnoreCase));
            }
            catch (Exception ex)
            {
  _logger.LogError(ex, "Error al validar token JWT");
        return false;
            }
        }
    }

    // Extensión para facilitar el registro del middleware
    public static class JwtRoleGuardMiddlewareExtensions
    {
        public static IApplicationBuilder UseJwtRoleGuard(this IApplicationBuilder builder)
        {
  return builder.UseMiddleware<JwtRoleGuardMiddleware>();
        }
    }
}
