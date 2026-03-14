using Backlogr.Api.Common;
using Backlogr.Api.DTOs.Admin;
using Backlogr.Api.Models.Entities;
using Backlogr.Api.Options;
using Backlogr.Api.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Backlogr.Api.Services.Implementations;

public sealed class SuperAdminBootstrapService : ISuperAdminBootstrapService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly BootstrapSuperAdminOptions _options;

    public SuperAdminBootstrapService(
        UserManager<ApplicationUser> userManager,
        IOptions<BootstrapSuperAdminOptions> options)
    {
        _userManager = userManager;
        _options = options.Value;
    }

    public async Task<AdminUserSummaryDto> BootstrapAsync(
        string providedSecret,
        CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
        {
            throw new InvalidOperationException("SuperAdmin bootstrap is not enabled.");
        }

        if (string.IsNullOrWhiteSpace(_options.TargetEmail) || string.IsNullOrWhiteSpace(_options.Secret))
        {
            throw new InvalidOperationException(
                "SuperAdmin bootstrap configuration is incomplete. Set TargetEmail and Secret.");
        }

        if (string.IsNullOrWhiteSpace(providedSecret) || !string.Equals(providedSecret, _options.Secret, StringComparison.Ordinal))
        {
            throw new UnauthorizedAccessException("Bootstrap secret is invalid.");
        }

        var targetUser = await _userManager.FindByEmailAsync(_options.TargetEmail.Trim());
        if (targetUser is null)
        {
            throw new KeyNotFoundException("The configured bootstrap user was not found.");
        }

        var currentRoles = await _userManager.GetRolesAsync(targetUser);
        if (!currentRoles.Contains(RoleNames.Admin))
        {
            throw new InvalidOperationException(
                "The configured bootstrap user must already be an Admin before being elevated to SuperAdmin.");
        }

        if (currentRoles.Contains(RoleNames.SuperAdmin))
        {
            throw new InvalidOperationException("The configured bootstrap user is already a SuperAdmin.");
        }

        var addRoleResult = await _userManager.AddToRoleAsync(targetUser, RoleNames.SuperAdmin);
        if (!addRoleResult.Succeeded)
        {
            var errors = string.Join("; ", addRoleResult.Errors.Select(error => error.Description));
            throw new InvalidOperationException($"Failed to assign SuperAdmin role. Errors: {errors}");
        }

        var updatedRoles = await _userManager.GetRolesAsync(targetUser);
        return new AdminUserSummaryDto
        {
            UserId = targetUser.Id,
            UserName = targetUser.UserName ?? string.Empty,
            DisplayName = targetUser.DisplayName,
            Email = targetUser.Email ?? string.Empty,
            Roles = updatedRoles,
            CreatedAtUtc = DateTime.SpecifyKind(targetUser.CreatedAt, DateTimeKind.Utc),
        };
    }
}
