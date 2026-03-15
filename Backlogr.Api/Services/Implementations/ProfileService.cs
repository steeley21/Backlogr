using Backlogr.Api.Data;
using Backlogr.Api.DTOs.Profiles;
using Backlogr.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backlogr.Api.Services.Implementations;

public sealed class ProfileService : IProfileService
{
    private readonly ApplicationDbContext _dbContext;

    public ProfileService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PublicProfileResponseDto> GetPublicProfileAsync(
        Guid currentUserId,
        string userName,
        int recentReviewCount = 6)
    {
        var user = await GetUserByUserNameAsync(userName);

        var followerCount = await _dbContext.Follows
            .AsNoTracking()
            .CountAsync(f => f.FollowingId == user.Id);

        var followingCount = await _dbContext.Follows
            .AsNoTracking()
            .CountAsync(f => f.FollowerId == user.Id);

        var reviewCount = await _dbContext.Reviews
            .AsNoTracking()
            .CountAsync(r => r.UserId == user.Id);

        var libraryCount = await _dbContext.GameLogs
            .AsNoTracking()
            .CountAsync(gl => gl.UserId == user.Id);

        var isSelf = user.Id == currentUserId;

        var isFollowing = !isSelf && await _dbContext.Follows
            .AsNoTracking()
            .AnyAsync(f => f.FollowerId == currentUserId && f.FollowingId == user.Id);

        recentReviewCount = Math.Clamp(recentReviewCount, 1, 12);

        var recentReviews = await _dbContext.Reviews
            .AsNoTracking()
            .Where(r => r.UserId == user.Id)
            .OrderByDescending(r => r.UpdatedAt)
            .Take(recentReviewCount)
            .Select(r => new PublicProfileReviewSummaryDto
            {
                ReviewId = r.ReviewId,
                GameId = r.GameId,
                GameTitle = r.Game.Title,
                CoverImageUrl = r.Game.CoverImageUrl,
                Text = r.Text,
                HasSpoilers = r.HasSpoilers,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt,
                LikeCount = r.ReviewLikes.Count,
                CommentCount = r.ReviewComments.Count,
                LikedByCurrentUser = r.ReviewLikes.Any(rl => rl.UserId == currentUserId),
                IsOwner = r.UserId == currentUserId
            })
            .ToListAsync();

        return new PublicProfileResponseDto
        {
            UserId = user.Id,
            UserName = user.UserName,
            DisplayName = user.DisplayName,
            Bio = user.Bio,
            AvatarUrl = user.AvatarUrl,
            FollowerCount = followerCount,
            FollowingCount = followingCount,
            ReviewCount = reviewCount,
            LibraryCount = libraryCount,
            IsFollowing = isFollowing,
            IsSelf = isSelf,
            RecentReviews = recentReviews
        };
    }

    public async Task<IReadOnlyList<PublicProfileLibraryItemResponseDto>> GetPublicLibraryAsync(string userName)
    {
        var user = await GetUserByUserNameAsync(userName);

        return await _dbContext.GameLogs
            .AsNoTracking()
            .Where(gl => gl.UserId == user.Id)
            .OrderByDescending(gl => gl.UpdatedAt)
            .Select(gl => new PublicProfileLibraryItemResponseDto
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
                UpdatedAt = gl.UpdatedAt
            })
            .ToListAsync();
    }

    private async Task<ProfileUserLookup> GetUserByUserNameAsync(string userName)
    {
        if (string.IsNullOrWhiteSpace(userName))
        {
            throw new ArgumentException("Username is required.", nameof(userName));
        }

        var normalizedUserName = NormalizeUserName(userName);

        var user = await _dbContext.Users
            .AsNoTracking()
            .Where(u => u.NormalizedUserName == normalizedUserName)
            .Select(u => new ProfileUserLookup
            {
                Id = u.Id,
                UserName = u.UserName ?? string.Empty,
                DisplayName = u.DisplayName,
                Bio = u.Bio,
                AvatarUrl = u.AvatarUrl
            })
            .SingleOrDefaultAsync();

        return user ?? throw new KeyNotFoundException("User profile was not found.");
    }

    private static string NormalizeUserName(string userName)
    {
        return userName.Trim().ToUpperInvariant();
    }

    private sealed class ProfileUserLookup
    {
        public Guid Id { get; set; }

        public string UserName { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public string? Bio { get; set; }

        public string? AvatarUrl { get; set; }
    }
}
