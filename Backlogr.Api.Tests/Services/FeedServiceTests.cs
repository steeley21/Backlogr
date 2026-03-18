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
    public async Task GetFeedAsync_ShouldReturnEmpty_WhenFollowingScopeHasNoActivityAndFollowsNoOne()
    {
        var dbContext = CreateDbContext();

        var user = CreateUser("feed_user", "feed_user@example.com", "Feed User");
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var service = new FeedService(dbContext);

        var result = await service.GetFeedAsync(user.Id, FeedScope.Following);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetFeedAsync_ShouldReturnAllUsersActivity_WhenScopeIsForYou()
    {
        var dbContext = CreateDbContext();

        var currentUser = CreateUser("current_user", "current_user@example.com", "Current User");
        var followedUser = CreateUser("followed_user", "followed_user@example.com", "Followed User");
        var otherUser = CreateUser("other_user", "other_user@example.com", "Other User");

        var game1 = CreateGame("Followed Game");
        var game2 = CreateGame("Other Game");
        var game3 = CreateGame("Current User Game");

        dbContext.Users.AddRange(currentUser, followedUser, otherUser);
        dbContext.Games.AddRange(game1, game2, game3);

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
            UpdatedAt = new DateTime(2026, 3, 13, 10, 0, 0, DateTimeKind.Utc),
            User = followedUser,
            Game = game1
        });

        dbContext.GameLogs.Add(new GameLog
        {
            GameLogId = Guid.NewGuid(),
            UserId = otherUser.Id,
            GameId = game2.GameId,
            Status = LibraryStatus.Backlog,
            UpdatedAt = new DateTime(2026, 3, 13, 11, 0, 0, DateTimeKind.Utc),
            User = otherUser,
            Game = game2
        });

        dbContext.Reviews.Add(new Review
        {
            ReviewId = Guid.NewGuid(),
            UserId = currentUser.Id,
            GameId = game3.GameId,
            Text = "My own review",
            HasSpoilers = false,
            CreatedAt = new DateTime(2026, 3, 13, 9, 0, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2026, 3, 13, 12, 0, 0, DateTimeKind.Utc),
            User = currentUser,
            Game = game3
        });

        await dbContext.SaveChangesAsync();

        var service = new FeedService(dbContext);

        var result = await service.GetFeedAsync(currentUser.Id, FeedScope.ForYou);

        result.Should().HaveCount(3);
        result.Select(item => item.UserId).Should().Contain(new[]
        {
            currentUser.Id,
            followedUser.Id,
            otherUser.Id
        });
    }

    [Fact]
    public async Task GetFeedAsync_ShouldReturnFollowedUsersAndCurrentUserActivity_WhenScopeIsFollowing()
    {
        var dbContext = CreateDbContext();

        var currentUser = CreateUser("current_user", "current_user@example.com", "Current User", avatarUrl: "https://example.com/current-user.png");
        var followedUser = CreateUser("followed_user", "followed_user@example.com", "Followed User", avatarUrl: "https://example.com/followed-user.png");
        var otherUser = CreateUser("other_user", "other_user@example.com", "Other User");

        var game1 = CreateGame("Followed Game");
        var game2 = CreateGame("Other Game");
        var game3 = CreateGame("Current User Game");

        dbContext.Users.AddRange(currentUser, followedUser, otherUser);
        dbContext.Games.AddRange(game1, game2, game3);

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

        var reviewId = Guid.NewGuid();
        var followedReview = new Review
        {
            ReviewId = reviewId,
            UserId = followedUser.Id,
            GameId = game1.GameId,
            Text = "Followed user's review",
            HasSpoilers = false,
            CreatedAt = new DateTime(2026, 3, 13, 11, 0, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2026, 3, 13, 13, 0, 0, DateTimeKind.Utc),
            User = followedUser,
            Game = game1
        };
        dbContext.Reviews.Add(followedReview);

        dbContext.Reviews.Add(new Review
        {
            ReviewId = Guid.NewGuid(),
            UserId = currentUser.Id,
            GameId = game3.GameId,
            Text = "My own review",
            HasSpoilers = false,
            CreatedAt = new DateTime(2026, 3, 13, 9, 0, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2026, 3, 13, 14, 0, 0, DateTimeKind.Utc),
            User = currentUser,
            Game = game3
        });

        dbContext.GameLogs.Add(new GameLog
        {
            GameLogId = Guid.NewGuid(),
            UserId = otherUser.Id,
            GameId = game2.GameId,
            Status = LibraryStatus.Backlog,
            UpdatedAt = new DateTime(2026, 3, 13, 15, 0, 0, DateTimeKind.Utc),
            User = otherUser,
            Game = game2
        });

        dbContext.ReviewLikes.Add(new ReviewLike
        {
            ReviewLikeId = Guid.NewGuid(),
            UserId = currentUser.Id,
            ReviewId = reviewId,
            CreatedAt = DateTime.UtcNow,
            User = currentUser,
            Review = followedReview
        });

        dbContext.ReviewComments.Add(new ReviewComment
        {
            ReviewCommentId = Guid.NewGuid(),
            UserId = currentUser.Id,
            ReviewId = reviewId,
            Text = "Nice review",
            CreatedAt = DateTime.UtcNow,
            User = currentUser,
            Review = followedReview
        });

        await dbContext.SaveChangesAsync();

        var service = new FeedService(dbContext);

        var result = await service.GetFeedAsync(currentUser.Id, FeedScope.Following);

        result.Should().HaveCount(3);
        result.Should().OnlyContain(item => item.UserId == currentUser.Id || item.UserId == followedUser.Id);
        result.Should().NotContain(item => item.UserId == otherUser.Id);

        result[0].UserId.Should().Be(currentUser.Id);
        result[0].IsOwner.Should().BeTrue();

        var followedReviewItem = result.Single(item => item.ItemType == FeedItemType.Review && item.UserId == followedUser.Id);
        followedReviewItem.AvatarUrl.Should().Be("https://example.com/followed-user.png");
        followedReviewItem.LikeCount.Should().Be(1);
        followedReviewItem.CommentCount.Should().Be(1);
        followedReviewItem.LikedByCurrentUser.Should().BeTrue();
        followedReviewItem.IsOwner.Should().BeFalse();

        var followedLogItem = result.Single(item => item.ItemType == FeedItemType.GameLog && item.UserId == followedUser.Id);
        followedLogItem.AvatarUrl.Should().Be("https://example.com/followed-user.png");
        followedLogItem.LikeCount.Should().Be(0);
        followedLogItem.CommentCount.Should().Be(0);
        followedLogItem.LikedByCurrentUser.Should().BeFalse();
        followedLogItem.IsOwner.Should().BeFalse();
    }

    [Fact]
    public async Task GetFeedAsync_ShouldIncludeCurrentUsersOwnActivity_AndMarkOwner_InFollowingScope()
    {
        var dbContext = CreateDbContext();

        var currentUser = CreateUser("self_feed_user", "self_feed_user@example.com", "Self Feed User", avatarUrl: "https://example.com/self-feed.png");
        var game = CreateGame("Self Feed Game");

        dbContext.Users.Add(currentUser);
        dbContext.Games.Add(game);
        dbContext.Reviews.Add(new Review
        {
            ReviewId = Guid.NewGuid(),
            UserId = currentUser.Id,
            GameId = game.GameId,
            Text = "My own review",
            HasSpoilers = false,
            CreatedAt = new DateTime(2026, 3, 13, 9, 0, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2026, 3, 13, 12, 0, 0, DateTimeKind.Utc),
            User = currentUser,
            Game = game
        });

        await dbContext.SaveChangesAsync();

        var service = new FeedService(dbContext);

        var result = await service.GetFeedAsync(currentUser.Id, FeedScope.Following);

        result.Should().ContainSingle();
        result[0].UserId.Should().Be(currentUser.Id);
        result[0].IsOwner.Should().BeTrue();
        result[0].AvatarUrl.Should().Be("https://example.com/self-feed.png");
    }

    [Fact]
    public async Task GetFeedAsync_ShouldRespectTakeLimit()
    {
        var dbContext = CreateDbContext();

        var currentUser = CreateUser("take_user", "take_user@example.com", "Take User");
        var followedUser = CreateUser("take_followed", "take_followed@example.com", "Take Followed");
        var otherUser = CreateUser("take_other", "take_other@example.com", "Take Other");
        var game = CreateGame("Take Game");

        dbContext.Users.AddRange(currentUser, followedUser, otherUser);
        dbContext.Games.Add(game);

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
            UserId = otherUser.Id,
            GameId = game.GameId,
            Text = "Latest review",
            HasSpoilers = false,
            CreatedAt = new DateTime(2026, 3, 13, 9, 0, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2026, 3, 13, 12, 0, 0, DateTimeKind.Utc),
            User = otherUser,
            Game = game
        });

        await dbContext.SaveChangesAsync();

        var service = new FeedService(dbContext);

        var result = await service.GetFeedAsync(currentUser.Id, FeedScope.ForYou, take: 2);

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

    private static ApplicationUser CreateUser(string userName, string email, string displayName, string? avatarUrl = null)
    {
        return new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = userName,
            Email = email,
            DisplayName = displayName,
            AvatarUrl = avatarUrl,
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