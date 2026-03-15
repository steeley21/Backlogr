using System.Net;
using System.Net.Http.Json;
using Backlogr.Api.Common;
using Backlogr.Api.Data;
using Backlogr.Api.DTOs.Reviews;
using Backlogr.Api.Models.Entities;
using FluentAssertions.Extensions;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Backlogr.Api.Tests.Integration;

public sealed class ReviewInteractionsFlowTests : IClassFixture<AuthenticatedBacklogrApiFactory>
{
    private readonly AuthenticatedBacklogrApiFactory _factory;

    public ReviewInteractionsFlowTests(AuthenticatedBacklogrApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task ReviewInteractionsFlow_ShouldLikeUnlikeCommentAndDeleteOwnedComment()
    {
        using var client = _factory.CreateClient();

        var userId = Guid.Parse("88888888-8888-8888-8888-888888888888");
        client.DefaultRequestHeaders.Add("X-Test-UserId", userId.ToString());

        await SeedUserAsync(
            userId,
            userName: "interaction_user",
            email: "interaction_user@example.com",
            displayName: "Interaction User",
            avatarUrl: "https://example.com/interaction-user.png");

        var createReviewResponse = await client.PostAsJsonAsync("/api/reviews", new CreateReviewRequestDto
        {
            GameId = DevelopmentDataSeeder.TestGameId,
            Text = "Review used for interaction testing.",
            HasSpoilers = false
        });

        createReviewResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var review = await createReviewResponse.Content.ReadFromJsonAsync<ReviewResponseDto>();
        review.Should().NotBeNull();

        var likeResponse = await client.PostAsync($"/api/reviews/{review!.ReviewId}/like", null);
        likeResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var unlikeResponse = await client.DeleteAsync($"/api/reviews/{review.ReviewId}/like");
        unlikeResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var commentResponse = await client.PostAsJsonAsync(
            $"/api/reviews/{review.ReviewId}/comments",
            new CreateReviewCommentRequestDto
            {
                Text = "Great review comment."
            });

        commentResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var comment = await commentResponse.Content.ReadFromJsonAsync<ReviewCommentResponseDto>();
        comment.Should().NotBeNull();
        comment!.ReviewId.Should().Be(review.ReviewId);
        comment.UserId.Should().Be(userId);
        comment.UserName.Should().Be("interaction_user");
        comment.DisplayName.Should().Be("Interaction User");
        comment.AvatarUrl.Should().Be("https://example.com/interaction-user.png");
        comment.Text.Should().Be("Great review comment.");
        comment.IsOwner.Should().BeTrue();
        comment.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, 10.Seconds());

        var getCommentsResponse = await client.GetAsync($"/api/reviews/{review.ReviewId}/comments");
        getCommentsResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var comments = await getCommentsResponse.Content.ReadFromJsonAsync<List<ReviewCommentResponseDto>>();
        comments.Should().NotBeNull();
        comments!.Should().ContainSingle();
        comments[0].ReviewCommentId.Should().Be(comment.ReviewCommentId);
        comments[0].AvatarUrl.Should().Be("https://example.com/interaction-user.png");
        comments[0].IsOwner.Should().BeTrue();

        var deleteCommentResponse = await client.DeleteAsync($"/api/comments/{comment.ReviewCommentId}");
        deleteCommentResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteComment_ShouldReturnForbidden_WhenUserDoesNotOwnComment()
    {
        using var ownerClient = _factory.CreateClient();
        using var otherClient = _factory.CreateClient();

        var ownerUserId = Guid.Parse("99999999-9999-9999-9999-999999999999");
        var otherUserId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

        ownerClient.DefaultRequestHeaders.Add("X-Test-UserId", ownerUserId.ToString());
        otherClient.DefaultRequestHeaders.Add("X-Test-UserId", otherUserId.ToString());

        await SeedUserAsync(
            ownerUserId,
            userName: "comment_owner_user",
            email: "comment_owner_user@example.com",
            displayName: "Comment Owner User");

        await SeedUserAsync(
            otherUserId,
            userName: "comment_other_user",
            email: "comment_other_user@example.com",
            displayName: "Comment Other User");

        var createReviewResponse = await ownerClient.PostAsJsonAsync("/api/reviews", new CreateReviewRequestDto
        {
            GameId = DevelopmentDataSeeder.TestGameId,
            Text = "Owner review for comment deletion test.",
            HasSpoilers = false
        });

        createReviewResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var review = await createReviewResponse.Content.ReadFromJsonAsync<ReviewResponseDto>();
        review.Should().NotBeNull();

        var createCommentResponse = await ownerClient.PostAsJsonAsync(
            $"/api/reviews/{review!.ReviewId}/comments",
            new CreateReviewCommentRequestDto
            {
                Text = "Owner comment"
            });

        createCommentResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var comment = await createCommentResponse.Content.ReadFromJsonAsync<ReviewCommentResponseDto>();
        comment.Should().NotBeNull();

        var deleteResponse = await otherClient.DeleteAsync($"/api/comments/{comment!.ReviewCommentId}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
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