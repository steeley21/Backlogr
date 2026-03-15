using Backlogr.Api.Data;
using Backlogr.Api.Models.Entities;
using Backlogr.Api.Models.Enums;
using Backlogr.Api.Services.Implementations;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Backlogr.Api.Tests.Services;

public sealed class ProfileServiceTests
{
    [Fact]
    public async Task GetPublicProfileAsync_ShouldReturnCountsAndRecentReviewFlags_CaseInsensitive()
    {
        var dbContext = CreateDbContext();

        var currentUser = CreateUser("current_profile_user", "current_profile_user@example.com", "Current Profile User");
        var profileUser = CreateUser(
            "target_profile_user",
            "target_profile_user@example.com",
            "Target Profile User",
            avatarUrl: "https://example.com/target-profile.png",
            bio: "Testing profiles.");

        var firstGame = CreateGame("First Profile Game", coverImageUrl: "https://example.com/first-cover.png");
        var secondGame = CreateGame("Second Profile Game", coverImageUrl: "https://example.com/second-cover.png");

        dbContext.Users.AddRange(currentUser, profileUser);
        dbContext.Games.AddRange(firstGame, secondGame);
        dbContext.Follows.Add(new Follow
        {
            FollowId = Guid.NewGuid(),
            FollowerId = currentUser.Id,
            FollowingId = profileUser.Id,
            CreatedAt = DateTime.UtcNow,
            Follower = currentUser,
            Following = profileUser
        });

        dbContext.GameLogs.AddRange(
            new GameLog
            {
                GameLogId = Guid.NewGuid(),
                UserId = profileUser.Id,
                GameId = firstGame.GameId,
                Status = LibraryStatus.Playing,
                UpdatedAt = new DateTime(2026, 3, 13, 10, 0, 0, DateTimeKind.Utc),
                User = profileUser,
                Game = firstGame
            },
            new GameLog
            {
                GameLogId = Guid.NewGuid(),
                UserId = profileUser.Id,
                GameId = secondGame.GameId,
                Status = LibraryStatus.Played,
                UpdatedAt = new DateTime(2026, 3, 13, 11, 0, 0, DateTimeKind.Utc),
                User = profileUser,
                Game = secondGame
            });

        var olderReview = new Review
        {
            ReviewId = Guid.NewGuid(),
            UserId = profileUser.Id,
            GameId = firstGame.GameId,
            Text = "Older review",
            HasSpoilers = false,
            CreatedAt = new DateTime(2026, 3, 12, 10, 0, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2026, 3, 12, 12, 0, 0, DateTimeKind.Utc),
            User = profileUser,
            Game = firstGame
        };

        var newerReview = new Review
        {
            ReviewId = Guid.NewGuid(),
            UserId = profileUser.Id,
            GameId = secondGame.GameId,
            Text = "Newer review",
            HasSpoilers = true,
            CreatedAt = new DateTime(2026, 3, 13, 10, 0, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2026, 3, 13, 13, 0, 0, DateTimeKind.Utc),
            User = profileUser,
            Game = secondGame
        };

        dbContext.Reviews.AddRange(olderReview, newerReview);
        dbContext.ReviewLikes.Add(new ReviewLike
        {
            ReviewLikeId = Guid.NewGuid(),
            UserId = currentUser.Id,
            ReviewId = newerReview.ReviewId,
            CreatedAt = DateTime.UtcNow,
            User = currentUser,
            Review = newerReview
        });
        dbContext.ReviewComments.Add(new ReviewComment
        {
            ReviewCommentId = Guid.NewGuid(),
            UserId = currentUser.Id,
            ReviewId = newerReview.ReviewId,
            Text = "Comment on newest review",
            CreatedAt = DateTime.UtcNow,
            User = currentUser,
            Review = newerReview
        });

        await dbContext.SaveChangesAsync();

        var service = new ProfileService(dbContext);

        var result = await service.GetPublicProfileAsync(currentUser.Id, "TARGET_PROFILE_USER");

        result.UserId.Should().Be(profileUser.Id);
        result.UserName.Should().Be("target_profile_user");
        result.DisplayName.Should().Be("Target Profile User");
        result.Bio.Should().Be("Testing profiles.");
        result.AvatarUrl.Should().Be("https://example.com/target-profile.png");
        result.FollowerCount.Should().Be(1);
        result.FollowingCount.Should().Be(0);
        result.ReviewCount.Should().Be(2);
        result.LibraryCount.Should().Be(2);
        result.IsFollowing.Should().BeTrue();
        result.IsSelf.Should().BeFalse();
        result.RecentReviews.Should().HaveCount(2);
        result.RecentReviews[0].GameTitle.Should().Be("Second Profile Game");
        result.RecentReviews[0].LikeCount.Should().Be(1);
        result.RecentReviews[0].CommentCount.Should().Be(1);
        result.RecentReviews[0].LikedByCurrentUser.Should().BeTrue();
        result.RecentReviews[0].IsOwner.Should().BeFalse();
    }

    [Fact]
    public async Task GetPublicLibraryAsync_ShouldReturnOrderedLibraryEntries()
    {
        var dbContext = CreateDbContext();

        var profileUser = CreateUser("library_profile_user", "library_profile_user@example.com", "Library Profile User");
        var olderGame = CreateGame("Older Library Game");
        var newerGame = CreateGame("Newer Library Game", coverImageUrl: "https://example.com/newer-cover.png");

        dbContext.Users.Add(profileUser);
        dbContext.Games.AddRange(olderGame, newerGame);
        dbContext.GameLogs.AddRange(
            new GameLog
            {
                GameLogId = Guid.NewGuid(),
                UserId = profileUser.Id,
                GameId = olderGame.GameId,
                Status = LibraryStatus.Backlog,
                UpdatedAt = new DateTime(2026, 3, 10, 10, 0, 0, DateTimeKind.Utc),
                User = profileUser,
                Game = olderGame
            },
            new GameLog
            {
                GameLogId = Guid.NewGuid(),
                UserId = profileUser.Id,
                GameId = newerGame.GameId,
                Status = LibraryStatus.Played,
                Rating = 5m,
                Platform = "PC",
                Hours = 22m,
                UpdatedAt = new DateTime(2026, 3, 13, 10, 0, 0, DateTimeKind.Utc),
                User = profileUser,
                Game = newerGame
            });

        await dbContext.SaveChangesAsync();

        var service = new ProfileService(dbContext);

        var result = await service.GetPublicLibraryAsync("library_profile_user");

        result.Should().HaveCount(2);
        result[0].GameTitle.Should().Be("Newer Library Game");
        result[0].CoverImageUrl.Should().Be("https://example.com/newer-cover.png");
        result[0].Status.Should().Be(LibraryStatus.Played);
        result[0].Rating.Should().Be(5m);
        result[0].Platform.Should().Be("PC");
        result[0].Hours.Should().Be(22m);
        result[1].GameTitle.Should().Be("Older Library Game");
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static ApplicationUser CreateUser(
        string userName,
        string email,
        string displayName,
        string? avatarUrl = null,
        string? bio = null)
    {
        return new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = userName,
            NormalizedUserName = userName.ToUpperInvariant(),
            Email = email,
            DisplayName = displayName,
            AvatarUrl = avatarUrl,
            Bio = bio,
            CreatedAt = DateTime.UtcNow
        };
    }

    private static Game CreateGame(string title, string? coverImageUrl = null)
    {
        return new Game
        {
            GameId = Guid.NewGuid(),
            Title = title,
            CoverImageUrl = coverImageUrl,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
}
