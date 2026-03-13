using Backlogr.Api.Data;
using Backlogr.Api.DTOs.Games;
using Backlogr.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backlogr.Api.Services.Implementations;

public sealed class GameService : IGameService
{
    private readonly ApplicationDbContext _dbContext;

    public GameService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
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
                Genres = g.Genres
            })
            .ToListAsync();
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
                UpdatedAt = g.UpdatedAt
            })
            .SingleOrDefaultAsync();
    }
}