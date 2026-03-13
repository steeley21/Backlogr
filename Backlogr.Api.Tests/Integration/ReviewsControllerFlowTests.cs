using System.Net;
using System.Net.Http.Json;
using Backlogr.Api.Common;
using Backlogr.Api.Data;
using Backlogr.Api.DTOs.Reviews;
using Backlogr.Api.Models.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Backlogr.Api.Tests.Integration;

public sealed class ReviewsControllerFlowTests : IClassFixture<AuthenticatedBacklogrApiFactory>
{
    private readonly AuthenticatedBacklogrApiFactory _factory;

    public ReviewsControllerFlowTests(AuthenticatedBacklogrApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task ReviewFlow_ShouldCreateUpdateAndDeleteOwnedReview()
    {
        using var client = _factory.CreateClient();

        var userId = Guid.Parse("44444444-4444-4444-4444-444444444444");
        client.DefaultRequestHeaders.Add("X-Test-UserId", userId.ToString());

        await SeedUserAsync(
            userId,
            userName: "review_flow_user",
            email: "review_flow_user@example.com",
            displayName: "Review Flow User");

        var createRequest = new CreateReviewRequestDto
        {
            GameId = DevelopmentDataSeeder.TestGameId,
            Text = "Great game with a strong opening and fun exploration.",
            HasSpoilers = false
        };

        var createResponse = await client.PostAsJsonAsync("/api/reviews", createRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var created = await createResponse.Content.ReadFromJsonAsync<ReviewResponseDto>();
        created.Should().NotBeNull();
        created!.UserId.Should().Be(userId);
        created.GameId.Should().Be(DevelopmentDataSeeder.TestGameId);
        created.GameTitle.Should().Be("Test Game");
        created.Text.Should().Be("Great game with a strong opening and fun exploration.");
        created.HasSpoilers.Should().BeFalse();

        var updateRequest = new UpdateReviewRequestDto
        {
            Text = "Updated review with spoiler discussion.",
            HasSpoilers = true
        };

        var updateResponse = await client.PutAsJsonAsync(
            $"/api/reviews/{created.ReviewId}",
            updateRequest);

        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var updated = await updateResponse.Content.ReadFromJsonAsync<ReviewResponseDto>();
        updated.Should().NotBeNull();
        updated!.ReviewId.Should().Be(created.ReviewId);
        updated.Text.Should().Be("Updated review with spoiler discussion.");
        updated.HasSpoilers.Should().BeTrue();

        var deleteResponse = await client.DeleteAsync($"/api/reviews/{created.ReviewId}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task CreateReview_ShouldReturnConflict_WhenDuplicateReviewIsCreated()
    {
        using var client = _factory.CreateClient();

        var userId = Guid.Parse("55555555-5555-5555-5555-555555555555");
        client.DefaultRequestHeaders.Add("X-Test-UserId", userId.ToString());

        await SeedUserAsync(
            userId,
            userName: "review_duplicate_user",
            email: "review_duplicate_user@example.com",
            displayName: "Review Duplicate User");

        var request = new CreateReviewRequestDto
        {
            GameId = DevelopmentDataSeeder.TestGameId,
            Text = "First review text",
            HasSpoilers = false
        };

        var firstResponse = await client.PostAsJsonAsync("/api/reviews", request);
        firstResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var secondResponse = await client.PostAsJsonAsync("/api/reviews", request);
        secondResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task UpdateReview_ShouldReturnForbid_WhenUserDoesNotOwnReview()
    {
        using var ownerClient = _factory.CreateClient();
        using var otherClient = _factory.CreateClient();

        var ownerUserId = Guid.Parse("66666666-6666-6666-6666-666666666666");
        var otherUserId = Guid.Parse("77777777-7777-7777-7777-777777777777");

        ownerClient.DefaultRequestHeaders.Add("X-Test-UserId", ownerUserId.ToString());
        otherClient.DefaultRequestHeaders.Add("X-Test-UserId", otherUserId.ToString());

        await SeedUserAsync(
            ownerUserId,
            userName: "review_owner_user",
            email: "review_owner_user@example.com",
            displayName: "Review Owner User");

        await SeedUserAsync(
            otherUserId,
            userName: "review_other_user",
            email: "review_other_user@example.com",
            displayName: "Review Other User");

        var createResponse = await ownerClient.PostAsJsonAsync("/api/reviews", new CreateReviewRequestDto
        {
            GameId = DevelopmentDataSeeder.TestGameId,
            Text = "Owner review text",
            HasSpoilers = false
        });

        createResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var created = await createResponse.Content.ReadFromJsonAsync<ReviewResponseDto>();
        created.Should().NotBeNull();

        var updateResponse = await otherClient.PutAsJsonAsync(
            $"/api/reviews/{created!.ReviewId}",
            new UpdateReviewRequestDto
            {
                Text = "I should not be able to edit this.",
                HasSpoilers = true
            });

        updateResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    private async Task SeedUserAsync(Guid userId, string userName, string email, string displayName)
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
            CreatedAt = DateTime.UtcNow
        };

        var createUserResult = await userManager.CreateAsync(user, "Password1");
        createUserResult.Succeeded.Should().BeTrue();

        var addToRoleResult = await userManager.AddToRoleAsync(user, RoleNames.User);
        addToRoleResult.Succeeded.Should().BeTrue();
    }
}