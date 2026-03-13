namespace Backlogr.Api.Models.Entities;

public sealed class Review
{
    public Guid ReviewId { get; set; }

    public Guid UserId { get; set; }

    public Guid GameId { get; set; }

    public string Text { get; set; } = string.Empty;

    public bool HasSpoilers { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ApplicationUser User { get; set; } = null!;

    public Game Game { get; set; } = null!;

    public ICollection<ReviewLike> ReviewLikes { get; set; } = new List<ReviewLike>();

    public ICollection<ReviewComment> ReviewComments { get; set; } = new List<ReviewComment>();
}