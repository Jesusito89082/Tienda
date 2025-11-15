using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Tienda.Data;
using Tienda.Models;

namespace Tienda.Services
{
    public class DataSeeder
    {
      public static async Task SeedRolesAndAdminUser(IServiceProvider serviceProvider)
 {
       var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
       var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
     var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
  var logger = serviceProvider.GetRequiredService<ILogger<DataSeeder>>();

        try
      {
                logger.LogInformation("?? Iniciando seeding de datos...");

                // Crear roles si no existen
    string[] roleNames = { "ADMINISTRADOR", "CAJERO", "USUARIO" };
  foreach (var roleName in roleNames)
   {
        var roleExist = await roleManager.RoleExistsAsync(roleName);
  if (!roleExist)
        {
       var result = await roleManager.CreateAsync(new IdentityRole(roleName));
      if (result.Succeeded)
     {
logger.LogInformation($"? Rol '{roleName}' creado exitosamente");
  }
          else
  {
      logger.LogError($"? Error al crear rol '{roleName}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
      }
       }
      else
        {
        logger.LogInformation($"??  Rol '{roleName}' ya existe");
}
 }

 // Crear usuario administrador
     var adminEmail = "admin@tienda.com";
    var adminUser = await userManager.FindByNameAsync("admin");

    if (adminUser == null)
{
           logger.LogInformation("? Creando usuario administrador...");
         
  var newAdminUser = new IdentityUser
    {
UserName = "admin",
   Email = adminEmail,
   EmailConfirmed = true,
             LockoutEnabled = false
         };

     var createAdmin = await userManager.CreateAsync(newAdminUser, "admin123");
        if (createAdmin.Succeeded)
{
    var roleResult = await userManager.AddToRoleAsync(newAdminUser, "ADMINISTRADOR");
    if (roleResult.Succeeded)
    {
 logger.LogInformation("? Usuario administrador 'admin' creado exitosamente");
    logger.LogInformation("?? Email: admin@tienda.com");
       logger.LogInformation("?? Contraseña: admin123");
logger.LogInformation("?? Rol: ADMINISTRADOR");
           }
   else
      {
      logger.LogError($"? Error al asignar rol: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
   }
     }
else
 {
        logger.LogError($"? Error al crear usuario admin: {string.Join(", ", createAdmin.Errors.Select(e => e.Description))}");
      }
       }
      else
  {
           logger.LogInformation("??Usuario 'admin' ya existe");

          // Verificar y asignar rol si no lo tiene
     var hasRole = await userManager.IsInRoleAsync(adminUser, "ADMINISTRADOR");
      if (!hasRole)
 {
          logger.LogInformation("? Asignando rol ADMINISTRADOR al usuario admin...");
   var roleResult = await userManager.AddToRoleAsync(adminUser, "ADMINISTRADOR");
       if (roleResult.Succeeded)
  {
      logger.LogInformation("? Rol ADMINISTRADOR asignado correctamente");
      }
 else
     {
  logger.LogError($"? Error al asignar rol: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
     }
       }
  else
  {
     logger.LogInformation("? Usuario admin ya tiene el rol ADMINISTRADOR");
    }
         }

   // Crear usuario cajero
            var cajeroEmail = "cajero@tienda.com";
     var cajeroUser = await userManager.FindByNameAsync("cajero");

         if (cajeroUser == null)
       {
                logger.LogInformation("? Creando usuario cajero...");

      var newCajeroUser = new IdentityUser
{
    UserName = "cajero",
   Email = cajeroEmail,
         EmailConfirmed = true,
LockoutEnabled = false
    };

  var createCajero = await userManager.CreateAsync(newCajeroUser, "cajero123");
if (createCajero.Succeeded)
    {
           var roleResult = await userManager.AddToRoleAsync(newCajeroUser, "CAJERO");
           if (roleResult.Succeeded)
{
          logger.LogInformation("? Usuario cajero 'cajero' creado exitosamente");
  logger.LogInformation("?? Email: cajero@tienda.com");
       logger.LogInformation("?? Contraseña: cajero123");
               logger.LogInformation("?? Rol: CAJERO");
   }
          else
                    {
       logger.LogError($"? Error al asignar rol: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
       }
                }
    else
     {
          logger.LogError($"? Error al crear usuario cajero: {string.Join(", ", createCajero.Errors.Select(e => e.Description))}");
   }
       }
            else
            {
          logger.LogInformation("??  Usuario 'cajero' ya existe");

  // Verificar y asignar rol si no lo tiene
                var hasRole = await userManager.IsInRoleAsync(cajeroUser, "CAJERO");
  if (!hasRole)
      {
          logger.LogInformation("? Asignando rol CAJERO al usuario cajero...");
         var roleResult = await userManager.AddToRoleAsync(cajeroUser, "CAJERO");
                 if (roleResult.Succeeded)
     {
     logger.LogInformation("? Rol CAJERO asignado correctamente");
        }
        else
   {
         logger.LogError($"? Error al asignar rol: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
              }
  }
       else
             {
 logger.LogInformation("? Usuario cajero ya tiene el rol CAJERO");
 }
            }

       // Verificación final
         logger.LogInformation("?? Verificación final de roles...");
     var finalAdmin = await userManager.FindByNameAsync("admin");
 if (finalAdmin != null)
          {
     var roles = await userManager.GetRolesAsync(finalAdmin);
      logger.LogInformation($"? Usuario 'admin' tiene los siguientes roles: {string.Join(", ", roles)}");
        }

            var finalCajero = await userManager.FindByNameAsync("cajero");
    if (finalCajero != null)
       {
      var roles = await userManager.GetRolesAsync(finalCajero);
         logger.LogInformation($"? Usuario 'cajero' tiene los siguientes roles: {string.Join(", ", roles)}");
    }

     // Seed de datos de prueba
         await SeedTestData(context, logger);

     logger.LogInformation("?? Seeding completado exitosamente");
            }
      catch (Exception ex)
      {
        logger.LogError(ex, "? Error durante el seeding de datos");
    throw;
       }
 }

        private static async Task SeedTestData(ApplicationDbContext context, ILogger logger)
  {
// Seed Categorías
    if (!await context.Categorias.AnyAsync())
       {
   var categorias = new List<Categoria>
     {
   new Categoria { Nombre = "Camisas" },
   new Categoria { Nombre = "Pantalones" },
    new Categoria { Nombre = "Zapatos" },
 new Categoria { Nombre = "Accesorios" }
 };

     context.Categorias.AddRange(categorias);
     await context.SaveChangesAsync();
         logger.LogInformation($"? {categorias.Count} categorías de prueba creadas");
   }

       // Seed Clientes
if (!await context.Clientes.AnyAsync())
          {
        var clientes = new List<Cliente>
      {
   new Cliente
            {
           Nombre = "Juan Pérez",
        Email = "juan.perez@ejemplo.com",
           Telefono = "8888-1111",
        Direccion = "San José, Costa Rica"
 },
  new Cliente
       {
   Nombre = "María González",
   Email = "maria.gonzalez@ejemplo.com",
       Telefono = "8888-2222",
    Direccion = "Heredia, Costa Rica"
         },
  new Cliente
        {
        Nombre = "Carlos Rodríguez",
   Email = "carlos.rodriguez@ejemplo.com",
         Telefono = "8888-3333",
            Direccion = "Alajuela, Costa Rica"
    }
     };

context.Clientes.AddRange(clientes);
   await context.SaveChangesAsync();
   logger.LogInformation($"? {clientes.Count} clientes de prueba creados");
            }

// Seed Productos
       if (!await context.Productos.AnyAsync())
       {
    var categorias = await context.Categorias.ToListAsync();

         var productos = new List<Producto>
  {
    new Producto
    {
      Nombre = "Camisa Casual Azul",
      CategoriaId = categorias.FirstOrDefault(c => c.Nombre == "Camisas")?.CategoriaId ?? 1,
            Precio = 15000,
           Stock = 25,
 Talla = "M",
      Color = "Azul"
 },
       new Producto
            {
           Nombre = "Pantalón Jean Negro",
           CategoriaId = categorias.FirstOrDefault(c => c.Nombre == "Pantalones")?.CategoriaId ?? 2,
          Precio = 25000,
           Stock = 15,
    Talla = "32",
  Color = "Negro"
         },
           new Producto
   {
       Nombre = "Zapatos Deportivos",
            CategoriaId = categorias.FirstOrDefault(c => c.Nombre == "Zapatos")?.CategoriaId ?? 3,
     Precio = 35000,
 Stock = 10,
        Talla = "42",
  Color = "Blanco"
        },
   new Producto
    {
   Nombre = "Gorra Negra",
    CategoriaId = categorias.FirstOrDefault(c => c.Nombre == "Accesorios")?.CategoriaId ?? 4,
     Precio = 8000,
    Stock = 30,
            Color = "Negro"
                  },
       new Producto
     {
        Nombre = "Camisa Formal Blanca",
    CategoriaId = categorias.FirstOrDefault(c => c.Nombre == "Camisas")?.CategoriaId ?? 1,
 Precio = 20000,
      Stock = 20,
 Talla = "L",
  Color = "Blanco"
      }
      };

 context.Productos.AddRange(productos);
     await context.SaveChangesAsync();
  logger.LogInformation($"? {productos.Count} productos de prueba creados");
    }
 }
    }
}
