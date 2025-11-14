using Microsoft.AspNetCore.Identity;
using Tienda.Data;

namespace Tienda.Services
{
    public class DataSeeder
    {
  public static async Task SeedRolesAndAdminUser(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
  var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
  var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

        // Crear roles si no existen
            string[] roleNames = { "ADMINISTRADOR", "USUARIO" };
            foreach (var roleName in roleNames)
        {
   var roleExist = await roleManager.RoleExistsAsync(roleName);
    if (!roleExist)
       {
     await roleManager.CreateAsync(new IdentityRole(roleName));
    }
   }

  // Crear usuario administrador
            var adminEmail = "admin@tienda.com";
    var adminUser = await userManager.FindByNameAsync("admin");

            if (adminUser == null)
  {
 var newAdminUser = new IdentityUser
         {
         UserName = "admin",
       Email = adminEmail,
         EmailConfirmed = true
         };

          var createAdmin = await userManager.CreateAsync(newAdminUser, "admin123");
     if (createAdmin.Succeeded)
   {
         await userManager.AddToRoleAsync(newAdminUser, "ADMINISTRADOR");
       }
     }
        }
    }
}
