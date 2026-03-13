namespace Backlogr.Api.DTOs.AI;

public sealed class RecommendationResponseDto
{
    public IReadOnlyList<RecommendedGameDto> Items { get; set; } = [];
}