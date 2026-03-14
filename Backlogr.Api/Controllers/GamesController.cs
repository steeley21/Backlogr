using Backlogr.Api.DTOs.Games;
using Backlogr.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Backlogr.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class GamesController : ControllerBase
{
    private readonly IGameService _gameService;

    public GamesController(IGameService gameService)
    {
        _gameService = gameService;
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
}