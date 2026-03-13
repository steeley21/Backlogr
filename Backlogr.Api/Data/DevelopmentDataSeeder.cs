using Backlogr.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Backlogr.Api.Data;

public static class DevelopmentDataSeeder
{
    public static readonly Guid TestGameId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    public static async Task SeedTestGameAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var existingGame = await dbContext.Games
            .AsNoTracking()
            .AnyAsync(g => g.GameId == TestGameId);

        if (existingGame)
        {
            return;
        }

        dbContext.Games.Add(new Game
        {
            GameId = TestGameId,
            IgdbId = null,
            Title = "Test Game",
            Slug = "test-game",
            Summary = "Temporary development seed game for library endpoint testing.",
            CoverImageUrl = null,
            ReleaseDate = new DateTime(2024, 1, 1),
            Developer = "Backlogr Dev Seed",
            Publisher = "Backlogr Dev Seed",
            Platforms = "PC",
            Genres = "Adventure",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync();
    }
}