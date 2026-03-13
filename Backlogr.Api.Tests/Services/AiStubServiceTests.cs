using Backlogr.Api.Data;
using Backlogr.Api.DTOs.AI;
using Backlogr.Api.Models.Entities;
using Backlogr.Api.Models.Enums;
using Backlogr.Api.Services.Implementations;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Backlogr.Api.Tests.Services;

public sealed class AiStubServiceTests
{
    [Fact]
    public async Task GetRecommendationsAsync_ShouldRecommendCandidateGames_NotAlreadyLogged()
    {
        var dbContext = CreateDbContext();

        var user = CreateUser("rec_user", "rec_user@example.com", "Rec User");

        var likedGame = CreateGame(
            title: "Liked Game",
            genres: "Action, Roguelike",
            platforms: "PC");

        var candidateGame = CreateGame(
            title: "Candidate Game",
            genres: "Roguelike, Indie",
            platforms: "PC");

        var unrelatedGame = CreateGame(
            title: "Unrelated Game",
            genres: "Puzzle",
            platforms: "Mobile");

        dbContext.Users.Add(user);
        dbContext.Games.AddRange(likedGame, candidateGame, unrelatedGame);

        dbContext.GameLogs.Add(new GameLog
        {
            GameLogId = Guid.NewGuid(),
            UserId = user.Id,
            GameId = likedGame.GameId,
            Status = LibraryStatus.Played,
            Rating = 4.5m,
            UpdatedAt = DateTime.UtcNow,
            User = user,
            Game = likedGame
        });

        await dbContext.SaveChangesAsync();

        var service = new StubRecommendationService(dbContext);

        var result = await service.GetRecommendationsAsync(user.Id, take: 5);

        result.Items.Should().NotBeEmpty();
        result.Items.Should().NotContain(i => i.GameId == likedGame.GameId);
        result.Items.Select(i => i.Title).Should().Contain("Candidate Game");
    }

    [Fact]
    public async Task ReviewAssistant_ShouldReturnDraftText_ForDraftMode()
    {
        var service = new StubReviewAssistantService();

        var result = await service.GenerateAsync(new ReviewAssistantRequestDto
        {
            Mode = "draft",
            Prompt = "fast combat and strong music"
        });

        result.Mode.Should().Be("draft");
        result.ResultText.Should().Contain("Draft review");
    }

    [Fact]
    public async Task ReviewAssistant_ShouldThrow_WhenPromptAndExistingTextMissing()
    {
        var service = new StubReviewAssistantService();

        var act = async () => await service.GenerateAsync(new ReviewAssistantRequestDto
        {
            Mode = "draft",
            Prompt = "   ",
            ExistingText = null
        });

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Prompt or existing text is required*");
    }

    [Fact]
    public async Task SemanticSearch_ShouldReturnMatchingGames()
    {
        var dbContext = CreateDbContext();

        dbContext.Games.AddRange(
            CreateGame(
                title: "Cozy Valley",
                genres: "Simulation, Indie",
                platforms: "PC",
                summary: "A cozy story game with crafting and exploration."),
            CreateGame(
                title: "Arena Blitz",
                genres: "Action",
                platforms: "PC",
                summary: "Fast arena combat.")
        );

        await dbContext.SaveChangesAsync();

        var service = new StubSemanticSearchService(dbContext);

        var result = await service.SearchAsync("cozy crafting exploration", take: 10);

        result.Should().NotBeEmpty();
        result[0].Title.Should().Be("Cozy Valley");
        result[0].Score.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task SemanticSearch_ShouldThrow_WhenQueryMissing()
    {
        var dbContext = CreateDbContext();
        var service = new StubSemanticSearchService(dbContext);

        var act = async () => await service.SearchAsync("   ", take: 10);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Query is required*");
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static ApplicationUser CreateUser(string userName, string email, string displayName)
    {
        return new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = userName,
            Email = email,
            DisplayName = displayName,
            CreatedAt = DateTime.UtcNow
        };
    }

    private static Game CreateGame(
        string title,
        string? genres,
        string? platforms,
        string? summary = null)
    {
        return new Game
        {
            GameId = Guid.NewGuid(),
            Title = title,
            Genres = genres,
            Platforms = platforms,
            Summary = summary,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
}