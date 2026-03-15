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
using Backlogr.Api.Models.Enums;
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
    public async Task GetFeed_ShouldReturnFollowedUsersLogAndReviewActivity()
    {
        using var currentUserClient = _factory.CreateClient();
        using var followedUserClient = _factory.CreateClient();

        var currentUserId = Guid.Parse("f1111111-1111-1111-1111-111111111111");
        var followedUserId = Guid.Parse("f2222222-2222-2222-2222-222222222222");

        currentUserClient.DefaultRequestHeaders.Add("X-Test-UserId", currentUserId.ToString());
        followedUserClient.DefaultRequestHeaders.Add("X-Test-UserId", followedUserId.ToString());

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

        var followResponse = await currentUserClient.PostAsync($"/api/follows/{followedUserId}", null);
        followResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var logResponse = await followedUserClient.PostAsJsonAsync("/api/library", new UpsertLibraryLogRequestDto
        {
            GameId = DevelopmentDataSeeder.TestGameId,
            Status = LibraryStatus.Playing,
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

        var feedResponse = await currentUserClient.GetAsync("/api/feed?take=10");
        feedResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var items = await feedResponse.Content.ReadFromJsonAsync<List<FeedItemResponseDto>>(JsonOptions);

        items.Should().NotBeNull();
        items!.Should().HaveCount(2);
        items.Should().OnlyContain(item => item.UserId == followedUserId);
        items.Should().Contain(item => item.ItemType == FeedItemType.GameLog);
        items.Should().Contain(item => item.ItemType == FeedItemType.Review);
        items.Should().OnlyContain(item => item.GameId == DevelopmentDataSeeder.TestGameId);

        var reviewItem = items.Single(item => item.ItemType == FeedItemType.Review);
        reviewItem.AvatarUrl.Should().Be("https://example.com/feed-followed-user.png");
        reviewItem.LikeCount.Should().Be(1);
        reviewItem.CommentCount.Should().Be(1);
        reviewItem.LikedByCurrentUser.Should().BeTrue();
        reviewItem.IsOwner.Should().BeFalse();

        var logItem = items.Single(item => item.ItemType == FeedItemType.GameLog);
        logItem.AvatarUrl.Should().Be("https://example.com/feed-followed-user.png");
        logItem.LikeCount.Should().Be(0);
        logItem.CommentCount.Should().Be(0);
        logItem.LikedByCurrentUser.Should().BeFalse();
        logItem.IsOwner.Should().BeFalse();
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