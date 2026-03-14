using Backlogr.Api.Common;
using Backlogr.Api.DTOs.Admin;
using Backlogr.Api.Models.Entities;
using Backlogr.Api.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Backlogr.Api.Services.Implementations;

public sealed class AdminService : IAdminService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<IReadOnlyList<AdminUserSummaryDto>> GetUsersAsync(CancellationToken cancellationToken = default)
    {
        var users = await _userManager.Users
            .OrderByDescending(user => user.CreatedAt)
            .ThenBy(user => user.UserName)
            .ToListAsync(cancellationToken);

        var result = new List<AdminUserSummaryDto>(users.Count);

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            result.Add(MapUser(user, roles));
        }

        return result;
    }

    public async Task<AdminUserSummaryDto> CreateUserAsync(
        Guid currentUserId,
        AdminCreateUserRequestDto dto,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var currentUser = await _userManager.FindByIdAsync(currentUserId.ToString());
        if (currentUser is null)
        {
            throw new UnauthorizedAccessException("Current user was not found.");
        }

        if (string.IsNullOrWhiteSpace(dto.UserName) ||
            string.IsNullOrWhiteSpace(dto.DisplayName) ||
            string.IsNullOrWhiteSpace(dto.Email) ||
            string.IsNullOrWhiteSpace(dto.Password))
        {
            throw new ArgumentException("Username, display name, email, password, and role are required.");
        }

        var requestedRoleName = dto.Role switch
        {
            AdminAssignableRole.User => RoleNames.User,
            AdminAssignableRole.Admin => RoleNames.Admin,
            _ => throw new ArgumentOutOfRangeException(nameof(dto.Role), "Unsupported role selection.")
        };

        var canCreateAdmin = await _userManager.IsInRoleAsync(currentUser, RoleNames.SuperAdmin);

        if (requestedRoleName == RoleNames.Admin && !canCreateAdmin)
        {
            throw new UnauthorizedAccessException("Only SuperAdmin can create Admin accounts.");
        }

        var existingUserByEmail = await _userManager.FindByEmailAsync(dto.Email);
        if (existingUserByEmail is not null)
        {
            throw new InvalidOperationException("An account with that email already exists.");
        }

        var existingUserByUserName = await _userManager.FindByNameAsync(dto.UserName);
        if (existingUserByUserName is not null)
        {
            throw new InvalidOperationException("That username is already taken.");
        }

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = dto.UserName.Trim(),
            DisplayName = dto.DisplayName.Trim(),
            Email = dto.Email.Trim(),
            CreatedAt = DateTime.UtcNow,
        };

        var createResult = await _userManager.CreateAsync(user, dto.Password);
        if (!createResult.Succeeded)
        {
            var errors = string.Join("; ", createResult.Errors.Select(error => error.Description));
            throw new ArgumentException(errors);
        }

        var rolesToAssign = BuildRolesForAssignableRole(dto.Role);

        var addRolesResult = await _userManager.AddToRolesAsync(user, rolesToAssign);
        if (!addRolesResult.Succeeded)
        {
            var errors = string.Join("; ", addRolesResult.Errors.Select(error => error.Description));
            throw new InvalidOperationException($"User created but roles could not be assigned. Errors: {errors}");
        }

        var assignedRoles = await _userManager.GetRolesAsync(user);
        return MapUser(user, assignedRoles);
    }

    public async Task<AdminUserSummaryDto> UpdateUserRoleAsync(
        Guid currentUserId,
        Guid targetUserId,
        AdminUpdateUserRoleRequestDto dto,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var currentUser = await _userManager.FindByIdAsync(currentUserId.ToString());
        if (currentUser is null)
        {
            throw new UnauthorizedAccessException("Current user was not found.");
        }

        var isCurrentUserSuperAdmin = await _userManager.IsInRoleAsync(currentUser, RoleNames.SuperAdmin);
        if (!isCurrentUserSuperAdmin)
        {
            throw new UnauthorizedAccessException("Only SuperAdmin can change user roles.");
        }

        if (currentUserId == targetUserId)
        {
            throw new InvalidOperationException("You cannot change your own role from the admin dashboard.");
        }

        var targetUser = await _userManager.FindByIdAsync(targetUserId.ToString());
        if (targetUser is null)
        {
            throw new KeyNotFoundException("That user was not found.");
        }

        var targetUserRoles = await _userManager.GetRolesAsync(targetUser);
        if (targetUserRoles.Contains(RoleNames.SuperAdmin))
        {
            throw new InvalidOperationException("SuperAdmin accounts cannot be edited from this dashboard.");
        }

        var desiredRoles = BuildRolesForAssignableRole(dto.Role);

        var rolesToRemove = targetUserRoles
            .Where(role => role is RoleNames.User or RoleNames.Admin)
            .Except(desiredRoles)
            .ToArray();

        if (rolesToRemove.Length > 0)
        {
            var removeResult = await _userManager.RemoveFromRolesAsync(targetUser, rolesToRemove);
            if (!removeResult.Succeeded)
            {
                var errors = string.Join("; ", removeResult.Errors.Select(error => error.Description));
                throw new InvalidOperationException($"Failed to remove existing roles. Errors: {errors}");
            }
        }

        var rolesToAdd = desiredRoles
            .Except(targetUserRoles)
            .ToArray();

        if (rolesToAdd.Length > 0)
        {
            var addResult = await _userManager.AddToRolesAsync(targetUser, rolesToAdd);
            if (!addResult.Succeeded)
            {
                var errors = string.Join("; ", addResult.Errors.Select(error => error.Description));
                throw new InvalidOperationException($"Failed to assign the new role. Errors: {errors}");
            }
        }

        var updatedRoles = await _userManager.GetRolesAsync(targetUser);
        return MapUser(targetUser, updatedRoles);
    }

    private static List<string> BuildRolesForAssignableRole(AdminAssignableRole role)
    {
        return role switch
        {
            AdminAssignableRole.User => [RoleNames.User],
            AdminAssignableRole.Admin => [RoleNames.User, RoleNames.Admin],
            _ => throw new ArgumentOutOfRangeException(nameof(role), "Unsupported role selection.")
        };
    }

    private static AdminUserSummaryDto MapUser(ApplicationUser user, IList<string> roles)
    {
        return new AdminUserSummaryDto
        {
            UserId = user.Id,
            UserName = user.UserName ?? string.Empty,
            DisplayName = user.DisplayName,
            Email = user.Email ?? string.Empty,
            Roles = roles,
            CreatedAtUtc = DateTime.SpecifyKind(user.CreatedAt, DateTimeKind.Utc),
        };
    }
}
