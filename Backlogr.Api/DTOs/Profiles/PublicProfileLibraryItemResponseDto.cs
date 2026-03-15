using Backlogr.Api.Models.Enums;

namespace Backlogr.Api.DTOs.Profiles;

public sealed class PublicProfileLibraryItemResponseDto
{
    public Guid GameLogId { get; set; }

    public Guid GameId { get; set; }

    public string GameTitle { get; set; } = string.Empty;

    public string? CoverImageUrl { get; set; }

    public LibraryStatus Status { get; set; }

    public decimal? Rating { get; set; }

    public string? Platform { get; set; }

    public decimal? Hours { get; set; }

    public DateTime? StartedAt { get; set; }

    public DateTime? FinishedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
