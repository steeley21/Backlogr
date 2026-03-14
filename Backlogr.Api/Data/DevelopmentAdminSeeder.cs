using Backlogr.Api.Common;
using Backlogr.Api.Models.Entities;
using Microsoft.AspNetCore.Identity;

namespace Backlogr.Api.Data;

public static class DevelopmentAdminSeeder
{
    public static async Task SeedAdminAsync(IServiceProvider serviceProvider, IConfiguration configuration)
    {
        using var scope = serviceProvider.CreateScope();

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var email = configuration["SeedAdmin:Email"];
        var userName = configuration["SeedAdmin:UserName"];
        var displayName = configuration["SeedAdmin:DisplayName"];
        var password = configuration["SeedAdmin:Password"];

        if (string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(userName) ||
            string.IsNullOrWhiteSpace(displayName) ||
            string.IsNullOrWhiteSpace(password))
        {
            return;
        }

        var existingUser = await userManager.FindByEmailAsync(email);

        if (existingUser is null)
        {
            var adminUser = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                Email = email,
                UserName = userName,
                DisplayName = displayName,
                CreatedAt = DateTime.UtcNow
            };

            var createResult = await userManager.CreateAsync(adminUser, password);

            if (!createResult.Succeeded)
            {
                var errors = string.Join("; ", createResult.Errors.Select(error => error.Description));
                throw new InvalidOperationException($"Failed to create development admin user. Errors: {errors}");
            }

            existingUser = adminUser;
        }

        if (!await userManager.IsInRoleAsync(existingUser, RoleNames.User))
        {
            var userRoleResult = await userManager.AddToRoleAsync(existingUser, RoleNames.User);

            if (!userRoleResult.Succeeded)
            {
                var errors = string.Join("; ", userRoleResult.Errors.Select(error => error.Description));
                throw new InvalidOperationException($"Failed to add development admin to User role. Errors: {errors}");
            }
        }

        if (!await userManager.IsInRoleAsync(existingUser, RoleNames.Admin))
        {
            var adminRoleResult = await userManager.AddToRoleAsync(existingUser, RoleNames.Admin);

            if (!adminRoleResult.Succeeded)
            {
                var errors = string.Join("; ", adminRoleResult.Errors.Select(error => error.Description));
                throw new InvalidOperationException($"Failed to add development admin to Admin role. Errors: {errors}");
            }
        }

        if (!await userManager.IsInRoleAsync(existingUser, RoleNames.SuperAdmin))
        {
            var superAdminRoleResult = await userManager.AddToRoleAsync(existingUser, RoleNames.SuperAdmin);

            if (!superAdminRoleResult.Succeeded)
            {
                var errors = string.Join("; ", superAdminRoleResult.Errors.Select(error => error.Description));
                throw new InvalidOperationException($"Failed to add development admin to SuperAdmin role. Errors: {errors}");
            }
        }
    }
}