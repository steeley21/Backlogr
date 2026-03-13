namespace Backlogr.Api.Models.Entities;

public sealed class Follow
{
    public Guid FollowId { get; set; }

    public Guid FollowerId { get; set; }

    public Guid FollowingId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ApplicationUser Follower { get; set; } = null!;

    public ApplicationUser Following { get; set; } = null!;
}