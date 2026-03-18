using Backlogr.Api.DTOs.Feed;

namespace Backlogr.Api.Services.Interfaces;

public interface IFeedService
{
    Task<IReadOnlyList<FeedItemResponseDto>> GetFeedAsync(Guid userId, FeedScope scope, int take = 25);
}