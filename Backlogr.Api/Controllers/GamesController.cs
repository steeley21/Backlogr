using System.Security.Claims;
using Backlogr.Api.DTOs.Feed;
using Backlogr.Api.DTOs.Games;
using Backlogr.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backlogr.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class GamesController : ControllerBase
{
    private readonly IGameService _gameService;
    private readonly IGameDetailsService _gameDetailsService;

    public GamesController(IGameService gameService, IGameDetailsService gameDetailsService)
    {
        _gameService = gameService;
        _gameDetailsService = gameDetailsService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<GameSummaryResponseDto>>> GetGames(
        [FromQuery] string? query,
        [FromQuery] int take = 25)
    {
        var games = await _gameService.SearchGamesAsync(query, take);
        return Ok(games);
    }

    [HttpGet("search")]
    public async Task<ActionResult<IReadOnlyList<GameBrowseResultDto>>> SearchBrowseGames(
        [FromQuery] string? query,
        [FromQuery] int take = 25)
    {
        var games = await _gameService.SearchBrowseGamesAsync(query, take);
        return Ok(games);
    }

    [HttpGet("{gameId:guid}")]
    public async Task<ActionResult<GameDetailResponseDto>> GetGameById(Guid gameId)
    {
        var game = await _gameService.GetGameByIdAsync(gameId);

        if (game is null)
        {
            return NotFound();
        }

        return Ok(game);
    }

    [Authorize]
    [HttpGet("{gameId:guid}/me")]
    public async Task<ActionResult<GameViewerStateResponseDto>> GetViewerState(Guid gameId)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var viewerState = await _gameDetailsService.GetViewerStateAsync(userId.Value, gameId);

        if (viewerState is null)
        {
            return NotFound();
        }

        return Ok(viewerState);
    }

    [Authorize]
    [HttpGet("{gameId:guid}/reviews")]
    public async Task<ActionResult<IReadOnlyList<FeedItemResponseDto>>> GetGameReviews(
        Guid gameId,
        [FromQuery] int take = 20)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var reviews = await _gameDetailsService.GetGameReviewsAsync(userId.Value, gameId, take);

        if (reviews is null)
        {
            return NotFound();
        }

        return Ok(reviews);
    }

    [Authorize]
    [HttpGet("{gameId:guid}/activity")]
    public async Task<ActionResult<IReadOnlyList<FeedItemResponseDto>>> GetGameActivity(
        Guid gameId,
        [FromQuery] int take = 25)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var activity = await _gameDetailsService.GetGameActivityAsync(userId.Value, gameId, take);

        if (activity is null)
        {
            return NotFound();
        }

        return Ok(activity);
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
