using BloodDonor.Domain.Constants;
using Microsoft.AspNetCore.Identity;
using System.Data;

namespace BloodDonor.Infrastructure.Identity
{
    public static class RoleSeeder
    {
        public static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            foreach (var roleName in Roles.All)
            {
                var exists = await roleManager.RoleExistsAsync(roleName);
                if (!exists)
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
        }
    }
}