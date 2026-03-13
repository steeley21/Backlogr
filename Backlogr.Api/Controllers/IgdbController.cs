using Backlogr.Api.Common;
using Backlogr.Api.DTOs.Igdb;
using Backlogr.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backlogr.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class IgdbController : ControllerBase
{
    private readonly IIgdbService _igdbService;

    public IgdbController(IIgdbService igdbService)
    {
        _igdbService = igdbService;
    }

    [HttpGet("search")]
    [Authorize]
    public async Task<ActionResult<IReadOnlyList<IgdbGameSearchResultDto>>> SearchGames(
        [FromQuery] string query,
        [FromQuery] int take = 10)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return BadRequest("Query is required.");
        }

        var results = await _igdbService.SearchGamesAsync(query, take);
        return Ok(results);
    }

    [HttpPost("import/{igdbId:long}")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<ActionResult<ImportedGameResponseDto>> ImportGame(long igdbId)
    {
        try
        {
            var result = await _igdbService.ImportGameAsync(igdbId);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }
}