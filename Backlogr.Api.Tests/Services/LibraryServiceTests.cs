using Backlogr.Api.Data;
using Backlogr.Api.DTOs.Library;
using Backlogr.Api.Models.Entities;
using Backlogr.Api.Models.Enums;
using Backlogr.Api.Services.Implementations;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Backlogr.Api.Tests.Services;

public sealed class LibraryServiceTests
{
    [Fact]
    public async Task GetLibraryAsync_ShouldReturnOnlyCurrentUsersLogs()
    {
        var dbContext = CreateDbContext();

        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        var game1 = CreateGame("Game One");
        var game2 = CreateGame("Game Two");

        dbContext.Games.AddRange(game1, game2);

        dbContext.GameLogs.AddRange(
            new GameLog
            {
                GameLogId = Guid.NewGuid(),
                UserId = userId,
                GameId = game1.GameId,
                Status = LibraryStatus.Backlog,
                UpdatedAt = DateTime.UtcNow,
                Game = game1
            },
            new GameLog
            {
                GameLogId = Guid.NewGuid(),
                UserId = otherUserId,
                GameId = game2.GameId,
                Status = LibraryStatus.Playing,
                UpdatedAt = DateTime.UtcNow,
                Game = game2
            });

        await dbContext.SaveChangesAsync();

        var service = new LibraryService(dbContext);

        var result = await service.GetLibraryAsync(userId);

        result.Should().HaveCount(1);
        result[0].GameId.Should().Be(game1.GameId);
        result[0].GameTitle.Should().Be("Game One");
    }

    [Fact]
    public async Task UpsertLibraryLogAsync_ShouldCreateNewLog_WhenNoneExists()
    {
        var dbContext = CreateDbContext();

        var userId = Guid.NewGuid();
        var game = CreateGame("Test Game");
        dbContext.Games.Add(game);
        await dbContext.SaveChangesAsync();

        var service = new LibraryService(dbContext);

        var dto = new UpsertLibraryLogRequestDto
        {
            GameId = game.GameId,
            Status = LibraryStatus.Backlog,
            Rating = 4.5m,
            Platform = "PC",
            Hours = 12.5m,
            StartedAt = new DateTime(2026, 3, 13, 0, 0, 0, DateTimeKind.Utc),
            Notes = "Testing create."
        };

        var result = await service.UpsertLibraryLogAsync(userId, dto);

        result.GameId.Should().Be(game.GameId);
        result.GameTitle.Should().Be("Test Game");
        result.Status.Should().Be(LibraryStatus.Backlog);
        result.Rating.Should().Be(4.5m);

        dbContext.GameLogs.Should().HaveCount(1);
    }

    [Fact]
    public async Task UpsertLibraryLogAsync_ShouldUpdateExistingLog_InsteadOfCreatingDuplicate()
    {
        var dbContext = CreateDbContext();

        var userId = Guid.NewGuid();
        var game = CreateGame("Test Game");
        dbContext.Games.Add(game);

        dbContext.GameLogs.Add(new GameLog
        {
            GameLogId = Guid.NewGuid(),
            UserId = userId,
            GameId = game.GameId,
            Status = LibraryStatus.Backlog,
            Rating = 3.0m,
            Platform = "PC",
            Hours = 5.0m,
            Notes = "Original",
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow.AddDays(-1),
            Game = game
        });

        await dbContext.SaveChangesAsync();

        var service = new LibraryService(dbContext);

        var dto = new UpsertLibraryLogRequestDto
        {
            GameId = game.GameId,
            Status = LibraryStatus.Played,
            Rating = 4.5m,
            Platform = "Steam Deck",
            Hours = 18.0m,
            StartedAt = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc),
            FinishedAt = new DateTime(2026, 3, 10, 0, 0, 0, DateTimeKind.Utc),
            Notes = "Updated"
        };

        var result = await service.UpsertLibraryLogAsync(userId, dto);

        result.Status.Should().Be(LibraryStatus.Played);
        result.Rating.Should().Be(4.5m);
        result.Platform.Should().Be("Steam Deck");
        result.Hours.Should().Be(18.0m);
        result.Notes.Should().Be("Updated");

        dbContext.GameLogs.Should().HaveCount(1);
    }

    [Fact]
    public async Task UpsertLibraryLogAsync_ShouldThrow_WhenRatingIsNotHalfStep()
    {
        var dbContext = CreateDbContext();

        var userId = Guid.NewGuid();
        var game = CreateGame("Test Game");
        dbContext.Games.Add(game);
        await dbContext.SaveChangesAsync();

        var service = new LibraryService(dbContext);

        var dto = new UpsertLibraryLogRequestDto
        {
            GameId = game.GameId,
            Status = LibraryStatus.Backlog,
            Rating = 4.3m
        };

        var act = async () => await service.UpsertLibraryLogAsync(userId, dto);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*0.5 increments*");
    }

    [Fact]
    public async Task DeleteLibraryLogAsync_ShouldDeleteOnlyOwnersLog()
    {
        var dbContext = CreateDbContext();

        var ownerUserId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var game = CreateGame("Delete Test");
        dbContext.Games.Add(game);

        dbContext.GameLogs.Add(new GameLog
        {
            GameLogId = Guid.NewGuid(),
            UserId = ownerUserId,
            GameId = game.GameId,
            Status = LibraryStatus.Backlog,
            UpdatedAt = DateTime.UtcNow,
            Game = game
        });

        await dbContext.SaveChangesAsync();

        var service = new LibraryService(dbContext);

        var deletedByOtherUser = await service.DeleteLibraryLogAsync(otherUserId, game.GameId);
        deletedByOtherUser.Should().BeFalse();
        dbContext.GameLogs.Should().HaveCount(1);

        var deletedByOwner = await service.DeleteLibraryLogAsync(ownerUserId, game.GameId);
        deletedByOwner.Should().BeTrue();
        dbContext.GameLogs.Should().BeEmpty();
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static Game CreateGame(string title)
    {
        return new Game
        {
            GameId = Guid.NewGuid(),
            Title = title,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
}