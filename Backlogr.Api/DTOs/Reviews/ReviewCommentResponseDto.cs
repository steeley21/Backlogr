namespace Backlogr.Api.DTOs.Reviews;

public sealed class ReviewCommentResponseDto
{
    public Guid ReviewCommentId { get; set; }

    public Guid ReviewId { get; set; }

    public Guid UserId { get; set; }

    public string UserName { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string Text { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}