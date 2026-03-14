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

        var rolesToAssign = new List<string> { RoleNames.User };
        if (requestedRoleName == RoleNames.Admin)
        {
            rolesToAssign.Add(RoleNames.Admin);
        }

        var addRolesResult = await _userManager.AddToRolesAsync(user, rolesToAssign);
        if (!addRolesResult.Succeeded)
        {
            var errors = string.Join("; ", addRolesResult.Errors.Select(error => error.Description));
            throw new InvalidOperationException($"User created but roles could not be assigned. Errors: {errors}");
        }

        var assignedRoles = await _userManager.GetRolesAsync(user);
        return MapUser(user, assignedRoles);
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
