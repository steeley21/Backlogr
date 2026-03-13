namespace Backlogr.Api.Models.Entities;

public sealed class Game
{
    public Guid GameId { get; set; }

    public long? IgdbId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Slug { get; set; }

    public string? Summary { get; set; }

    public string? CoverImageUrl { get; set; }

    public DateTime? ReleaseDate { get; set; }

    public string? Developer { get; set; }

    public string? Publisher { get; set; }

    public string? Platforms { get; set; }

    public string? Genres { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<GameLog> GameLogs { get; set; } = new List<GameLog>();
}