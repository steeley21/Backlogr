using Backlogr.Api.Data;
using Backlogr.Api.DTOs.Reviews;
using Backlogr.Api.Models.Entities;
using Backlogr.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backlogr.Api.Services.Implementations;

public sealed class ReviewService : IReviewService
{
    private readonly ApplicationDbContext _dbContext;

    public ReviewService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ReviewResponseDto> CreateReviewAsync(Guid userId, CreateReviewRequestDto dto)
    {
        ValidateCreateRequest(dto);
        await EnsureGameExistsAsync(dto.GameId);

        var existingReview = await _dbContext.Reviews
            .AnyAsync(r => r.UserId == userId && r.GameId == dto.GameId);

        if (existingReview)
        {
            throw new InvalidOperationException("You already have a review for this game.");
        }

        var review = new Review
        {
            ReviewId = Guid.NewGuid(),
            UserId = userId,
            GameId = dto.GameId,
            Text = dto.Text.Trim(),
            HasSpoilers = dto.HasSpoilers,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _dbContext.Reviews.Add(review);
        await _dbContext.SaveChangesAsync();

        await _dbContext.Entry(review).Reference(r => r.User).LoadAsync();
        await _dbContext.Entry(review).Reference(r => r.Game).LoadAsync();

        return MapResponse(review);
    }

    public async Task<ReviewResponseDto> UpdateReviewAsync(Guid userId, Guid reviewId, UpdateReviewRequestDto dto)
    {
        ValidateUpdateRequest(dto);

        var review = await _dbContext.Reviews
            .Include(r => r.User)
            .Include(r => r.Game)
            .SingleOrDefaultAsync(r => r.ReviewId == reviewId);

        if (review is null)
        {
            throw new KeyNotFoundException("Review was not found.");
        }

        if (review.UserId != userId)
        {
            throw new UnauthorizedAccessException("You do not own this review.");
        }

        review.Text = dto.Text.Trim();
        review.HasSpoilers = dto.HasSpoilers;
        review.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return MapResponse(review);
    }

    public async Task<bool> DeleteReviewAsync(Guid userId, Guid reviewId)
    {
        var review = await _dbContext.Reviews
            .SingleOrDefaultAsync(r => r.ReviewId == reviewId);

        if (review is null)
        {
            return false;
        }

        if (review.UserId != userId)
        {
            throw new UnauthorizedAccessException("You do not own this review.");
        }

        _dbContext.Reviews.Remove(review);
        await _dbContext.SaveChangesAsync();

        return true;
    }

    private async Task EnsureGameExistsAsync(Guid gameId)
    {
        var exists = await _dbContext.Games.AnyAsync(g => g.GameId == gameId);

        if (!exists)
        {
            throw new KeyNotFoundException("Game was not found.");
        }
    }

    private static void ValidateCreateRequest(CreateReviewRequestDto dto)
    {
        if (dto.GameId == Guid.Empty)
        {
            throw new ArgumentException("GameId is required.");
        }

        if (string.IsNullOrWhiteSpace(dto.Text))
        {
            throw new ArgumentException("Review text is required.");
        }

        if (dto.Text.Trim().Length > 4000)
        {
            throw new ArgumentException("Review text cannot exceed 4000 characters.");
        }
    }

    private static void ValidateUpdateRequest(UpdateReviewRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Text))
        {
            throw new ArgumentException("Review text is required.");
        }

        if (dto.Text.Trim().Length > 4000)
        {
            throw new ArgumentException("Review text cannot exceed 4000 characters.");
        }
    }

    private static ReviewResponseDto MapResponse(Review review)
    {
        return new ReviewResponseDto
        {
            ReviewId = review.ReviewId,
            UserId = review.UserId,
            UserName = review.User.UserName ?? string.Empty,
            DisplayName = review.User.DisplayName,
            GameId = review.GameId,
            GameTitle = review.Game.Title,
            Text = review.Text,
            HasSpoilers = review.HasSpoilers,
            CreatedAt = review.CreatedAt,
            UpdatedAt = review.UpdatedAt
        };
    }
}