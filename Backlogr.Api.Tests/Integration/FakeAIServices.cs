using Backlogr.Api.DTOs.AI;
using Backlogr.Api.Services.Interfaces;

namespace Backlogr.Api.Tests.Integration;

public sealed class FakeRecommendationService : IRecommendationService
{
    public Task<RecommendationResponseDto> GetRecommendationsAsync(Guid userId, int take = 5)
    {
        _ = userId;
        _ = take;

        return Task.FromResult(new RecommendationResponseDto
        {
            Items =
            [
                new RecommendedGameDto
                {
                    GameId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    Title = "Fake Recommended Game",
                    CoverImageUrl = null,
                    Why = "Recommended from fake AI service."
                }
            ]
        });
    }
}

public sealed class FakeReviewAssistantService : IReviewAssistantService
{
    public Task<ReviewAssistantResponseDto> GenerateAsync(ReviewAssistantRequestDto dto)
    {
        return Task.FromResult(new ReviewAssistantResponseDto
        {
            Mode = dto.Mode ?? "draft",
            ResultText = "Draft review from fake AI service."
        });
    }
}

public sealed class FakeSemanticSearchService : ISemanticSearchService
{
    public Task<IReadOnlyList<SemanticSearchResultDto>> SearchAsync(string query, int take = 10)
    {
        _ = query;
        _ = take;

        IReadOnlyList<SemanticSearchResultDto> results =
        [
            new SemanticSearchResultDto
            {
                GameId = Guid.Parse("17171717-1717-1717-1717-171717171717"),
                Title = "Cozy Fields",
                CoverImageUrl = null,
                Summary = "A cozy story game with crafting and exploration.",
                Genres = "Simulation, Indie",
                Platforms = "PC, Switch",
                Score = 0.99,
                Why = "Matched by fake semantic search."
            }
        ];

        return Task.FromResult(results);
    }
}