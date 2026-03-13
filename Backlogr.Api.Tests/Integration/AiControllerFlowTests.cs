using System.Net;
using System.Net.Http.Json;
using Backlogr.Api.Common;
using Backlogr.Api.Data;
using Backlogr.Api.DTOs.AI;
using Backlogr.Api.Models.Entities;
using Backlogr.Api.Models.Enums;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace Backlogr.Api.Tests.Integration;

public sealed class AiControllerFlowTests : IClassFixture<AuthenticatedBacklogrApiFactory>
{
    private readonly AuthenticatedBacklogrApiFactory _factory;

    public AiControllerFlowTests(AuthenticatedBacklogrApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task AiFlow_ShouldReturnRecommendationsAssistantOutputAndSemanticSearchResults()
    {
        using var client = _factory.CreateClient();

        var userId = Guid.Parse("15151515-1515-1515-1515-151515151515");
        client.DefaultRequestHeaders.Add("X-Test-UserId", userId.ToString());
        client.DefaultRequestHeaders.Add("X-Test-Roles", "User");

        await SeedUserAsync(
            userId,
            "ai_flow_user",
            "ai_flow_user@example.com",
            "AI Flow User");

        await SeedAdditionalGamesAsync();

        var logResponse = await client.PostAsJsonAsync("/api/library", new
        {
            gameId = DevelopmentDataSeeder.TestGameId,
            status = "Played",
            rating = 4.5,
            platform = "PC",
            hours = 8.0,
            startedAt = new DateTime(2026, 3, 10, 0, 0, 0, DateTimeKind.Utc),
            finishedAt = new DateTime(2026, 3, 12, 0, 0, 0, DateTimeKind.Utc),
            notes = "Loved this one"
        });

        logResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var recommendationResponse = await client.PostAsJsonAsync("/api/ai/recommendations", new
        {
            take = 5
        });

        recommendationResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var recommendations =
            await recommendationResponse.Content.ReadFromJsonAsync<RecommendationResponseDto>();

        recommendations.Should().NotBeNull();
        recommendations!.Items.Should().NotBeEmpty();

        var assistantResponse = await client.PostAsJsonAsync("/api/ai/review-assistant", new
        {
            mode = "draft",
            prompt = "great atmosphere and satisfying combat"
        });

        assistantResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var assistant =
            await assistantResponse.Content.ReadFromJsonAsync<ReviewAssistantResponseDto>();

        assistant.Should().NotBeNull();
        assistant!.ResultText.Should().Contain("Draft review");

        var semanticResponse =
            await client.GetAsync("/api/ai/semantic-search?query=cozy+crafting+exploration");

        semanticResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var semanticResults =
            await semanticResponse.Content.ReadFromJsonAsync<List<SemanticSearchResultDto>>();

        semanticResults.Should().NotBeNull();
        semanticResults!.Should().NotBeEmpty();
        semanticResults.Select(r => r.Title).Should().Contain("Cozy Fields");
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

        var addToRoleResult = await userManager.AddToRoleAsync(user, RoleNames.User);
        addToRoleResult.Succeeded.Should().BeTrue();
    }

    private async Task SeedAdditionalGamesAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        if (!await dbContext.Games.AnyAsync(g => g.Title == "Rogue Echo"))
        {
            dbContext.Games.Add(new Game
            {
                GameId = Guid.Parse("16161616-1616-1616-1616-161616161616"),
                Title = "Rogue Echo",
                Summary = "A fast roguelike action game with quick runs.",
                Genres = "Action, Roguelike",
                Platforms = "PC",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        if (!await dbContext.Games.AnyAsync(g => g.Title == "Cozy Fields"))
        {
            dbContext.Games.Add(new Game
            {
                GameId = Guid.Parse("17171717-1717-1717-1717-171717171717"),
                Title = "Cozy Fields",
                Summary = "A cozy story game with crafting and exploration.",
                Genres = "Simulation, Indie",
                Platforms = "PC, Switch",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        await dbContext.SaveChangesAsync();
    }
}