using BravoBack.Models;
using Microsoft.AspNetCore.Identity;

namespace BravoBack.Data
{
    public static class DbSeeder
    {
        public static async Task SeedUsersAndRolesAsync(IServiceProvider serviceProvider)
        {
            // Se obtienen los servicios necesarios para crear usuarios y roles
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Roles base que debe tener el sistema
            string[] roles = { "Gerente", "Conductor" };

            // Se crea cada rol solo si no existe aun
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // Datos del usuario gerente por defecto
            var adminEmail = "b1gerente@bravo.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            // Si el usuario no existe, se crea
            if (adminUser == null)
            {
                var newAdmin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = "Bravo",
                    PaternalLastName = "Admin",
                    MaternalLastName = "Sistema",
                    EmailConfirmed = true
                };

                // Se intenta crear el usuario con la contrasena base
                var result = await userManager.CreateAsync(newAdmin, "987654321");

                // Si se creo bien, se asigna el rol de gerente
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(newAdmin, "Gerente");
                }
            }
            // Si ya existe, simplemente no se hace nada
        }
    }
}
