using System.Net;
using Backlogr.Api.Common;
using Backlogr.Api.Models.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Backlogr.Api.Tests.Integration;

public sealed class FollowsControllerFlowTests : IClassFixture<AuthenticatedBacklogrApiFactory>
{
    private readonly AuthenticatedBacklogrApiFactory _factory;

    public FollowsControllerFlowTests(AuthenticatedBacklogrApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task FollowFlow_ShouldFollowAndUnfollowUser()
    {
        using var client = _factory.CreateClient();

        var currentUserId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
        var targetUserId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");

        client.DefaultRequestHeaders.Add("X-Test-UserId", currentUserId.ToString());

        await SeedUserAsync(
            currentUserId,
            "follow_flow_user",
            "follow_flow_user@example.com",
            "Follow Flow User");

        await SeedUserAsync(
            targetUserId,
            "follow_target_user",
            "follow_target_user@example.com",
            "Follow Target User");

        var followResponse = await client.PostAsync($"/api/follows/{targetUserId}", null);
        followResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var duplicateFollowResponse = await client.PostAsync($"/api/follows/{targetUserId}", null);
        duplicateFollowResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var unfollowResponse = await client.DeleteAsync($"/api/follows/{targetUserId}");
        unfollowResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var secondUnfollowResponse = await client.DeleteAsync($"/api/follows/{targetUserId}");
        secondUnfollowResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task FollowUser_ShouldReturnBadRequest_WhenTryingToFollowSelf()
    {
        using var client = _factory.CreateClient();

        var currentUserId = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd");
        client.DefaultRequestHeaders.Add("X-Test-UserId", currentUserId.ToString());

        await SeedUserAsync(
            currentUserId,
            "self_follow_user",
            "self_follow_user@example.com",
            "Self Follow User");

        var response = await client.PostAsync($"/api/follows/{currentUserId}", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task FollowUser_ShouldReturnNotFound_WhenTargetUserMissing()
    {
        using var client = _factory.CreateClient();

        var currentUserId = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee");
        client.DefaultRequestHeaders.Add("X-Test-UserId", currentUserId.ToString());

        await SeedUserAsync(
            currentUserId,
            "missing_target_follower",
            "missing_target_follower@example.com",
            "Missing Target Follower");

        var response = await client.PostAsync($"/api/follows/{Guid.NewGuid()}", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
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