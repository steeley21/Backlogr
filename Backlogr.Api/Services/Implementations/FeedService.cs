using Backlogr.Api.Data;
using Backlogr.Api.DTOs.Feed;
using Backlogr.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backlogr.Api.Services.Implementations;

public sealed class FeedService : IFeedService
{
    private readonly ApplicationDbContext _dbContext;

    public FeedService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<FeedItemResponseDto>> GetFeedAsync(Guid userId, FeedScope scope, int take = 25)
    {
        take = Math.Clamp(take, 1, 100);

        List<Guid>? activityUserIds = null;

        if (scope == FeedScope.Following)
        {
            activityUserIds = await _dbContext.Follows
                .AsNoTracking()
                .Where(f => f.FollowerId == userId)
                .Select(f => f.FollowingId)
                .ToListAsync();

            activityUserIds.Add(userId);
            activityUserIds = activityUserIds
                .Distinct()
                .ToList();
        }

        IQueryable<Models.Entities.GameLog> logQuery = _dbContext.GameLogs
            .AsNoTracking();

        if (activityUserIds is not null)
        {
            logQuery = logQuery.Where(gl => activityUserIds.Contains(gl.UserId));
        }

        var logItems = await logQuery
            .Include(gl => gl.User)
            .Include(gl => gl.Game)
            .Select(gl => new FeedItemResponseDto
            {
                ItemType = FeedItemType.GameLog,
                ActivityAt = gl.UpdatedAt,
                UserId = gl.UserId,
                UserName = gl.User.UserName ?? string.Empty,
                DisplayName = gl.User.DisplayName,
                AvatarUrl = gl.User.AvatarUrl,
                GameId = gl.GameId,
                GameTitle = gl.Game.Title,
                CoverImageUrl = gl.Game.CoverImageUrl,
                GameLogId = gl.GameLogId,
                ReviewId = null,
                Status = gl.Status,
                Rating = gl.Rating,
                Platform = gl.Platform,
                Hours = gl.Hours,
                ReviewText = null,
                HasSpoilers = null,
                LikeCount = 0,
                CommentCount = 0,
                LikedByCurrentUser = false,
                IsOwner = gl.UserId == userId,
            })
            .ToListAsync();

        IQueryable<Models.Entities.Review> reviewQuery = _dbContext.Reviews
            .AsNoTracking();

        if (activityUserIds is not null)
        {
            reviewQuery = reviewQuery.Where(r => activityUserIds.Contains(r.UserId));
        }

        var reviewItems = await reviewQuery
            .Include(r => r.User)
            .Include(r => r.Game)
            .Select(r => new FeedItemResponseDto
            {
                ItemType = FeedItemType.Review,
                ActivityAt = r.UpdatedAt,
                UserId = r.UserId,
                UserName = r.User.UserName ?? string.Empty,
                DisplayName = r.User.DisplayName,
                AvatarUrl = r.User.AvatarUrl,
                GameId = r.GameId,
                GameTitle = r.Game.Title,
                CoverImageUrl = r.Game.CoverImageUrl,
                GameLogId = null,
                ReviewId = r.ReviewId,
                Status = null,
                Rating = null,
                Platform = null,
                Hours = null,
                ReviewText = r.Text,
                HasSpoilers = r.HasSpoilers,
                LikeCount = r.ReviewLikes.Count,
                CommentCount = r.ReviewComments.Count,
                LikedByCurrentUser = r.ReviewLikes.Any(rl => rl.UserId == userId),
                IsOwner = r.UserId == userId,
            })
            .ToListAsync();

        return logItems
            .Concat(reviewItems)
            .OrderByDescending(item => item.ActivityAt)
            .Take(take)
            .ToList();
    }
}