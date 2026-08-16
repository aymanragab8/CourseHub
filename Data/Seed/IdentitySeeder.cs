using CourseHub.Models;
using Microsoft.AspNetCore.Identity;

namespace CourseHub.Data.Seed
{
    public static class IdentitySeeder
    {
        public static async Task SeedRolesAsync(
            RoleManager<IdentityRole> roleManager)
        {
            string[] roles =
            {
                "Admin",
                "Instructor",
                "Student"
            };

            foreach (string role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(
                        new IdentityRole(role)
                    );
                }
            }
        }

        public static async Task SeedAdminAsync(
            UserManager<ApplicationUser> userManager)
        {
            string adminEmail =
                "admin@coursehub.com";

            string adminPassword =
                "Admin@123";

            ApplicationUser? admin =
                await userManager.FindByEmailAsync(
                    adminEmail
                );

            if (admin == null)
            {
                admin = new ApplicationUser
                {
                    FullName = "CourseHub Admin",
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                IdentityResult result =
                    await userManager.CreateAsync(
                        admin,
                        adminPassword
                    );

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(
                        admin,
                        "Admin"
                    );
                }
            }
        }
    }
}