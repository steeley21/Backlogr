using Backlogr.Api.Data;
using Backlogr.Api.DTOs.Games;
using Backlogr.Api.DTOs.Igdb;
using Backlogr.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backlogr.Api.Services.Implementations;

public sealed class GameService : IGameService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IIgdbService _igdbService;

    public GameService(ApplicationDbContext dbContext, IIgdbService igdbService)
    {
        _dbContext = dbContext;
        _igdbService = igdbService;
    }

    public async Task<IReadOnlyList<GameSummaryResponseDto>> SearchGamesAsync(string? query, int take = 25)
    {
        take = Math.Clamp(take, 1, 100);

        var gamesQuery = _dbContext.Games
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query))
        {
            var trimmed = query.Trim();

            gamesQuery = gamesQuery.Where(g =>
                g.Title.Contains(trimmed) ||
                (g.Slug != null && g.Slug.Contains(trimmed)) ||
                (g.Genres != null && g.Genres.Contains(trimmed)) ||
                (g.Platforms != null && g.Platforms.Contains(trimmed)));
        }

        return await gamesQuery
            .OrderBy(g => g.Title)
            .Take(take)
            .Select(g => new GameSummaryResponseDto
            {
                GameId = g.GameId,
                IgdbId = g.IgdbId,
                Title = g.Title,
                CoverImageUrl = g.CoverImageUrl,
                ReleaseDate = g.ReleaseDate,
                Platforms = g.Platforms,
                Genres = g.Genres,
            })
            .ToListAsync();
    }

    public async Task<IReadOnlyList<GameBrowseResultDto>> SearchBrowseGamesAsync(string? query, int take = 25)
    {
        take = Math.Clamp(take, 1, 100);

        var gamesQuery = _dbContext.Games
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query))
        {
            var trimmed = query.Trim();

            gamesQuery = gamesQuery.Where(g =>
                g.Title.Contains(trimmed) ||
                (g.Slug != null && g.Slug.Contains(trimmed)) ||
                (g.Genres != null && g.Genres.Contains(trimmed)) ||
                (g.Platforms != null && g.Platforms.Contains(trimmed)));
        }

        var localResults = await gamesQuery
            .OrderBy(g => g.Title)
            .Take(take)
            .Select(g => new GameBrowseResultDto
            {
                GameId = g.GameId,
                IgdbId = g.IgdbId,
                Title = g.Title,
                CoverImageUrl = g.CoverImageUrl,
                ReleaseDate = g.ReleaseDate,
                Platforms = g.Platforms,
                Genres = g.Genres,
            })
            .ToListAsync();

        if (string.IsNullOrWhiteSpace(query))
        {
            return localResults;
        }

        if (localResults.Count >= take)
        {
            return localResults;
        }

        var igdbResults = await _igdbService.SearchGamesAsync(query.Trim(), take);

        var igdbIds = igdbResults
            .Select(result => result.IgdbId)
            .Distinct()
            .ToList();

        var importedByIgdbId = igdbIds.Count == 0
            ? new Dictionary<long, GameBrowseResultDto>()
            : await _dbContext.Games
                .AsNoTracking()
                .Where(g => g.IgdbId.HasValue && igdbIds.Contains(g.IgdbId.Value))
                .Select(g => new GameBrowseResultDto
                {
                    GameId = g.GameId,
                    IgdbId = g.IgdbId,
                    Title = g.Title,
                    CoverImageUrl = g.CoverImageUrl,
                    ReleaseDate = g.ReleaseDate,
                    Platforms = g.Platforms,
                    Genres = g.Genres,
                })
                .ToDictionaryAsync(game => game.IgdbId!.Value);

        var mergedResults = new List<GameBrowseResultDto>(localResults);

        var seenGameIds = localResults
            .Where(game => game.GameId.HasValue)
            .Select(game => game.GameId!.Value)
            .ToHashSet();

        var seenIgdbIds = localResults
            .Where(game => game.IgdbId.HasValue)
            .Select(game => game.IgdbId!.Value)
            .ToHashSet();

        foreach (var igdbResult in igdbResults)
        {
            if (importedByIgdbId.TryGetValue(igdbResult.IgdbId, out var importedGame))
            {
                if (importedGame.GameId.HasValue && seenGameIds.Add(importedGame.GameId.Value))
                {
                    mergedResults.Add(importedGame);

                    if (importedGame.IgdbId.HasValue)
                    {
                        seenIgdbIds.Add(importedGame.IgdbId.Value);
                    }
                }
            }
            else if (seenIgdbIds.Add(igdbResult.IgdbId))
            {
                mergedResults.Add(MapIgdbSearchResultToBrowseResult(igdbResult));
            }

            if (mergedResults.Count >= take)
            {
                break;
            }
        }

        return mergedResults;
    }

    public async Task<GameDetailResponseDto?> GetGameByIdAsync(Guid gameId)
    {
        return await _dbContext.Games
            .AsNoTracking()
            .Where(g => g.GameId == gameId)
            .Select(g => new GameDetailResponseDto
            {
                GameId = g.GameId,
                IgdbId = g.IgdbId,
                Title = g.Title,
                Slug = g.Slug,
                Summary = g.Summary,
                CoverImageUrl = g.CoverImageUrl,
                ReleaseDate = g.ReleaseDate,
                Developer = g.Developer,
                Publisher = g.Publisher,
                Platforms = g.Platforms,
                Genres = g.Genres,
                CreatedAt = g.CreatedAt,
                UpdatedAt = g.UpdatedAt,
            })
            .SingleOrDefaultAsync();
    }

    private static GameBrowseResultDto MapIgdbSearchResultToBrowseResult(IgdbGameSearchResultDto game)
    {
        return new GameBrowseResultDto
        {
            GameId = null,
            IgdbId = game.IgdbId,
            Title = game.Title,
            CoverImageUrl = game.CoverImageUrl,
            ReleaseDate = game.ReleaseDate,
            Platforms = game.Platforms,
            Genres = game.Genres,
        };
    }
}