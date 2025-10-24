using Microsoft.AspNetCore.Identity;
using GymManagmentSystem.Models;

namespace GymManagmentSystem.Services
{
    public class RoleSeederService
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<User> _userManager;

        public RoleSeederService(RoleManager<IdentityRole> roleManager, UserManager<User> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public async Task SeedRolesAsync()
        {
            string[] roles = { "Admin", "Staff", "Member" };

            foreach (string role in roles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    await _roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }

        public async Task SeedAdminUserAsync()
        {
            string adminEmail = "admin@gym.com";
            string adminPassword = "AdminPassword123!";

            var adminUser = await _userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new User
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = "Admin",
                    LastName = "User",
                    Gender = "Other",
                    Address = "Gym Office",
                    DateOfBirth = DateTime.UtcNow.AddYears(-30),
                    Occupation = "Gym Administrator",
                    CreatedBy = "System",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(adminUser, adminPassword);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
        }
    }
}
