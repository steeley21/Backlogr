using Backlogr.Api.Data;
using Backlogr.Api.DTOs.Igdb;
using Backlogr.Api.Models.Entities;
using Backlogr.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backlogr.Api.Services.Implementations;

public sealed class StubIgdbService : IIgdbService
{
    private readonly ApplicationDbContext _dbContext;

    private static readonly List<IgdbGameSearchResultDto> StubGames =
    [
        new()
        {
            IgdbId = 1001,
            Title = "Hollow Knight",
            Summary = "A challenging atmospheric metroidvania set in a fallen kingdom.",
            CoverImageUrl = "https://images.igdb.com/igdb/image/upload/t_cover_big/co1r7f.webp",
            ReleaseDate = new DateTime(2017, 2, 24),
            Developer = "Team Cherry",
            Publisher = "Team Cherry",
            Platforms = "PC, Switch, PlayStation, Xbox",
            Genres = "Metroidvania, Action, Adventure"
        },
        new()
        {
            IgdbId = 1002,
            Title = "Stardew Valley",
            Summary = "A cozy farming and life sim with relationships, crafting, and exploration.",
            CoverImageUrl = "https://images.igdb.com/igdb/image/upload/t_cover_big/co2o7x.webp",
            ReleaseDate = new DateTime(2016, 2, 26),
            Developer = "ConcernedApe",
            Publisher = "ConcernedApe",
            Platforms = "PC, Switch, PlayStation, Xbox, Mobile",
            Genres = "Simulation, RPG, Indie"
        },
        new()
        {
            IgdbId = 1003,
            Title = "Hades",
            Summary = "A fast-paced roguelike action game set in the Underworld.",
            CoverImageUrl = "https://images.igdb.com/igdb/image/upload/t_cover_big/co39vc.webp",
            ReleaseDate = new DateTime(2020, 9, 17),
            Developer = "Supergiant Games",
            Publisher = "Supergiant Games",
            Platforms = "PC, Switch, PlayStation, Xbox",
            Genres = "Roguelike, Action, Indie"
        }
    ];

    public StubIgdbService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<IReadOnlyList<IgdbGameSearchResultDto>> SearchGamesAsync(string query, int take = 10)
    {
        take = Math.Clamp(take, 1, 50);
        query = query.Trim();

        var results = StubGames
            .Where(g =>
                g.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                (g.Genres?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (g.Platforms?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false))
            .Take(take)
            .ToList();

        return Task.FromResult<IReadOnlyList<IgdbGameSearchResultDto>>(results);
    }

    public async Task<ImportedGameResponseDto> ImportGameAsync(long igdbId)
    {
        var stubGame = StubGames.SingleOrDefault(g => g.IgdbId == igdbId);

        if (stubGame is null)
        {
            throw new KeyNotFoundException("IGDB game was not found.");
        }

        var existingGame = await _dbContext.Games
            .SingleOrDefaultAsync(g => g.IgdbId == igdbId);

        if (existingGame is null)
        {
            existingGame = new Game
            {
                GameId = Guid.NewGuid(),
                IgdbId = stubGame.IgdbId,
                Title = stubGame.Title,
                Slug = ToSlug(stubGame.Title),
                Summary = stubGame.Summary,
                CoverImageUrl = stubGame.CoverImageUrl,
                ReleaseDate = stubGame.ReleaseDate,
                Developer = stubGame.Developer,
                Publisher = stubGame.Publisher,
                Platforms = stubGame.Platforms,
                Genres = stubGame.Genres,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _dbContext.Games.Add(existingGame);
        }
        else
        {
            existingGame.Title = stubGame.Title;
            existingGame.Slug = ToSlug(stubGame.Title);
            existingGame.Summary = stubGame.Summary;
            existingGame.CoverImageUrl = stubGame.CoverImageUrl;
            existingGame.ReleaseDate = stubGame.ReleaseDate;
            existingGame.Developer = stubGame.Developer;
            existingGame.Publisher = stubGame.Publisher;
            existingGame.Platforms = stubGame.Platforms;
            existingGame.Genres = stubGame.Genres;
            existingGame.UpdatedAt = DateTime.UtcNow;
        }

        await _dbContext.SaveChangesAsync();

        return new ImportedGameResponseDto
        {
            GameId = existingGame.GameId,
            IgdbId = existingGame.IgdbId ?? igdbId,
            Title = existingGame.Title,
            Slug = existingGame.Slug,
            Summary = existingGame.Summary,
            CoverImageUrl = existingGame.CoverImageUrl,
            ReleaseDate = existingGame.ReleaseDate,
            Developer = existingGame.Developer,
            Publisher = existingGame.Publisher,
            Platforms = existingGame.Platforms,
            Genres = existingGame.Genres,
            UpdatedAt = existingGame.UpdatedAt
        };
    }

    private static string ToSlug(string title)
    {
        return title
            .Trim()
            .ToLowerInvariant()
            .Replace("'", string.Empty)
            .Replace(" ", "-");
    }
}