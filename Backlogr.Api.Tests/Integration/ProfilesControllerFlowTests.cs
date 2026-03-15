using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Backlogr.Api.Common;
using Backlogr.Api.Data;
using Backlogr.Api.DTOs.Profiles;
using Backlogr.Api.Models.Entities;
using Backlogr.Api.Models.Enums;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Backlogr.Api.Tests.Integration;

public sealed class ProfilesControllerFlowTests : IClassFixture<AuthenticatedBacklogrApiFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly AuthenticatedBacklogrApiFactory _factory;

    public ProfilesControllerFlowTests(AuthenticatedBacklogrApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetPublicProfile_ShouldReturnProfileSummaryAndRecentReviews()
    {
        using var currentUserClient = _factory.CreateClient();

        var currentUserId = Guid.Parse("10101010-1010-1010-1010-101010101010");
        var profileUserId = Guid.Parse("20202020-2020-2020-2020-202020202020");
        var secondGameId = Guid.Parse("30303030-3030-3030-3030-303030303030");

        currentUserClient.DefaultRequestHeaders.Add("X-Test-UserId", currentUserId.ToString());

        await SeedUserAsync(
            currentUserId,
            "profile_current_user",
            "profile_current_user@example.com",
            "Profile Current User",
            avatarUrl: "https://example.com/current-user.png");

        await SeedUserAsync(
            profileUserId,
            "profile_target_user",
            "profile_target_user@example.com",
            "Profile Target User",
            avatarUrl: "https://example.com/target-user.png",
            bio: "Retro RPG fan.");

        await SeedGameAsync(secondGameId, "Second Profile Game", "https://example.com/second-cover.png");
        await SeedFollowAsync(currentUserId, profileUserId);
        await SeedProfileReviewActivityAsync(currentUserId, profileUserId, secondGameId);

        var response = await currentUserClient.GetAsync("/api/profiles/PROFILE_TARGET_USER");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var profile = await response.Content.ReadFromJsonAsync<PublicProfileResponseDto>(JsonOptions);
        profile.Should().NotBeNull();
        profile!.UserId.Should().Be(profileUserId);
        profile.UserName.Should().Be("profile_target_user");
        profile.DisplayName.Should().Be("Profile Target User");
        profile.Bio.Should().Be("Retro RPG fan.");
        profile.AvatarUrl.Should().Be("https://example.com/target-user.png");
        profile.FollowerCount.Should().Be(1);
        profile.FollowingCount.Should().Be(0);
        profile.ReviewCount.Should().Be(2);
        profile.LibraryCount.Should().Be(2);
        profile.IsFollowing.Should().BeTrue();
        profile.IsSelf.Should().BeFalse();
        profile.RecentReviews.Should().HaveCount(2);

        var newestReview = profile.RecentReviews[0];
        newestReview.GameTitle.Should().Be("Second Profile Game");
        newestReview.CoverImageUrl.Should().Be("https://example.com/second-cover.png");
        newestReview.LikeCount.Should().Be(1);
        newestReview.CommentCount.Should().Be(1);
        newestReview.LikedByCurrentUser.Should().BeTrue();
        newestReview.IsOwner.Should().BeFalse();
    }

    [Fact]
    public async Task GetPublicProfile_ShouldMarkIsSelf_WhenLookingUpOwnProfile()
    {
        using var client = _factory.CreateClient();

        var userId = Guid.Parse("40404040-4040-4040-4040-404040404040");
        client.DefaultRequestHeaders.Add("X-Test-UserId", userId.ToString());

        await SeedUserAsync(
            userId,
            "self_profile_user",
            "self_profile_user@example.com",
            "Self Profile User",
            avatarUrl: "https://example.com/self-user.png");

        var response = await client.GetAsync("/api/profiles/self_profile_user");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var profile = await response.Content.ReadFromJsonAsync<PublicProfileResponseDto>(JsonOptions);
        profile.Should().NotBeNull();
        profile!.IsSelf.Should().BeTrue();
        profile.IsFollowing.Should().BeFalse();
    }

    [Fact]
    public async Task GetPublicLibrary_ShouldReturnOrderedPublicLibraryEntries()
    {
        using var currentUserClient = _factory.CreateClient();

        var currentUserId = Guid.Parse("50505050-5050-5050-5050-505050505050");
        var profileUserId = Guid.Parse("60606060-6060-6060-6060-606060606060");
        var secondGameId = Guid.Parse("70707070-7070-7070-7070-707070707070");

        currentUserClient.DefaultRequestHeaders.Add("X-Test-UserId", currentUserId.ToString());

        await SeedUserAsync(
            currentUserId,
            "library_current_user",
            "library_current_user@example.com",
            "Library Current User");

        await SeedUserAsync(
            profileUserId,
            "library_target_user",
            "library_target_user@example.com",
            "Library Target User");

        await SeedGameAsync(secondGameId, "Second Library Game", "https://example.com/library-cover.png");
        await SeedProfileLibraryActivityAsync(profileUserId, secondGameId);

        var response = await currentUserClient.GetAsync("/api/profiles/library_target_user/library");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var libraryItems = await response.Content.ReadFromJsonAsync<List<PublicProfileLibraryItemResponseDto>>(JsonOptions);
        libraryItems.Should().NotBeNull();
        libraryItems!.Should().HaveCount(2);
        libraryItems[0].GameTitle.Should().Be("Second Library Game");
        libraryItems[0].CoverImageUrl.Should().Be("https://example.com/library-cover.png");
        libraryItems[0].Status.Should().Be(LibraryStatus.Played);
        libraryItems[0].Rating.Should().Be(4.5m);
        libraryItems[0].Platform.Should().Be("PC");
        libraryItems[0].Hours.Should().Be(18m);
        libraryItems[1].GameId.Should().Be(DevelopmentDataSeeder.TestGameId);
    }

    private async Task SeedUserAsync(
        Guid userId,
        string userName,
        string email,
        string displayName,
        string? avatarUrl = null,
        string? bio = null)
    {
        using var scope = _factory.Services.CreateScope();

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

        if (!await roleManager.RoleExistsAsync(RoleNames.User))
        {
            var createRoleResult = await roleManager.CreateAsync(new ApplicationRole
            {
                Name = RoleNames.User
            });

            createRoleResult.Succeeded.Should().BeTrue();
        }

        var existingUser = await userManager.FindByIdAsync(userId.ToString());
        if (existingUser is not null)
        {
            return;
        }

        var user = new ApplicationUser
        {
            Id = userId,
            UserName = userName,
            Email = email,
            DisplayName = displayName,
            AvatarUrl = avatarUrl,
            Bio = bio,
            CreatedAt = DateTime.UtcNow
        };

        var createUserResult = await userManager.CreateAsync(user, "Password1");
        createUserResult.Succeeded.Should().BeTrue();

        var addToRoleResult = await userManager.AddToRoleAsync(user, RoleNames.User);
        addToRoleResult.Succeeded.Should().BeTrue();
    }

    private async Task SeedGameAsync(Guid gameId, string title, string? coverImageUrl = null)
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var existingGame = await dbContext.Games.FindAsync(gameId);
        if (existingGame is not null)
        {
            return;
        }

        dbContext.Games.Add(new Game
        {
            GameId = gameId,
            Title = title,
            CoverImageUrl = coverImageUrl,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync();
    }

    private async Task SeedFollowAsync(Guid followerId, Guid followingId)
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var exists = await dbContext.Follows.AnyAsync(f => f.FollowerId == followerId && f.FollowingId == followingId);
        if (exists)
        {
            return;
        }

        dbContext.Follows.Add(new Follow
        {
            FollowId = Guid.NewGuid(),
            FollowerId = followerId,
            FollowingId = followingId,
            CreatedAt = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync();
    }

    private async Task SeedProfileReviewActivityAsync(Guid currentUserId, Guid profileUserId, Guid secondGameId)
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        dbContext.GameLogs.AddRange(
            new GameLog
            {
                GameLogId = Guid.NewGuid(),
                UserId = profileUserId,
                GameId = DevelopmentDataSeeder.TestGameId,
                Status = LibraryStatus.Playing,
                Rating = 4.0m,
                Platform = "Switch",
                Hours = 8m,
                UpdatedAt = new DateTime(2026, 3, 13, 9, 0, 0, DateTimeKind.Utc)
            },
            new GameLog
            {
                GameLogId = Guid.NewGuid(),
                UserId = profileUserId,
                GameId = secondGameId,
                Status = LibraryStatus.Played,
                Rating = 4.5m,
                Platform = "PC",
                Hours = 18m,
                StartedAt = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc),
                FinishedAt = new DateTime(2026, 3, 12, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2026, 3, 13, 15, 0, 0, DateTimeKind.Utc)
            });

        var olderReviewId = Guid.NewGuid();
        var newerReviewId = Guid.NewGuid();

        dbContext.Reviews.AddRange(
            new Review
            {
                ReviewId = olderReviewId,
                UserId = profileUserId,
                GameId = DevelopmentDataSeeder.TestGameId,
                Text = "Older review text.",
                HasSpoilers = false,
                CreatedAt = new DateTime(2026, 3, 10, 9, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2026, 3, 11, 9, 0, 0, DateTimeKind.Utc)
            },
            new Review
            {
                ReviewId = newerReviewId,
                UserId = profileUserId,
                GameId = secondGameId,
                Text = "Newest review text.",
                HasSpoilers = true,
                CreatedAt = new DateTime(2026, 3, 12, 9, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2026, 3, 13, 16, 0, 0, DateTimeKind.Utc)
            });

        dbContext.ReviewLikes.Add(new ReviewLike
        {
            ReviewLikeId = Guid.NewGuid(),
            UserId = currentUserId,
            ReviewId = newerReviewId,
            CreatedAt = DateTime.UtcNow
        });

        dbContext.ReviewComments.Add(new ReviewComment
        {
            ReviewCommentId = Guid.NewGuid(),
            UserId = currentUserId,
            ReviewId = newerReviewId,
            Text = "Love this take.",
            CreatedAt = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync();
    }

    private async Task SeedProfileLibraryActivityAsync(Guid profileUserId, Guid secondGameId)
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        dbContext.GameLogs.AddRange(
            new GameLog
            {
                GameLogId = Guid.NewGuid(),
                UserId = profileUserId,
                GameId = DevelopmentDataSeeder.TestGameId,
                Status = LibraryStatus.Backlog,
                Rating = null,
                Platform = null,
                Hours = null,
                UpdatedAt = new DateTime(2026, 3, 10, 9, 0, 0, DateTimeKind.Utc)
            },
            new GameLog
            {
                GameLogId = Guid.NewGuid(),
                UserId = profileUserId,
                GameId = secondGameId,
                Status = LibraryStatus.Played,
                Rating = 4.5m,
                Platform = "PC",
                Hours = 18m,
                StartedAt = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc),
                FinishedAt = new DateTime(2026, 3, 12, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2026, 3, 13, 15, 0, 0, DateTimeKind.Utc)
            });

        await dbContext.SaveChangesAsync();
    }
}
