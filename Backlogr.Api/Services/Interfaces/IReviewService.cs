using Backlogr.Api.DTOs.Reviews;

namespace Backlogr.Api.Services.Interfaces;

public interface IReviewService
{
    Task<ReviewResponseDto> CreateReviewAsync(Guid userId, CreateReviewRequestDto dto);

    Task<ReviewResponseDto> UpdateReviewAsync(Guid userId, Guid reviewId, UpdateReviewRequestDto dto);

    Task<bool> DeleteReviewAsync(Guid userId, Guid reviewId);
}