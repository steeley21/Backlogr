using System.Security.Claims;
using Backlogr.Api.DTOs.Profiles;
using Backlogr.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backlogr.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class ProfilesController : ControllerBase
{
    private readonly IProfileService _profileService;

    public ProfilesController(IProfileService profileService)
    {
        _profileService = profileService;
    }

    [HttpGet("{userName}")]
    public async Task<ActionResult<PublicProfileResponseDto>> GetPublicProfile(string userName)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId is null)
        {
            return Unauthorized();
        }

        try
        {
            var profile = await _profileService.GetPublicProfileAsync(currentUserId.Value, userName);
            return Ok(profile);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpGet("{userName}/library")]
    public async Task<ActionResult<IReadOnlyList<PublicProfileLibraryItemResponseDto>>> GetPublicLibrary(
        string userName)
    {
        try
        {
            var library = await _profileService.GetPublicLibraryAsync(userName);
            return Ok(library);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
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
