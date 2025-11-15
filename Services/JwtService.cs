using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Tienda.Services
{
    public class JwtService
    {
     private readonly IConfiguration _configuration;
        private readonly UserManager<IdentityUser> _userManager;

        public JwtService(IConfiguration configuration, UserManager<IdentityUser> userManager)
  {
_configuration = configuration;
            _userManager = userManager;
        }

    public async Task<string> GenerateTokenAsync(IdentityUser user)
        {
       var claims = new List<Claim>
        {
              new Claim(ClaimTypes.NameIdentifier, user.Id),
      new Claim(ClaimTypes.Name, user.UserName ?? ""),
     new Claim(ClaimTypes.Email, user.Email ?? ""),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
         };

       // Agregar roles al token
    var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
 {
        claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
 _configuration["Jwt:Key"] ?? "SuperSecretKey12345678901234567890"));
  var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

   var token = new JwtSecurityToken(
  issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
    expires: DateTime.UtcNow.AddHours(8),
      signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
