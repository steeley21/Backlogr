using Backlogr.Api.Data;
using Backlogr.Api.DTOs.Feed;
using Backlogr.Api.DTOs.Games;
using Backlogr.Api.DTOs.Library;
using Backlogr.Api.DTOs.Reviews;
using Backlogr.Api.Models.Enums;
using Backlogr.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backlogr.Api.Services.Implementations;

public sealed class GameDetailsService : IGameDetailsService
{
    private readonly ApplicationDbContext _dbContext;

    public GameDetailsService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GameViewerStateResponseDto?> GetViewerStateAsync(Guid currentUserId, Guid gameId)
    {
        if (!await GameExistsAsync(gameId))
        {
            return null;
        }

        var log = await _dbContext.GameLogs
            .AsNoTracking()
            .Where(gl => gl.UserId == currentUserId && gl.GameId == gameId)
            .Select(gl => new LibraryLogResponseDto
            {
                GameLogId = gl.GameLogId,
                GameId = gl.GameId,
                GameTitle = gl.Game.Title,
                CoverImageUrl = gl.Game.CoverImageUrl,
                Status = gl.Status,
                Rating = gl.Rating,
                Platform = gl.Platform,
                Hours = gl.Hours,
                StartedAt = gl.StartedAt,
                FinishedAt = gl.FinishedAt,
                Notes = gl.Notes,
                UpdatedAt = gl.UpdatedAt,
            })
            .SingleOrDefaultAsync();

        var review = await _dbContext.Reviews
            .AsNoTracking()
            .Where(r => r.UserId == currentUserId && r.GameId == gameId)
            .Select(r => new ReviewResponseDto
            {
                ReviewId = r.ReviewId,
                UserId = r.UserId,
                UserName = r.User.UserName ?? string.Empty,
                DisplayName = r.User.DisplayName,
                GameId = r.GameId,
                GameTitle = r.Game.Title,
                Text = r.Text,
                HasSpoilers = r.HasSpoilers,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt,
            })
            .SingleOrDefaultAsync();

        return new GameViewerStateResponseDto
        {
            Log = log,
            Review = review,
        };
    }

    public async Task<IReadOnlyList<FeedItemResponseDto>?> GetGameReviewsAsync(
        Guid currentUserId,
        Guid gameId,
        int take = 20)
    {
        if (!await GameExistsAsync(gameId))
        {
            return null;
        }

        take = Math.Clamp(take, 1, 100);

        return await _dbContext.Reviews
            .AsNoTracking()
            .Where(r => r.GameId == gameId)
            .OrderByDescending(r => r.UpdatedAt)
            .Take(take)
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
                GameLogId = _dbContext.GameLogs
                    .Where(gl => gl.UserId == r.UserId && gl.GameId == r.GameId)
                    .Select(gl => (Guid?)gl.GameLogId)
                    .FirstOrDefault(),
                ReviewId = r.ReviewId,
                Status = _dbContext.GameLogs
                    .Where(gl => gl.UserId == r.UserId && gl.GameId == r.GameId)
                    .Select(gl => (LibraryStatus?)gl.Status)
                    .FirstOrDefault(),
                Rating = _dbContext.GameLogs
                    .Where(gl => gl.UserId == r.UserId && gl.GameId == r.GameId)
                    .Select(gl => gl.Rating)
                    .FirstOrDefault(),
                Platform = _dbContext.GameLogs
                    .Where(gl => gl.UserId == r.UserId && gl.GameId == r.GameId)
                    .Select(gl => gl.Platform)
                    .FirstOrDefault(),
                Hours = _dbContext.GameLogs
                    .Where(gl => gl.UserId == r.UserId && gl.GameId == r.GameId)
                    .Select(gl => gl.Hours)
                    .FirstOrDefault(),
                ReviewText = r.Text,
                HasSpoilers = r.HasSpoilers,
                LikeCount = r.ReviewLikes.Count,
                CommentCount = r.ReviewComments.Count,
                LikedByCurrentUser = r.ReviewLikes.Any(rl => rl.UserId == currentUserId),
                IsOwner = r.UserId == currentUserId,
            })
            .ToListAsync();
    }

    public async Task<IReadOnlyList<FeedItemResponseDto>?> GetGameActivityAsync(
        Guid currentUserId,
        Guid gameId,
        int take = 25)
    {
        if (!await GameExistsAsync(gameId))
        {
            return null;
        }

        take = Math.Clamp(take, 1, 100);

        var logItems = await _dbContext.GameLogs
            .AsNoTracking()
            .Where(gl => gl.GameId == gameId)
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
                IsOwner = gl.UserId == currentUserId,
            })
            .ToListAsync();

        var reviewItems = await _dbContext.Reviews
            .AsNoTracking()
            .Where(r => r.GameId == gameId)
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
                GameLogId = _dbContext.GameLogs
                    .Where(gl => gl.UserId == r.UserId && gl.GameId == r.GameId)
                    .Select(gl => (Guid?)gl.GameLogId)
                    .FirstOrDefault(),
                ReviewId = r.ReviewId,
                Status = _dbContext.GameLogs
                    .Where(gl => gl.UserId == r.UserId && gl.GameId == r.GameId)
                    .Select(gl => (LibraryStatus?)gl.Status)
                    .FirstOrDefault(),
                Rating = _dbContext.GameLogs
                    .Where(gl => gl.UserId == r.UserId && gl.GameId == r.GameId)
                    .Select(gl => gl.Rating)
                    .FirstOrDefault(),
                Platform = _dbContext.GameLogs
                    .Where(gl => gl.UserId == r.UserId && gl.GameId == r.GameId)
                    .Select(gl => gl.Platform)
                    .FirstOrDefault(),
                Hours = _dbContext.GameLogs
                    .Where(gl => gl.UserId == r.UserId && gl.GameId == r.GameId)
                    .Select(gl => gl.Hours)
                    .FirstOrDefault(),
                ReviewText = r.Text,
                HasSpoilers = r.HasSpoilers,
                LikeCount = r.ReviewLikes.Count,
                CommentCount = r.ReviewComments.Count,
                LikedByCurrentUser = r.ReviewLikes.Any(rl => rl.UserId == currentUserId),
                IsOwner = r.UserId == currentUserId,
            })
            .ToListAsync();

        return logItems
            .Concat(reviewItems)
            .OrderByDescending(item => item.ActivityAt)
            .Take(take)
            .ToList();
    }

    private Task<bool> GameExistsAsync(Guid gameId)
    {
        return _dbContext.Games
            .AsNoTracking()
            .AnyAsync(g => g.GameId == gameId);
    }
}
