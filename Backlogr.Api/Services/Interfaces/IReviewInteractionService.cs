using Backlogr.Api.DTOs.Reviews;

namespace Backlogr.Api.Services.Interfaces;

public interface IReviewInteractionService
{
    Task<IReadOnlyList<ReviewCommentResponseDto>> GetCommentsAsync(Guid currentUserId, Guid reviewId);

    Task AddLikeAsync(Guid userId, Guid reviewId);

    Task RemoveLikeAsync(Guid userId, Guid reviewId);

    Task<ReviewCommentResponseDto> AddCommentAsync(
        Guid userId,
        Guid reviewId,
        CreateReviewCommentRequestDto dto);

    Task<bool> DeleteCommentAsync(Guid userId, Guid reviewCommentId, bool isAdmin);
}