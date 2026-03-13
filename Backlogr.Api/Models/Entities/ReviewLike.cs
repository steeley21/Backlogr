namespace Backlogr.Api.Models.Entities;

public sealed class ReviewLike
{
    public Guid ReviewLikeId { get; set; }

    public Guid UserId { get; set; }

    public Guid ReviewId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ApplicationUser User { get; set; } = null!;

    public Review Review { get; set; } = null!;
}