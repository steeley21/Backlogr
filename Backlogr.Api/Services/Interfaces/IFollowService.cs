namespace Backlogr.Api.Services.Interfaces;

public interface IFollowService
{
    Task FollowAsync(Guid followerId, Guid followingUserId);

    Task UnfollowAsync(Guid followerId, Guid followingUserId);
}