using Backlogr.Api.DTOs.Feed;

namespace Backlogr.Api.Services.Interfaces;

public interface IFeedService
{
    Task<IReadOnlyList<FeedItemResponseDto>> GetFeedAsync(Guid userId, int take = 25);
}