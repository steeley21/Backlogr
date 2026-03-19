using Backlogr.Api.Data;
using Backlogr.Api.DTOs.AI;
using Backlogr.Api.Models.Entities;
using Backlogr.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backlogr.Api.Services.Implementations;

public sealed class AzureRecommendationService : IRecommendationService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ISemanticSearchService _semanticSearchService;

    public AzureRecommendationService(
        ApplicationDbContext dbContext,
        ISemanticSearchService semanticSearchService)
    {
        _dbContext = dbContext;
        _semanticSearchService = semanticSearchService;
    }

    public async Task<RecommendationResponseDto> GetRecommendationsAsync(Guid userId, int take = 5)
    {
        take = Math.Clamp(take, 1, 20);

        var userLogs = await _dbContext.GameLogs
            .AsNoTracking()
            .Include(log => log.Game)
            .Where(log => log.UserId == userId)
            .ToListAsync();

        var userReviews = await _dbContext.Reviews
            .AsNoTracking()
            .Include(review => review.Game)
            .Where(review => review.UserId == userId)
            .OrderByDescending(review => review.UpdatedAt)
            .ToListAsync();

        var excludedGameIds = userLogs
            .Select(log => log.GameId)
            .ToHashSet();

        if (userLogs.Count == 0 && userReviews.Count == 0)
        {
            return await BuildFallbackResponseAsync(excludedGameIds, take);
        }

        var profileQuery = BuildTasteProfileQuery(userLogs, userReviews);

        if (string.IsNullOrWhiteSpace(profileQuery))
        {
            return await BuildFallbackResponseAsync(excludedGameIds, take);
        }

        var favoriteGenres = GetTopTokens(
            userLogs
                .Where(log => log.Rating is >= 4.0m)
                .SelectMany(log => SplitTokens(log.Game.Genres)),
            5);

        var favoritePlatforms = GetTopTokens(
            userLogs
                .Where(log => log.Rating is >= 4.0m)
                .SelectMany(log => SplitTokens(log.Game.Platforms)),
            4);

        var candidates = await _semanticSearchService.SearchAsync(
            profileQuery,
            Math.Clamp(take * 4, 12, 40));

        var items = candidates
            .Where(candidate =>
                !excludedGameIds.Contains(candidate.GameId) &&
                !string.Equals(candidate.Title, "Test Game", StringComparison.OrdinalIgnoreCase))
            .Select(candidate => new
            {
                Candidate = candidate,
                GenreMatches = SplitTokens(candidate.Genres)
                    .Count(token => favoriteGenres.Contains(token, StringComparer.OrdinalIgnoreCase)),
                PlatformMatches = SplitTokens(candidate.Platforms)
                    .Count(token => favoritePlatforms.Contains(token, StringComparer.OrdinalIgnoreCase))
            })
            .OrderByDescending(x => x.GenreMatches)
            .ThenByDescending(x => x.PlatformMatches)
            .ThenByDescending(x => x.Candidate.Score)
            .Take(take)
            .Select(x => new RecommendedGameDto
            {
                GameId = x.Candidate.GameId,
                Title = x.Candidate.Title,
                CoverImageUrl = x.Candidate.CoverImageUrl,
                Why = BuildWhy(x.Candidate, x.GenreMatches, x.PlatformMatches)
            })
            .ToList();

        if (items.Count == 0)
        {
            return await BuildFallbackResponseAsync(excludedGameIds, take);
        }

        return new RecommendationResponseDto
        {
            Items = items
        };
    }

    private async Task<RecommendationResponseDto> BuildFallbackResponseAsync(
        HashSet<Guid> excludedGameIds,
        int take)
    {
        var items = await _dbContext.Games
            .AsNoTracking()
            .Where(game =>
                !excludedGameIds.Contains(game.GameId) &&
                game.Title != "Test Game")
            .OrderByDescending(game => game.ReleaseDate)
            .ThenBy(game => game.Title)
            .Take(take)
            .Select(game => new RecommendedGameDto
            {
                GameId = game.GameId,
                Title = game.Title,
                CoverImageUrl = game.CoverImageUrl,
                Why = "Add a few ratings or reviews to unlock more personalized recommendations."
            })
            .ToListAsync();

        return new RecommendationResponseDto
        {
            Items = items
        };
    }

    private static string BuildTasteProfileQuery(
        IReadOnlyList<GameLog> userLogs,
        IReadOnlyList<Review> userReviews)
    {
        var likedLogs = userLogs
            .Where(log => log.Rating is >= 4.0m)
            .OrderByDescending(log => log.Rating)
            .ThenByDescending(log => log.UpdatedAt)
            .Take(5)
            .ToList();

        var fallbackLogs = userLogs
            .OrderByDescending(log => log.UpdatedAt)
            .Take(5)
            .ToList();

        var sourceLogs = likedLogs.Count > 0 ? likedLogs : fallbackLogs;

        var favoriteTitles = sourceLogs
            .Select(log => log.Game.Title)
            .Where(title => !string.IsNullOrWhiteSpace(title))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(5)
            .ToList();

        var favoriteGenres = GetTopTokens(
            sourceLogs.SelectMany(log => SplitTokens(log.Game.Genres)),
            5);

        var favoritePlatforms = GetTopTokens(
            sourceLogs.SelectMany(log => SplitTokens(log.Game.Platforms)),
            4);

        var reviewSnippets = userReviews
            .Select(review => review.Text?.Trim())
            .Where(text => !string.IsNullOrWhiteSpace(text))
            .Select(text => Truncate(text!, 220))
            .Take(3)
            .ToList();

        var parts = new List<string>();

        if (favoriteTitles.Count > 0)
        {
            parts.Add($"Games this user liked: {string.Join(", ", favoriteTitles)}.");
        }

        if (favoriteGenres.Count > 0)
        {
            parts.Add($"Favorite genres: {string.Join(", ", favoriteGenres)}.");
        }

        if (favoritePlatforms.Count > 0)
        {
            parts.Add($"Favorite platforms: {string.Join(", ", favoritePlatforms)}.");
        }

        if (reviewSnippets.Count > 0)
        {
            parts.Add($"Review themes and preferences: {string.Join(" ", reviewSnippets)}");
        }

        return string.Join(" ", parts).Trim();
    }

    private static string BuildWhy(
        SemanticSearchResultDto candidate,
        int genreMatches,
        int platformMatches)
    {
        var matchedGenres = SplitTokens(candidate.Genres).Take(2).ToList();
        var matchedPlatforms = SplitTokens(candidate.Platforms).Take(2).ToList();

        if (genreMatches > 0 && platformMatches > 0)
        {
            return $"Recommended because it lines up with your favorite genres and platforms.";
        }

        if (genreMatches > 0)
        {
            return matchedGenres.Count > 0
                ? $"Recommended because it overlaps with genres you seem to like, including {string.Join(" and ", matchedGenres)}."
                : "Recommended because it overlaps with genres you seem to like.";
        }

        if (platformMatches > 0)
        {
            return matchedPlatforms.Count > 0
                ? $"Recommended because it fits platforms you play on, including {string.Join(" and ", matchedPlatforms)}."
                : "Recommended because it fits platforms you play on.";
        }

        return "Recommended because it semantically matches the games and review themes in your taste profile.";
    }

    private static List<string> GetTopTokens(IEnumerable<string> tokens, int take)
    {
        return tokens
            .Where(token => !string.IsNullOrWhiteSpace(token))
            .GroupBy(token => token, StringComparer.OrdinalIgnoreCase)
            .OrderByDescending(group => group.Count())
            .ThenBy(group => group.Key)
            .Take(take)
            .Select(group => group.Key)
            .ToList();
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

    private static string Truncate(string value, int maxLength)
    {
        if (value.Length <= maxLength)
        {
            return value;
        }

        return $"{value[..maxLength].TrimEnd()}...";
    }
}