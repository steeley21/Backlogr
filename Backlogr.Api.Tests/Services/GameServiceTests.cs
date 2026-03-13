using Backlogr.Api.Data;
using Backlogr.Api.Models.Entities;
using Backlogr.Api.Services.Implementations;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Backlogr.Api.Tests.Services;

public sealed class GameServiceTests
{
    [Fact]
    public async Task SearchGamesAsync_ShouldReturnAllGames_WhenQueryIsNull()
    {
        var dbContext = CreateDbContext();

        dbContext.Games.AddRange(
            CreateGame("Hollow Knight", "Metroidvania", "PC"),
            CreateGame("Stardew Valley", "Simulation", "PC, Switch"));

        await dbContext.SaveChangesAsync();

        var service = new GameService(dbContext);

        var result = await service.SearchGamesAsync(null, take: 10);

        result.Should().HaveCount(2);
        result.Select(g => g.Title).Should().Contain(["Hollow Knight", "Stardew Valley"]);
    }

    [Fact]
    public async Task SearchGamesAsync_ShouldFilterByTitleGenreOrPlatform()
    {
        var dbContext = CreateDbContext();

        dbContext.Games.AddRange(
            CreateGame("Hollow Knight", "Metroidvania", "PC"),
            CreateGame("Stardew Valley", "Simulation", "Switch"),
            CreateGame("Hades", "Roguelike", "PC"));

        await dbContext.SaveChangesAsync();

        var service = new GameService(dbContext);

        var result = await service.SearchGamesAsync("Roguelike", take: 10);

        result.Should().ContainSingle();
        result[0].Title.Should().Be("Hades");
    }

    [Fact]
    public async Task GetGameByIdAsync_ShouldReturnDetail_WhenGameExists()
    {
        var dbContext = CreateDbContext();

        var game = CreateGame("Hollow Knight", "Metroidvania", "PC");
        game.Slug = "hollow-knight";
        game.Summary = "An atmospheric action adventure.";
        game.Developer = "Team Cherry";
        game.Publisher = "Team Cherry";

        dbContext.Games.Add(game);
        await dbContext.SaveChangesAsync();

        var service = new GameService(dbContext);

        var result = await service.GetGameByIdAsync(game.GameId);

        result.Should().NotBeNull();
        result!.GameId.Should().Be(game.GameId);
        result.Title.Should().Be("Hollow Knight");
        result.Slug.Should().Be("hollow-knight");
        result.Developer.Should().Be("Team Cherry");
    }

    [Fact]
    public async Task GetGameByIdAsync_ShouldReturnNull_WhenGameMissing()
    {
        var dbContext = CreateDbContext();

        var service = new GameService(dbContext);

        var result = await service.GetGameByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static Game CreateGame(string title, string? genres, string? platforms)
    {
        return new Game
        {
            GameId = Guid.NewGuid(),
            Title = title,
            Genres = genres,
            Platforms = platforms,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
}