namespace Backlogr.Api.DTOs.Reviews;

public sealed class ReviewResponseDto
{
    public Guid ReviewId { get; set; }

    public Guid UserId { get; set; }

    public string UserName { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public Guid GameId { get; set; }

    public string GameTitle { get; set; } = string.Empty;

    public string Text { get; set; } = string.Empty;

    public bool HasSpoilers { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}