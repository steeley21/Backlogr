using Backlogr.Api.DTOs.AI;

namespace Backlogr.Api.Services.Interfaces;

public interface IReviewAssistantService
{
    Task<ReviewAssistantResponseDto> GenerateAsync(ReviewAssistantRequestDto dto);
}