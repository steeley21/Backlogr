using Backlogr.Api.DTOs.AI;

namespace Backlogr.Api.Services.Interfaces;

public interface IRecommendationService
{
    Task<RecommendationResponseDto> GetRecommendationsAsync(Guid userId, int take = 5);
}