using Backlogr.Api.Data;
using Backlogr.Api.DTOs.Reviews;
using Backlogr.Api.Models.Entities;
using Backlogr.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backlogr.Api.Services.Implementations;

public sealed class ReviewInteractionService : IReviewInteractionService
{
    private readonly ApplicationDbContext _dbContext;

    public ReviewInteractionService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<ReviewCommentResponseDto>> GetCommentsAsync(Guid currentUserId, Guid reviewId)
    {
        await EnsureReviewExistsAsync(reviewId);

        return await _dbContext.ReviewComments
            .AsNoTracking()
            .Where(rc => rc.ReviewId == reviewId)
            .OrderBy(rc => rc.CreatedAt)
            .Select(rc => new ReviewCommentResponseDto
            {
                ReviewCommentId = rc.ReviewCommentId,
                ReviewId = rc.ReviewId,
                UserId = rc.UserId,
                UserName = rc.User.UserName ?? string.Empty,
                DisplayName = rc.User.DisplayName,
                AvatarUrl = rc.User.AvatarUrl,
                Text = rc.Text,
                CreatedAt = rc.CreatedAt,
                IsOwner = rc.UserId == currentUserId
            })
            .ToListAsync();
    }

    public async Task AddLikeAsync(Guid userId, Guid reviewId)
    {
        await EnsureReviewExistsAsync(reviewId);

        var existingLike = await _dbContext.ReviewLikes
            .AnyAsync(rl => rl.UserId == userId && rl.ReviewId == reviewId);

        if (existingLike)
        {
            return;
        }

        var like = new ReviewLike
        {
            ReviewLikeId = Guid.NewGuid(),
            UserId = userId,
            ReviewId = reviewId,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.ReviewLikes.Add(like);
        await _dbContext.SaveChangesAsync();
    }

    public async Task RemoveLikeAsync(Guid userId, Guid reviewId)
    {
        var existingLike = await _dbContext.ReviewLikes
            .SingleOrDefaultAsync(rl => rl.UserId == userId && rl.ReviewId == reviewId);

        if (existingLike is null)
        {
            return;
        }

        _dbContext.ReviewLikes.Remove(existingLike);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<ReviewCommentResponseDto> AddCommentAsync(
        Guid userId,
        Guid reviewId,
        CreateReviewCommentRequestDto dto)
    {
        ValidateCommentRequest(dto);
        await EnsureReviewExistsAsync(reviewId);

        var comment = new ReviewComment
        {
            ReviewCommentId = Guid.NewGuid(),
            UserId = userId,
            ReviewId = reviewId,
            Text = dto.Text.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.ReviewComments.Add(comment);
        await _dbContext.SaveChangesAsync();

        await _dbContext.Entry(comment).Reference(rc => rc.User).LoadAsync();

        return new ReviewCommentResponseDto
        {
            ReviewCommentId = comment.ReviewCommentId,
            ReviewId = comment.ReviewId,
            UserId = comment.UserId,
            UserName = comment.User.UserName ?? string.Empty,
            DisplayName = comment.User.DisplayName,
            AvatarUrl = comment.User.AvatarUrl,
            Text = comment.Text,
            CreatedAt = comment.CreatedAt,
            IsOwner = comment.UserId == userId
        };
    }

    public async Task<bool> DeleteCommentAsync(Guid userId, Guid reviewCommentId, bool isAdmin)
    {
        var comment = await _dbContext.ReviewComments
            .SingleOrDefaultAsync(rc => rc.ReviewCommentId == reviewCommentId);

        if (comment is null)
        {
            return false;
        }

        if (comment.UserId != userId && !isAdmin)
        {
            throw new UnauthorizedAccessException("You do not have permission to delete this comment.");
        }

        _dbContext.ReviewComments.Remove(comment);
        await _dbContext.SaveChangesAsync();

        return true;
    }

    private async Task EnsureReviewExistsAsync(Guid reviewId)
    {
        var exists = await _dbContext.Reviews.AnyAsync(r => r.ReviewId == reviewId);

        if (!exists)
        {
            throw new KeyNotFoundException("Review was not found.");
        }
    }

    private static void ValidateCommentRequest(CreateReviewCommentRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Text))
        {
            throw new ArgumentException("Comment text is required.");
        }

        if (dto.Text.Trim().Length > 2000)
        {
            throw new ArgumentException("Comment text cannot exceed 2000 characters.");
        }
    }
}