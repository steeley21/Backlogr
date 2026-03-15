namespace Backlogr.Api.DTOs.Profiles;

public sealed class PublicProfileResponseDto
{
    public Guid UserId { get; set; }

    public string UserName { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string? Bio { get; set; }

    public string? AvatarUrl { get; set; }

    public int FollowerCount { get; set; }

    public int FollowingCount { get; set; }

    public int ReviewCount { get; set; }

    public int LibraryCount { get; set; }

    public bool IsFollowing { get; set; }

    public bool IsSelf { get; set; }

    public IReadOnlyList<PublicProfileReviewSummaryDto> RecentReviews { get; set; } = [];
}
