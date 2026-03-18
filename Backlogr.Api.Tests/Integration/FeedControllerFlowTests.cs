using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Backlogr.Api.Common;
using Backlogr.Api.Data;
using Backlogr.Api.DTOs.Feed;
using Backlogr.Api.DTOs.Library;
using Backlogr.Api.DTOs.Reviews;
using Backlogr.Api.Models.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Backlogr.Api.Tests.Integration;

public sealed class FeedControllerFlowTests : IClassFixture<AuthenticatedBacklogrApiFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly AuthenticatedBacklogrApiFactory _factory;

    public FeedControllerFlowTests(AuthenticatedBacklogrApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetFeed_ShouldReturnAllRecentActivity_ForYouScope()
    {
        using var currentUserClient = _factory.CreateClient();
        using var followedUserClient = _factory.CreateClient();
        using var otherUserClient = _factory.CreateClient();

        var currentUserId = Guid.Parse("f1111111-1111-1111-1111-111111111111");
        var followedUserId = Guid.Parse("f2222222-2222-2222-2222-222222222222");
        var otherUserId = Guid.Parse("f3333333-3333-3333-3333-333333333333");

        currentUserClient.DefaultRequestHeaders.Add("X-Test-UserId", currentUserId.ToString());
        followedUserClient.DefaultRequestHeaders.Add("X-Test-UserId", followedUserId.ToString());
        otherUserClient.DefaultRequestHeaders.Add("X-Test-UserId", otherUserId.ToString());

        await SeedUserAsync(
            currentUserId,
            "feed_current_user",
            "feed_current_user@example.com",
            "Feed Current User");

        await SeedUserAsync(
            followedUserId,
            "feed_followed_user",
            "feed_followed_user@example.com",
            "Feed Followed User",
            avatarUrl: "https://example.com/feed-followed-user.png");

        await SeedUserAsync(
            otherUserId,
            "feed_other_user",
            "feed_other_user@example.com",
            "Feed Other User");

        var currentReviewResponse = await currentUserClient.PostAsJsonAsync("/api/reviews", new CreateReviewRequestDto
        {
            GameId = DevelopmentDataSeeder.TestGameId,
            Text = "Current user review",
            HasSpoilers = false
        });
        currentReviewResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var followedLogResponse = await followedUserClient.PostAsJsonAsync("/api/library", new UpsertLibraryLogRequestDto
        {
            GameId = DevelopmentDataSeeder.TestGameId,
            Status = Models.Enums.LibraryStatus.Playing,
            Rating = 4.5m,
            Platform = "PC",
            Hours = 12.0m,
            Notes = "Followed user log entry"
        });
        followedLogResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var otherReviewResponse = await otherUserClient.PostAsJsonAsync("/api/reviews", new CreateReviewRequestDto
        {
            GameId = DevelopmentDataSeeder.TestGameId,
            Text = "Other user review",
            HasSpoilers = false
        });
        otherReviewResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var feedResponse = await currentUserClient.GetAsync("/api/feed?take=10&scope=for-you");
        feedResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var items = await feedResponse.Content.ReadFromJsonAsync<List<FeedItemResponseDto>>(JsonOptions);

        items.Should().NotBeNull();
        items!.Should().HaveCount(3);
        items.Select(item => item.UserId).Should().Contain(new[]
        {
            currentUserId,
            followedUserId,
            otherUserId
        });

        items.Should().Contain(item => item.ItemType == FeedItemType.Review && item.UserId == currentUserId);
        items.Should().Contain(item => item.ItemType == FeedItemType.GameLog && item.UserId == followedUserId);
        items.Should().Contain(item => item.ItemType == FeedItemType.Review && item.UserId == otherUserId);
    }

    [Fact]
    public async Task GetFeed_ShouldReturnFollowedUsersAndCurrentUserActivity_ForFollowingScope()
    {
        using var currentUserClient = _factory.CreateClient();
        using var followedUserClient = _factory.CreateClient();
        using var otherUserClient = _factory.CreateClient();

        var currentUserId = Guid.Parse("e1111111-1111-1111-1111-111111111111");
        var followedUserId = Guid.Parse("e2222222-2222-2222-2222-222222222222");
        var otherUserId = Guid.Parse("e3333333-3333-3333-3333-333333333333");

        currentUserClient.DefaultRequestHeaders.Add("X-Test-UserId", currentUserId.ToString());
        followedUserClient.DefaultRequestHeaders.Add("X-Test-UserId", followedUserId.ToString());
        otherUserClient.DefaultRequestHeaders.Add("X-Test-UserId", otherUserId.ToString());

        await SeedUserAsync(
            currentUserId,
            "following_current_user",
            "following_current_user@example.com",
            "Following Current User");

        await SeedUserAsync(
            followedUserId,
            "following_followed_user",
            "following_followed_user@example.com",
            "Following Followed User",
            avatarUrl: "https://example.com/feed-followed-user.png");

        await SeedUserAsync(
            otherUserId,
            "following_other_user",
            "following_other_user@example.com",
            "Following Other User");

        var followResponse = await currentUserClient.PostAsync($"/api/follows/{followedUserId}", null);
        followResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var selfReviewResponse = await currentUserClient.PostAsJsonAsync("/api/reviews", new CreateReviewRequestDto
        {
            GameId = DevelopmentDataSeeder.TestGameId,
            Text = "My own feed review",
            HasSpoilers = false
        });
        selfReviewResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var logResponse = await followedUserClient.PostAsJsonAsync("/api/library", new UpsertLibraryLogRequestDto
        {
            GameId = DevelopmentDataSeeder.TestGameId,
            Status = Models.Enums.LibraryStatus.Playing,
            Rating = 4.5m,
            Platform = "PC",
            Hours = 12.0m,
            Notes = "Feed log entry"
        });
        logResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var reviewResponse = await followedUserClient.PostAsJsonAsync("/api/reviews", new CreateReviewRequestDto
        {
            GameId = DevelopmentDataSeeder.TestGameId,
            Text = "Feed review entry",
            HasSpoilers = false
        });
        reviewResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var createdReview = await reviewResponse.Content.ReadFromJsonAsync<ReviewResponseDto>(JsonOptions);
        createdReview.Should().NotBeNull();

        var likeResponse = await currentUserClient.PostAsync($"/api/reviews/{createdReview!.ReviewId}/like", null);
        likeResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var commentResponse = await currentUserClient.PostAsJsonAsync(
            $"/api/reviews/{createdReview.ReviewId}/comments",
            new CreateReviewCommentRequestDto
            {
                Text = "Feed comment entry"
            });

        commentResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var otherLogResponse = await otherUserClient.PostAsJsonAsync("/api/library", new UpsertLibraryLogRequestDto
        {
            GameId = DevelopmentDataSeeder.TestGameId,
            Status = Models.Enums.LibraryStatus.Backlog,
            Rating = null,
            Platform = "Switch",
            Hours = null,
            Notes = "This should not appear"
        });
        otherLogResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var feedResponse = await currentUserClient.GetAsync("/api/feed?take=10&scope=following");
        feedResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var items = await feedResponse.Content.ReadFromJsonAsync<List<FeedItemResponseDto>>(JsonOptions);

        items.Should().NotBeNull();
        items!.Should().HaveCount(3);
        items.Should().OnlyContain(item => item.UserId == currentUserId || item.UserId == followedUserId);
        items.Should().NotContain(item => item.UserId == otherUserId);

        var selfReviewItem = items.Single(item => item.UserId == currentUserId);
        selfReviewItem.IsOwner.Should().BeTrue();

        var reviewItem = items.Single(item => item.ItemType == FeedItemType.Review && item.UserId == followedUserId);
        reviewItem.AvatarUrl.Should().Be("https://example.com/feed-followed-user.png");
        reviewItem.LikeCount.Should().Be(1);
        reviewItem.CommentCount.Should().Be(1);
        reviewItem.LikedByCurrentUser.Should().BeTrue();
        reviewItem.IsOwner.Should().BeFalse();

        var logItem = items.Single(item => item.ItemType == FeedItemType.GameLog && item.UserId == followedUserId);
        logItem.AvatarUrl.Should().Be("https://example.com/feed-followed-user.png");
        logItem.LikeCount.Should().Be(0);
        logItem.CommentCount.Should().Be(0);
        logItem.LikedByCurrentUser.Should().BeFalse();
        logItem.IsOwner.Should().BeFalse();
    }

    [Fact]
    public async Task GetFeed_ShouldReturnBadRequest_ForInvalidScope()
    {
        using var client = _factory.CreateClient();

        var currentUserId = Guid.Parse("d1111111-1111-1111-1111-111111111111");
        client.DefaultRequestHeaders.Add("X-Test-UserId", currentUserId.ToString());

        await SeedUserAsync(
            currentUserId,
            "invalid_scope_user",
            "invalid_scope_user@example.com",
            "Invalid Scope User");

        var response = await client.GetAsync("/api/feed?scope=not-a-real-scope");
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private async Task SeedUserAsync(Guid userId, string userName, string email, string displayName, string? avatarUrl = null)
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
            CreatedAt = DateTime.UtcNow
        };

        var createUserResult = await userManager.CreateAsync(user, "Password1");
        createUserResult.Succeeded.Should().BeTrue();

        var addToRoleResult = await userManager.AddToRoleAsync(user, RoleNames.User);
        addToRoleResult.Succeeded.Should().BeTrue();
    }
}