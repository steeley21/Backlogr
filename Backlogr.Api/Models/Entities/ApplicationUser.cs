using Microsoft.AspNetCore.Identity;

namespace Backlogr.Api.Models.Entities;

public sealed class ApplicationUser : IdentityUser<Guid>
{
    public string DisplayName { get; set; } = string.Empty;

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? AvatarUrl { get; set; }

    public string? Bio { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<GameLog> GameLogs { get; set; } = new List<GameLog>();

    public ICollection<Review> Reviews { get; set; } = new List<Review>();

    public ICollection<ReviewLike> ReviewLikes { get; set; } = new List<ReviewLike>();

    public ICollection<ReviewComment> ReviewComments { get; set; } = new List<ReviewComment>();
}