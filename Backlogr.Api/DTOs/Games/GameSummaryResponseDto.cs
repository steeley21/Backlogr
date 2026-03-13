namespace Backlogr.Api.DTOs.Games;

public sealed class GameSummaryResponseDto
{
    public Guid GameId { get; set; }

    public long? IgdbId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? CoverImageUrl { get; set; }

    public DateTime? ReleaseDate { get; set; }

    public string? Platforms { get; set; }

    public string? Genres { get; set; }
}