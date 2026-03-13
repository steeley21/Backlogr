namespace Backlogr.Api.DTOs.AI;

public sealed class RecommendedGameDto
{
    public Guid GameId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? CoverImageUrl { get; set; }

    public string? Why { get; set; }
}