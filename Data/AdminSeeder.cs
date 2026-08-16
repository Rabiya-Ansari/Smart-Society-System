using Microsoft.AspNetCore.Identity;

namespace SmartSociety.Data
{
    public static class AdminSeeder
    {
        public static async Task SeedAdminAsync(
            UserManager<ApplicationUser> userManager)
        {
            const string adminEmail = "admin@smartsociety.com";
            const string adminPassword = "Admin@12345";

            var existingAdmin = await userManager.FindByEmailAsync(adminEmail);

            if (existingAdmin == null)
            {
                var admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(
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
            else
            {
                if (!await userManager.IsInRoleAsync(
                    existingAdmin,
                    "Admin"))
                {
                    await userManager.AddToRoleAsync(
                        existingAdmin,
                        "Admin"
                    );
                }
            }
        }
    }
}