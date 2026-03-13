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

    public async Task<IReadOnlyList<FeedItemResponseDto>> GetFeedAsync(Guid userId, int take = 25)
    {
        take = Math.Clamp(take, 1, 100);

        var followedUserIds = await _dbContext.Follows
            .AsNoTracking()
            .Where(f => f.FollowerId == userId)
            .Select(f => f.FollowingId)
            .ToListAsync();

        if (followedUserIds.Count == 0)
        {
            return [];
        }

        var logItems = await _dbContext.GameLogs
            .AsNoTracking()
            .Where(gl => followedUserIds.Contains(gl.UserId))
            .Include(gl => gl.User)
            .Include(gl => gl.Game)
            .Select(gl => new FeedItemResponseDto
            {
                ItemType = FeedItemType.GameLog,
                ActivityAt = gl.UpdatedAt,
                UserId = gl.UserId,
                UserName = gl.User.UserName ?? string.Empty,
                DisplayName = gl.User.DisplayName,
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
                HasSpoilers = null
            })
            .ToListAsync();

        var reviewItems = await _dbContext.Reviews
            .AsNoTracking()
            .Where(r => followedUserIds.Contains(r.UserId))
            .Include(r => r.User)
            .Include(r => r.Game)
            .Select(r => new FeedItemResponseDto
            {
                ItemType = FeedItemType.Review,
                ActivityAt = r.UpdatedAt,
                UserId = r.UserId,
                UserName = r.User.UserName ?? string.Empty,
                DisplayName = r.User.DisplayName,
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
                HasSpoilers = r.HasSpoilers
            })
            .ToListAsync();

        return logItems
            .Concat(reviewItems)
            .OrderByDescending(item => item.ActivityAt)
            .Take(take)
            .ToList();
    }
}