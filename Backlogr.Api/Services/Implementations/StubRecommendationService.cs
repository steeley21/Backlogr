using Backlogr.Api.Data;
using Backlogr.Api.DTOs.AI;
using Backlogr.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backlogr.Api.Services.Implementations;

public sealed class StubRecommendationService : IRecommendationService
{
    private readonly ApplicationDbContext _dbContext;

    public StubRecommendationService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<RecommendationResponseDto> GetRecommendationsAsync(Guid userId, int take = 5)
    {
        take = Math.Clamp(take, 1, 20);

        var userLogs = await _dbContext.GameLogs
            .AsNoTracking()
            .Include(gl => gl.Game)
            .Where(gl => gl.UserId == userId)
            .ToListAsync();

        var loggedGameIds = userLogs.Select(gl => gl.GameId).ToHashSet();

        var likedGenreTokens = userLogs
            .Where(gl => gl.Rating is >= 4.0m)
            .SelectMany(gl => SplitTokens(gl.Game.Genres))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var likedPlatformTokens = userLogs
            .Where(gl => gl.Rating is >= 4.0m)
            .SelectMany(gl => SplitTokens(gl.Game.Platforms))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var candidates = await _dbContext.Games
            .AsNoTracking()
            .Where(g => !loggedGameIds.Contains(g.GameId))
            .ToListAsync();

        var items = candidates
            .Select(game =>
            {
                var genreMatches = SplitTokens(game.Genres).Count(token => likedGenreTokens.Contains(token));
                var platformMatches = SplitTokens(game.Platforms).Count(token => likedPlatformTokens.Contains(token));
                var score = genreMatches * 2 + platformMatches;

                var why = score > 0
                    ? $"Recommended because it overlaps with your liked genres/platforms ({score} shared signals)."
                    : "Recommended as a fallback from your current catalog.";

                return new
                {
                    Game = game,
                    Score = score,
                    Why = why
                };
            })
            .OrderByDescending(x => x.Score)
            .ThenBy(x => x.Game.Title)
            .Take(take)
            .Select(x => new RecommendedGameDto
            {
                GameId = x.Game.GameId,
                Title = x.Game.Title,
                CoverImageUrl = x.Game.CoverImageUrl,
                Why = x.Why
            })
            .ToList();

        return new RecommendationResponseDto
        {
            Items = items
        };
    }

    private static IEnumerable<string> SplitTokens(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return [];
        }

        return value
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(token => !string.IsNullOrWhiteSpace(token));
    }
}