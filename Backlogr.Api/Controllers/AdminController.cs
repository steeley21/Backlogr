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

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
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
