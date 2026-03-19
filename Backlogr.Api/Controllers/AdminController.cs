using System.Security.Claims;
using Backlogr.Api.Common;
using Backlogr.Api.DTOs.Admin;
using Backlogr.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backlogr.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = RoleNames.Admin + "," + RoleNames.SuperAdmin)]
public sealed class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;
    private readonly IDemoSeedService _demoSeedService;

    public AdminController(IAdminService adminService, IDemoSeedService demoSeedService)
    {
        _adminService = adminService;
        _demoSeedService = demoSeedService;
    }

    [HttpGet("users")]
    public async Task<ActionResult<IReadOnlyList<AdminUserSummaryDto>>> GetUsers(CancellationToken cancellationToken)
    {
        var users = await _adminService.GetUsersAsync(cancellationToken);
        return Ok(users);
    }

    [HttpPost("users")]
    public async Task<ActionResult<AdminUserSummaryDto>> CreateUser(
        AdminCreateUserRequestDto dto,
        CancellationToken cancellationToken)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId is null)
        {
            return Unauthorized();
        }

        try
        {
            var createdUser = await _adminService.CreateUserAsync(currentUserId.Value, dto, cancellationToken);
            return Ok(createdUser);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpPost("demo/seed")]
    public async Task<ActionResult<AdminDemoSeedResponseDto>> SeedDemoData(
        [FromBody] AdminDemoSeedRequestDto? dto,
        CancellationToken cancellationToken)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId is null)
        {
            return Unauthorized();
        }

        try
        {
            var result = await _demoSeedService.SeedAsync(currentUserId.Value, dto, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    [HttpPut("users/{userId:guid}/role")]
    public async Task<ActionResult<AdminUserSummaryDto>> UpdateUserRole(
        Guid userId,
        AdminUpdateUserRoleRequestDto dto,
        CancellationToken cancellationToken)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId is null)
        {
            return Unauthorized();
        }

        try
        {
            var updatedUser = await _adminService.UpdateUserRoleAsync(currentUserId.Value, userId, dto, cancellationToken);
            return Ok(updatedUser);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    private Guid? GetCurrentUserId()
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (Guid.TryParse(userIdValue, out var userId))
        {
            return userId;
        }

        return null;
    }
}
