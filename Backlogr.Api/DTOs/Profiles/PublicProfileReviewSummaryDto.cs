namespace Backlogr.Api.DTOs.Profiles;

public sealed class PublicProfileReviewSummaryDto
{
    public Guid ReviewId { get; set; }

    public Guid GameId { get; set; }

    public string GameTitle { get; set; } = string.Empty;

    public string? CoverImageUrl { get; set; }

    public string Text { get; set; } = string.Empty;

    public bool HasSpoilers { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public int LikeCount { get; set; }

    public int CommentCount { get; set; }

    public bool LikedByCurrentUser { get; set; }

    public bool IsOwner { get; set; }
}
