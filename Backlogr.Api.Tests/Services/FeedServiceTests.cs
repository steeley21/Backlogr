using Backlogr.Api.Data;
using Backlogr.Api.DTOs.Feed;
using Backlogr.Api.Models.Entities;
using Backlogr.Api.Models.Enums;
using Backlogr.Api.Services.Implementations;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Backlogr.Api.Tests.Services;

public sealed class FeedServiceTests
{
    [Fact]
    public async Task GetFeedAsync_ShouldReturnEmpty_WhenUserFollowsNoOne()
    {
        var dbContext = CreateDbContext();

        var user = CreateUser("feed_user", "feed_user@example.com", "Feed User");
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var service = new FeedService(dbContext);

        var result = await service.GetFeedAsync(user.Id);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetFeedAsync_ShouldReturnLogsAndReviews_FromFollowedUsersOnly()
    {
        var dbContext = CreateDbContext();

        var currentUser = CreateUser("current_user", "current_user@example.com", "Current User");
        var followedUser = CreateUser("followed_user", "followed_user@example.com", "Followed User");
        var otherUser = CreateUser("other_user", "other_user@example.com", "Other User");

        var game1 = CreateGame("Followed Game");
        var game2 = CreateGame("Other Game");

        dbContext.Users.AddRange(currentUser, followedUser, otherUser);
        dbContext.Games.AddRange(game1, game2);

        dbContext.Follows.Add(new Follow
        {
            FollowId = Guid.NewGuid(),
            FollowerId = currentUser.Id,
            FollowingId = followedUser.Id,
            CreatedAt = DateTime.UtcNow,
            Follower = currentUser,
            Following = followedUser
        });

        dbContext.GameLogs.Add(new GameLog
        {
            GameLogId = Guid.NewGuid(),
            UserId = followedUser.Id,
            GameId = game1.GameId,
            Status = LibraryStatus.Playing,
            Rating = 4.5m,
            Platform = "PC",
            Hours = 10.0m,
            UpdatedAt = new DateTime(2026, 3, 13, 12, 0, 0, DateTimeKind.Utc),
            User = followedUser,
            Game = game1
        });

        dbContext.Reviews.Add(new Review
        {
            ReviewId = Guid.NewGuid(),
            UserId = followedUser.Id,
            GameId = game1.GameId,
            Text = "Followed user's review",
            HasSpoilers = false,
            CreatedAt = new DateTime(2026, 3, 13, 11, 0, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2026, 3, 13, 13, 0, 0, DateTimeKind.Utc),
            User = followedUser,
            Game = game1
        });

        dbContext.GameLogs.Add(new GameLog
        {
            GameLogId = Guid.NewGuid(),
            UserId = otherUser.Id,
            GameId = game2.GameId,
            Status = LibraryStatus.Backlog,
            UpdatedAt = new DateTime(2026, 3, 13, 14, 0, 0, DateTimeKind.Utc),
            User = otherUser,
            Game = game2
        });

        await dbContext.SaveChangesAsync();

        var service = new FeedService(dbContext);

        var result = await service.GetFeedAsync(currentUser.Id);

        result.Should().HaveCount(2);
        result.Should().OnlyContain(item => item.UserId == followedUser.Id);
        result[0].ItemType.Should().Be(FeedItemType.Review);
        result[0].GameTitle.Should().Be("Followed Game");
        result[1].ItemType.Should().Be(FeedItemType.GameLog);
    }

    [Fact]
    public async Task GetFeedAsync_ShouldRespectTakeLimit()
    {
        var dbContext = CreateDbContext();

        var currentUser = CreateUser("take_user", "take_user@example.com", "Take User");
        var followedUser = CreateUser("take_followed", "take_followed@example.com", "Take Followed");
        var game = CreateGame("Take Game");

        dbContext.Users.AddRange(currentUser, followedUser);
        dbContext.Games.Add(game);

        dbContext.Follows.Add(new Follow
        {
            FollowId = Guid.NewGuid(),
            FollowerId = currentUser.Id,
            FollowingId = followedUser.Id,
            CreatedAt = DateTime.UtcNow,
            Follower = currentUser,
            Following = followedUser
        });

        dbContext.GameLogs.AddRange(
            new GameLog
            {
                GameLogId = Guid.NewGuid(),
                UserId = followedUser.Id,
                GameId = game.GameId,
                Status = LibraryStatus.Playing,
                UpdatedAt = new DateTime(2026, 3, 13, 10, 0, 0, DateTimeKind.Utc),
                User = followedUser,
                Game = game
            },
            new GameLog
            {
                GameLogId = Guid.NewGuid(),
                UserId = followedUser.Id,
                GameId = game.GameId,
                Status = LibraryStatus.Played,
                UpdatedAt = new DateTime(2026, 3, 13, 11, 0, 0, DateTimeKind.Utc),
                User = followedUser,
                Game = game
            });

        dbContext.Reviews.Add(new Review
        {
            ReviewId = Guid.NewGuid(),
            UserId = followedUser.Id,
            GameId = game.GameId,
            Text = "Latest review",
            HasSpoilers = false,
            CreatedAt = new DateTime(2026, 3, 13, 9, 0, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2026, 3, 13, 12, 0, 0, DateTimeKind.Utc),
            User = followedUser,
            Game = game
        });

        await dbContext.SaveChangesAsync();

        var service = new FeedService(dbContext);

        var result = await service.GetFeedAsync(currentUser.Id, take: 2);

        result.Should().HaveCount(2);
        result[0].ActivityAt.Should().BeAfter(result[1].ActivityAt);
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static ApplicationUser CreateUser(string userName, string email, string displayName)
    {
        return new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = userName,
            Email = email,
            DisplayName = displayName,
            CreatedAt = DateTime.UtcNow
        };
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