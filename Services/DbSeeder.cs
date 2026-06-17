using BetFlowSystems.Models.DbModels;
using Microsoft.AspNetCore.Identity;

namespace BetFlowSystems.Services
{
    public static class DbSeeder
    {
        public static async Task SeedAdminAsync(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<Admin>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            string[] roles = { "Clerk", "Manager", "Admin" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            string adminEmail = "admin@betflowsystems.com";
            string adminPassword = "Admin@BFS-2026!";

            var existingAdmin = await userManager.FindByEmailAsync(adminEmail);

            if (existingAdmin == null)
            {
                var admin = new Admin
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                };

                var result = await userManager.CreateAsync(admin, adminPassword);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "Admin");
                }
                else
                {
                    throw new Exception("Failed to create admin user: " +
                        string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
        }
    }
}