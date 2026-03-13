namespace Backlogr.Api.Models.Entities;

public sealed class ReviewComment
{
    public Guid ReviewCommentId { get; set; }

    public Guid UserId { get; set; }

    public Guid ReviewId { get; set; }

    public string Text { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ApplicationUser User { get; set; } = null!;

    public Review Review { get; set; } = null!;
}