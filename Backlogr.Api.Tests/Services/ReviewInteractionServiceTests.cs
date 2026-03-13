using Backlogr.Api.Data;
using Backlogr.Api.DTOs.Reviews;
using Backlogr.Api.Models.Entities;
using Backlogr.Api.Services.Implementations;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Backlogr.Api.Tests.Services;

public sealed class ReviewInteractionServiceTests
{
    [Fact]
    public async Task AddLikeAsync_ShouldCreateLike_WhenNotAlreadyLiked()
    {
        var dbContext = CreateDbContext();

        var user = CreateUser("like_user", "like_user@example.com", "Like User");
        var review = CreateReview(user, CreateGame("Like Test"));

        dbContext.Users.Add(user);
        dbContext.Games.Add(review.Game);
        dbContext.Reviews.Add(review);
        await dbContext.SaveChangesAsync();

        var service = new ReviewInteractionService(dbContext);

        await service.AddLikeAsync(user.Id, review.ReviewId);

        dbContext.ReviewLikes.Should().HaveCount(1);
        dbContext.ReviewLikes.Single().UserId.Should().Be(user.Id);
        dbContext.ReviewLikes.Single().ReviewId.Should().Be(review.ReviewId);
    }

    [Fact]
    public async Task AddLikeAsync_ShouldBeIdempotent_WhenAlreadyLiked()
    {
        var dbContext = CreateDbContext();

        var user = CreateUser("like_once", "like_once@example.com", "Like Once");
        var review = CreateReview(user, CreateGame("Idempotent Like"));

        dbContext.Users.Add(user);
        dbContext.Games.Add(review.Game);
        dbContext.Reviews.Add(review);
        dbContext.ReviewLikes.Add(new ReviewLike
        {
            ReviewLikeId = Guid.NewGuid(),
            UserId = user.Id,
            ReviewId = review.ReviewId,
            CreatedAt = DateTime.UtcNow,
            User = user,
            Review = review
        });

        await dbContext.SaveChangesAsync();

        var service = new ReviewInteractionService(dbContext);

        await service.AddLikeAsync(user.Id, review.ReviewId);

        dbContext.ReviewLikes.Should().HaveCount(1);
    }

    [Fact]
    public async Task RemoveLikeAsync_ShouldRemoveExistingLike()
    {
        var dbContext = CreateDbContext();

        var user = CreateUser("remove_like", "remove_like@example.com", "Remove Like");
        var review = CreateReview(user, CreateGame("Remove Like Test"));

        dbContext.Users.Add(user);
        dbContext.Games.Add(review.Game);
        dbContext.Reviews.Add(review);
        dbContext.ReviewLikes.Add(new ReviewLike
        {
            ReviewLikeId = Guid.NewGuid(),
            UserId = user.Id,
            ReviewId = review.ReviewId,
            CreatedAt = DateTime.UtcNow,
            User = user,
            Review = review
        });

        await dbContext.SaveChangesAsync();

        var service = new ReviewInteractionService(dbContext);

        await service.RemoveLikeAsync(user.Id, review.ReviewId);

        dbContext.ReviewLikes.Should().BeEmpty();
    }

    [Fact]
    public async Task AddCommentAsync_ShouldCreateComment_WhenValid()
    {
        var dbContext = CreateDbContext();

        var user = CreateUser("comment_user", "comment_user@example.com", "Comment User");
        var review = CreateReview(user, CreateGame("Comment Test"));

        dbContext.Users.Add(user);
        dbContext.Games.Add(review.Game);
        dbContext.Reviews.Add(review);
        await dbContext.SaveChangesAsync();

        var service = new ReviewInteractionService(dbContext);

        var result = await service.AddCommentAsync(user.Id, review.ReviewId, new CreateReviewCommentRequestDto
        {
            Text = "Nice review."
        });

        result.ReviewId.Should().Be(review.ReviewId);
        result.UserId.Should().Be(user.Id);
        result.UserName.Should().Be(user.UserName);
        result.DisplayName.Should().Be(user.DisplayName);
        result.Text.Should().Be("Nice review.");

        dbContext.ReviewComments.Should().HaveCount(1);
    }

    [Fact]
    public async Task AddCommentAsync_ShouldThrow_WhenTextIsBlank()
    {
        var dbContext = CreateDbContext();

        var user = CreateUser("blank_comment", "blank_comment@example.com", "Blank Comment");
        var review = CreateReview(user, CreateGame("Blank Comment Test"));

        dbContext.Users.Add(user);
        dbContext.Games.Add(review.Game);
        dbContext.Reviews.Add(review);
        await dbContext.SaveChangesAsync();

        var service = new ReviewInteractionService(dbContext);

        var act = async () => await service.AddCommentAsync(user.Id, review.ReviewId, new CreateReviewCommentRequestDto
        {
            Text = "   "
        });

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Comment text is required*");
    }

    [Fact]
    public async Task DeleteCommentAsync_ShouldDeleteOwnedComment()
    {
        var dbContext = CreateDbContext();

        var user = CreateUser("delete_comment", "delete_comment@example.com", "Delete Comment");
        var review = CreateReview(user, CreateGame("Delete Comment Test"));

        dbContext.Users.Add(user);
        dbContext.Games.Add(review.Game);
        dbContext.Reviews.Add(review);

        var comment = new ReviewComment
        {
            ReviewCommentId = Guid.NewGuid(),
            UserId = user.Id,
            ReviewId = review.ReviewId,
            Text = "Delete me",
            CreatedAt = DateTime.UtcNow,
            User = user,
            Review = review
        };

        dbContext.ReviewComments.Add(comment);
        await dbContext.SaveChangesAsync();

        var service = new ReviewInteractionService(dbContext);

        var deleted = await service.DeleteCommentAsync(user.Id, comment.ReviewCommentId, isAdmin: false);

        deleted.Should().BeTrue();
        dbContext.ReviewComments.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteCommentAsync_ShouldThrow_WhenUserDoesNotOwnComment_AndIsNotAdmin()
    {
        var dbContext = CreateDbContext();

        var owner = CreateUser("owner_comment", "owner_comment@example.com", "Owner Comment");
        var otherUserId = Guid.NewGuid();
        var review = CreateReview(owner, CreateGame("Comment Ownership Test"));

        dbContext.Users.Add(owner);
        dbContext.Games.Add(review.Game);
        dbContext.Reviews.Add(review);

        var comment = new ReviewComment
        {
            ReviewCommentId = Guid.NewGuid(),
            UserId = owner.Id,
            ReviewId = review.ReviewId,
            Text = "Still mine",
            CreatedAt = DateTime.UtcNow,
            User = owner,
            Review = review
        };

        dbContext.ReviewComments.Add(comment);
        await dbContext.SaveChangesAsync();

        var service = new ReviewInteractionService(dbContext);

        var act = async () => await service.DeleteCommentAsync(otherUserId, comment.ReviewCommentId, isAdmin: false);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*do not have permission*");
    }

    [Fact]
    public async Task DeleteCommentAsync_ShouldAllowAdmin_ToDeleteOthersComment()
    {
        var dbContext = CreateDbContext();

        var owner = CreateUser("admin_delete", "admin_delete@example.com", "Admin Delete");
        var adminUserId = Guid.NewGuid();
        var review = CreateReview(owner, CreateGame("Admin Delete Test"));

        dbContext.Users.Add(owner);
        dbContext.Games.Add(review.Game);
        dbContext.Reviews.Add(review);

        var comment = new ReviewComment
        {
            ReviewCommentId = Guid.NewGuid(),
            UserId = owner.Id,
            ReviewId = review.ReviewId,
            Text = "Admin can remove this",
            CreatedAt = DateTime.UtcNow,
            User = owner,
            Review = review
        };

        dbContext.ReviewComments.Add(comment);
        await dbContext.SaveChangesAsync();

        var service = new ReviewInteractionService(dbContext);

        var deleted = await service.DeleteCommentAsync(adminUserId, comment.ReviewCommentId, isAdmin: true);

        deleted.Should().BeTrue();
        dbContext.ReviewComments.Should().BeEmpty();
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

    private static Review CreateReview(ApplicationUser user, Game game)
    {
        return new Review
        {
            ReviewId = Guid.NewGuid(),
            UserId = user.Id,
            GameId = game.GameId,
            Text = "Base review",
            HasSpoilers = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            User = user,
            Game = game
        };
    }
}