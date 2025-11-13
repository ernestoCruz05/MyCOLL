using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MyCOLL.Data
{
    public static class Inicializacao
    {
        public static async Task SeedDataAsync(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            // migrations
            await context.Database.MigrateAsync();

            // criar roles base
            var roles = new[] { "Admin", "Gestor", "Fornecedor", "Cliente" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }


            // criar admin default
            if (await userManager.FindByEmailAsync("admin@mycoll.pt") == null)
            {
                var admin = new ApplicationUser
                {
                    UserName = "admin@mycoll.pt",
                    Email = "admin@mycoll.pt",
                    EmailConfirmed = true
                };

                await userManager.CreateAsync(admin, "Admin123!");
                await userManager.AddToRoleAsync(admin, "Admin");
            }
        }
    }
}
