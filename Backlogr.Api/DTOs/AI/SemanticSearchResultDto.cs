namespace Backlogr.Api.DTOs.AI;

public sealed class SemanticSearchResultDto
{
    public Guid GameId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? CoverImageUrl { get; set; }

    public string? Summary { get; set; }

    public string? Genres { get; set; }

    public string? Platforms { get; set; }

    public double Score { get; set; }

    public string? Why { get; set; }
}