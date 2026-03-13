using Backlogr.Api.Models.Enums;

namespace Backlogr.Api.Models.Entities;

public sealed class GameLog
{
    public Guid GameLogId { get; set; }

    public Guid UserId { get; set; }

    public Guid GameId { get; set; }

    public LibraryStatus Status { get; set; } = LibraryStatus.Backlog;

    public decimal? Rating { get; set; }

    public string? Platform { get; set; }

    public decimal? Hours { get; set; }

    public DateTime? StartedAt { get; set; }

    public DateTime? FinishedAt { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ApplicationUser User { get; set; } = null!;

    public Game Game { get; set; } = null!;
}