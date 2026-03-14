using Backlogr.Api.Common;
using Backlogr.Api.Models.Entities;
using Microsoft.AspNetCore.Identity;

namespace Backlogr.Api.Data;

public static class IdentityDataSeeder
{
    public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

        string[] roleNames =
        [
            RoleNames.User,
            RoleNames.Admin,
            RoleNames.SuperAdmin
        ];

        foreach (var roleName in roleNames)
        {
            var roleExists = await roleManager.RoleExistsAsync(roleName);
            if (roleExists)
            {
                continue;
            }

            var result = await roleManager.CreateAsync(new ApplicationRole
            {
                Name = roleName
            });

            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(error => error.Description));

                throw new InvalidOperationException(
                    $"Failed to seed role '{roleName}'. Errors: {errors}");
            }
        }
    }
}