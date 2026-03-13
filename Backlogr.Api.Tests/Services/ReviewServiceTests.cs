using Backlogr.Api.Data;
using Backlogr.Api.DTOs.Reviews;
using Backlogr.Api.Models.Entities;
using Backlogr.Api.Services.Implementations;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Backlogr.Api.Tests.Services;

public sealed class ReviewServiceTests
{
    [Fact]
    public async Task CreateReviewAsync_ShouldCreateReview_WhenValid()
    {
        var dbContext = CreateDbContext();

        var user = CreateUser("review_user", "review_user@example.com", "Review User");
        var game = CreateGame("Test Game");

        dbContext.Users.Add(user);
        dbContext.Games.Add(game);
        await dbContext.SaveChangesAsync();

        var service = new ReviewService(dbContext);

        var dto = new CreateReviewRequestDto
        {
            GameId = game.GameId,
            Text = "Great game with strong atmosphere.",
            HasSpoilers = false
        };

        var result = await service.CreateReviewAsync(user.Id, dto);

        result.ReviewId.Should().NotBe(Guid.Empty);
        result.UserId.Should().Be(user.Id);
        result.GameId.Should().Be(game.GameId);
        result.GameTitle.Should().Be("Test Game");
        result.UserName.Should().Be("review_user");
        result.DisplayName.Should().Be("Review User");
        result.Text.Should().Be("Great game with strong atmosphere.");
        result.HasSpoilers.Should().BeFalse();

        dbContext.Reviews.Should().HaveCount(1);
    }

    [Fact]
    public async Task CreateReviewAsync_ShouldThrow_WhenDuplicateReviewExists()
    {
        var dbContext = CreateDbContext();

        var user = CreateUser("duplicate_user", "duplicate_user@example.com", "Duplicate User");
        var game = CreateGame("Duplicate Test");

        dbContext.Users.Add(user);
        dbContext.Games.Add(game);
        dbContext.Reviews.Add(new Review
        {
            ReviewId = Guid.NewGuid(),
            UserId = user.Id,
            GameId = game.GameId,
            Text = "Original review",
            HasSpoilers = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            User = user,
            Game = game
        });

        await dbContext.SaveChangesAsync();

        var service = new ReviewService(dbContext);

        var dto = new CreateReviewRequestDto
        {
            GameId = game.GameId,
            Text = "Trying to create a second review",
            HasSpoilers = true
        };

        var act = async () => await service.CreateReviewAsync(user.Id, dto);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already have a review*");
    }

    [Fact]
    public async Task UpdateReviewAsync_ShouldUpdateOwnedReview()
    {
        var dbContext = CreateDbContext();

        var user = CreateUser("update_user", "update_user@example.com", "Update User");
        var game = CreateGame("Update Test");

        dbContext.Users.Add(user);
        dbContext.Games.Add(game);

        var review = new Review
        {
            ReviewId = Guid.NewGuid(),
            UserId = user.Id,
            GameId = game.GameId,
            Text = "Initial review text",
            HasSpoilers = false,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow.AddDays(-1),
            User = user,
            Game = game
        };

        dbContext.Reviews.Add(review);
        await dbContext.SaveChangesAsync();

        var service = new ReviewService(dbContext);

        var dto = new UpdateReviewRequestDto
        {
            Text = "Updated review text with spoiler mention.",
            HasSpoilers = true
        };

        var result = await service.UpdateReviewAsync(user.Id, review.ReviewId, dto);

        result.ReviewId.Should().Be(review.ReviewId);
        result.Text.Should().Be("Updated review text with spoiler mention.");
        result.HasSpoilers.Should().BeTrue();

        var savedReview = await dbContext.Reviews.SingleAsync(r => r.ReviewId == review.ReviewId);
        savedReview.Text.Should().Be("Updated review text with spoiler mention.");
        savedReview.HasSpoilers.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateReviewAsync_ShouldThrow_WhenUserDoesNotOwnReview()
    {
        var dbContext = CreateDbContext();

        var owner = CreateUser("owner_user", "owner_user@example.com", "Owner User");
        var otherUserId = Guid.NewGuid();
        var game = CreateGame("Ownership Test");

        dbContext.Users.Add(owner);
        dbContext.Games.Add(game);

        var review = new Review
        {
            ReviewId = Guid.NewGuid(),
            UserId = owner.Id,
            GameId = game.GameId,
            Text = "Owner review",
            HasSpoilers = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            User = owner,
            Game = game
        };

        dbContext.Reviews.Add(review);
        await dbContext.SaveChangesAsync();

        var service = new ReviewService(dbContext);

        var dto = new UpdateReviewRequestDto
        {
            Text = "Unauthorized update attempt",
            HasSpoilers = false
        };

        var act = async () => await service.UpdateReviewAsync(otherUserId, review.ReviewId, dto);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*do not own this review*");
    }

    [Fact]
    public async Task DeleteReviewAsync_ShouldDeleteOwnedReview()
    {
        var dbContext = CreateDbContext();

        var user = CreateUser("delete_user", "delete_user@example.com", "Delete User");
        var game = CreateGame("Delete Review Test");

        dbContext.Users.Add(user);
        dbContext.Games.Add(game);

        var review = new Review
        {
            ReviewId = Guid.NewGuid(),
            UserId = user.Id,
            GameId = game.GameId,
            Text = "Delete me",
            HasSpoilers = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            User = user,
            Game = game
        };

        dbContext.Reviews.Add(review);
        await dbContext.SaveChangesAsync();

        var service = new ReviewService(dbContext);

        var deleted = await service.DeleteReviewAsync(user.Id, review.ReviewId);

        deleted.Should().BeTrue();
        dbContext.Reviews.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteReviewAsync_ShouldThrow_WhenUserDoesNotOwnReview()
    {
        var dbContext = CreateDbContext();

        var owner = CreateUser("delete_owner", "delete_owner@example.com", "Delete Owner");
        var otherUserId = Guid.NewGuid();
        var game = CreateGame("Delete Ownership Test");

        dbContext.Users.Add(owner);
        dbContext.Games.Add(game);

        var review = new Review
        {
            ReviewId = Guid.NewGuid(),
            UserId = owner.Id,
            GameId = game.GameId,
            Text = "Still mine",
            HasSpoilers = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            User = owner,
            Game = game
        };

        dbContext.Reviews.Add(review);
        await dbContext.SaveChangesAsync();

        var service = new ReviewService(dbContext);

        var act = async () => await service.DeleteReviewAsync(otherUserId, review.ReviewId);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*do not own this review*");

        dbContext.Reviews.Should().HaveCount(1);
    }

    [Fact]
    public async Task CreateReviewAsync_ShouldThrow_WhenGameDoesNotExist()
    {
        var dbContext = CreateDbContext();

        var user = CreateUser("missing_game", "missing_game@example.com", "Missing Game User");
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var service = new ReviewService(dbContext);

        var dto = new CreateReviewRequestDto
        {
            GameId = Guid.NewGuid(),
            Text = "Review for a missing game",
            HasSpoilers = false
        };

        var act = async () => await service.CreateReviewAsync(user.Id, dto);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*Game was not found*");
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

    private static Game CreateGame(string title)
    {
        return new Game
        {
            GameId = Guid.NewGuid(),
            Title = title,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
}