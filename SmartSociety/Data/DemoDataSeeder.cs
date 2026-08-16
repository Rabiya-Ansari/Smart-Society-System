using Microsoft.AspNetCore.Identity;
using SmartSociety.Models;

namespace SmartSociety.Data
{
    public static class DemoDataSeeder
    {
        public static async Task SeedStaffAsync(UserManager<ApplicationUser> userManager)
        {
            await EnsureUserAsync(userManager, "guard@smartsociety.com", "Guard@12345", "SecurityStaff");
            await EnsureUserAsync(userManager, "staff@smartsociety.com", "Staff@12345", "MaintenanceStaff");
        }

        private static async Task EnsureUserAsync(UserManager<ApplicationUser> userManager, string email, string password, string role)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(user, password);
                if (!result.Succeeded)
                    throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));
            }

            if (!await userManager.IsInRoleAsync(user, role))
            {
                var result = await userManager.AddToRoleAsync(user, role);
                if (!result.Succeeded)
                    throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));
            }
        }
    }
}
