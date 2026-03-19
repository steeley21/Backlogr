using Backlogr.Api.DTOs.Feed;
using Backlogr.Api.DTOs.Games;

namespace Backlogr.Api.Services.Interfaces;

public interface IGameDetailsService
{
    Task<GameViewerStateResponseDto?> GetViewerStateAsync(Guid currentUserId, Guid gameId);

    Task<IReadOnlyList<FeedItemResponseDto>?> GetGameReviewsAsync(
        Guid currentUserId,
        Guid gameId,
        int take = 20);

    Task<IReadOnlyList<FeedItemResponseDto>?> GetGameActivityAsync(
        Guid currentUserId,
        Guid gameId,
        int take = 25);
}
