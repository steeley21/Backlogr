using System.Net;
using System.Net.Http.Json;
using Backlogr.Api.Common;
using Backlogr.Api.DTOs.Igdb;
using Backlogr.Api.Models.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Backlogr.Api.Tests.Integration;

public sealed class IgdbControllerFlowTests : IClassFixture<AuthenticatedBacklogrApiFactory>
{
    private readonly AuthenticatedBacklogrApiFactory _factory;

    public IgdbControllerFlowTests(AuthenticatedBacklogrApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task SearchGames_ShouldReturnResults_ForAuthenticatedUser()
    {
        using var client = _factory.CreateClient();

        var userId = Guid.Parse("12121212-1212-1212-1212-121212121212");
        client.DefaultRequestHeaders.Add("X-Test-UserId", userId.ToString());
        client.DefaultRequestHeaders.Add("X-Test-Roles", "User");

        await SeedUserAsync(
            userId,
            "igdb_search_user",
            "igdb_search_user@example.com",
            "IGDB Search User");

        var response = await client.GetAsync("/api/igdb/search?query=hades");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var results = await response.Content.ReadFromJsonAsync<List<IgdbGameSearchResultDto>>();
        results.Should().NotBeNull();
        results!.Should().ContainSingle(r => r.IgdbId == 1003);
    }

    [Fact]
    public async Task ImportGame_ShouldReturnForbidden_ForNonAdminUser()
    {
        using var client = _factory.CreateClient();

        var userId = Guid.Parse("13131313-1313-1313-1313-131313131313");
        client.DefaultRequestHeaders.Add("X-Test-UserId", userId.ToString());
        client.DefaultRequestHeaders.Add("X-Test-Roles", "User");

        await SeedUserAsync(
            userId,
            "igdb_non_admin",
            "igdb_non_admin@example.com",
            "IGDB Non Admin");

        var response = await client.PostAsync("/api/igdb/import/1001", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ImportGame_ShouldImportOrUpdateGame_ForAdminUser()
    {
        using var client = _factory.CreateClient();

        var adminUserId = Guid.Parse("14141414-1414-1414-1414-141414141414");
        client.DefaultRequestHeaders.Add("X-Test-UserId", adminUserId.ToString());
        client.DefaultRequestHeaders.Add("X-Test-Roles", "Admin");

        await SeedUserAsync(
            adminUserId,
            "igdb_admin_user",
            "igdb_admin_user@example.com",
            "IGDB Admin User");

        var response = await client.PostAsync("/api/igdb/import/1002", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var imported = await response.Content.ReadFromJsonAsync<ImportedGameResponseDto>();
        imported.Should().NotBeNull();
        imported!.IgdbId.Should().Be(1002);
        imported.Title.Should().Be("Stardew Valley");
    }

    private async Task SeedUserAsync(Guid userId, string userName, string email, string displayName)
    {
        using var scope = _factory.Services.CreateScope();

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

        foreach (var roleName in new[] { RoleNames.User, RoleNames.Admin })
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var createRoleResult = await roleManager.CreateAsync(new ApplicationRole
                {
                    Name = roleName
                });

                createRoleResult.Succeeded.Should().BeTrue();
            }
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
    }
}