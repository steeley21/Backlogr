namespace Backlogr.Api.DTOs.Igdb;

public sealed class IgdbGameSearchResultDto
{
    public long IgdbId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Summary { get; set; }

    public string? CoverImageUrl { get; set; }

    public DateTime? ReleaseDate { get; set; }

    public string? Developer { get; set; }

    public string? Publisher { get; set; }

    public string? Platforms { get; set; }

    public string? Genres { get; set; }
}