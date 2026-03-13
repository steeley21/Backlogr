using Backlogr.Api.Data;
using Backlogr.Api.DTOs.AI;
using Backlogr.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backlogr.Api.Services.Implementations;

public sealed class StubSemanticSearchService : ISemanticSearchService
{
    private readonly ApplicationDbContext _dbContext;

    public StubSemanticSearchService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<SemanticSearchResultDto>> SearchAsync(string query, int take = 10)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            throw new ArgumentException("Query is required.");
        }

        take = Math.Clamp(take, 1, 20);

        var queryTokens = query
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(token => token.ToLowerInvariant())
            .Distinct()
            .ToList();

        var games = await _dbContext.Games
            .AsNoTracking()
            .ToListAsync();

        var results = games
            .Select(game =>
            {
                var searchable = string.Join(
                    " ",
                    game.Title,
                    game.Summary ?? string.Empty,
                    game.Genres ?? string.Empty,
                    game.Platforms ?? string.Empty)
                    .ToLowerInvariant();

                var score = queryTokens.Count(token => searchable.Contains(token));

                return new
                {
                    Game = game,
                    Score = score
                };
            })
            .Where(x => x.Score > 0)
            .OrderByDescending(x => x.Score)
            .ThenBy(x => x.Game.Title)
            .Take(take)
            .Select(x => new SemanticSearchResultDto
            {
                GameId = x.Game.GameId,
                Title = x.Game.Title,
                CoverImageUrl = x.Game.CoverImageUrl,
                Summary = x.Game.Summary,
                Genres = x.Game.Genres,
                Platforms = x.Game.Platforms,
                Score = x.Score,
                Why = $"Matched {x.Score} query term(s) in title/summary/genres/platforms."
            })
            .ToList();

        return results;
    }
}