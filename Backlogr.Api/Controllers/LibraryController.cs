using System.Security.Claims;
using Backlogr.Api.DTOs.Library;
using Backlogr.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backlogr.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class LibraryController : ControllerBase
{
    private readonly ILibraryService _libraryService;

    public LibraryController(ILibraryService libraryService)
    {
        _libraryService = libraryService;
    }

    [HttpGet("me")]
    public async Task<ActionResult<IReadOnlyList<LibraryLogResponseDto>>> GetMyLibrary()
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var library = await _libraryService.GetLibraryAsync(userId.Value);
        return Ok(library);
    }

    [HttpPost]
    public async Task<ActionResult<LibraryLogResponseDto>> UpsertLibraryLog(
        UpsertLibraryLogRequestDto dto)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        try
        {
            var result = await _libraryService.UpsertLibraryLogAsync(userId.Value, dto);
            return Ok(result);
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

    [HttpDelete("{gameId:guid}")]
    public async Task<IActionResult> DeleteLibraryLog(Guid gameId)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var deleted = await _libraryService.DeleteLibraryLogAsync(userId.Value, gameId);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
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