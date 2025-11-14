using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Tienda.Models.DTOs;
using Tienda.Services;

namespace Tienda.Controllers
{
    public class AuthController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
    private readonly JwtService _jwtService;
     private readonly ILogger<AuthController> _logger;

        public AuthController(
     UserManager<IdentityUser> userManager,
   SignInManager<IdentityUser> signInManager,
JwtService jwtService,
 ILogger<AuthController> logger)
        {
   _userManager = userManager;
          _signInManager = signInManager;
         _jwtService = jwtService;
            _logger = logger;
        }

        // GET: /Auth/Login
        [HttpGet]
  [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
    {
           return RedirectToAction("Index", "Home");
        }

            ViewData["ReturnUrl"] = returnUrl;
     return View();
        }

 // POST: /Auth/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
   public async Task<IActionResult> Login(LoginDTO model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

   if (!ModelState.IsValid)
          {
           return View(model);
      }

     var user = await _userManager.FindByNameAsync(model.Username);
   if (user == null)
            {
       ModelState.AddModelError(string.Empty, "Usuario o contraseña incorrectos.");
      return View(model);
            }

       var result = await _signInManager.PasswordSignInAsync(
      model.Username,
   model.Password,
    model.RememberMe,
      lockoutOnFailure: false);

            if (result.Succeeded)
   {
    _logger.LogInformation($"Usuario {model.Username} inició sesión.");
           
  // Generar JWT para uso en APIs si es necesario
       var token = await _jwtService.GenerateTokenAsync(user);
    
           // Guardar el token en session (opcional)
    HttpContext.Session.SetString("JWTToken", token);

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
    return Redirect(returnUrl);
   }

        return RedirectToAction("Index", "Home");
         }

     if (result.IsLockedOut)
     {
             _logger.LogWarning($"Cuenta de usuario {model.Username} bloqueada.");
    ModelState.AddModelError(string.Empty, "Cuenta bloqueada.");
     return View(model);
   }

     ModelState.AddModelError(string.Empty, "Usuario o contraseña incorrectos.");
            return View(model);
        }

    // POST: /Auth/Logout
  [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
 await _signInManager.SignOutAsync();
        HttpContext.Session.Remove("JWTToken");
     _logger.LogInformation("Usuario cerró sesión.");
        return RedirectToAction("Login", "Auth");
    }

  // GET: /Auth/AccessDenied
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
