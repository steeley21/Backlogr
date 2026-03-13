using Backlogr.Api.Data;
using Backlogr.Api.Models.Entities;
using Backlogr.Api.Services.Implementations;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Backlogr.Api.Tests.Services;

public sealed class FollowServiceTests
{
    [Fact]
    public async Task FollowAsync_ShouldCreateFollow_WhenValid()
    {
        var dbContext = CreateDbContext();

        var follower = CreateUser("follower_user", "follower_user@example.com", "Follower User");
        var following = CreateUser("following_user", "following_user@example.com", "Following User");

        dbContext.Users.AddRange(follower, following);
        await dbContext.SaveChangesAsync();

        var service = new FollowService(dbContext);

        await service.FollowAsync(follower.Id, following.Id);

        dbContext.Follows.Should().HaveCount(1);

        var follow = dbContext.Follows.Single();
        follow.FollowerId.Should().Be(follower.Id);
        follow.FollowingId.Should().Be(following.Id);
    }

    [Fact]
    public async Task FollowAsync_ShouldBeIdempotent_WhenAlreadyFollowing()
    {
        var dbContext = CreateDbContext();

        var follower = CreateUser("idempotent_follower", "idempotent_follower@example.com", "Idempotent Follower");
        var following = CreateUser("idempotent_following", "idempotent_following@example.com", "Idempotent Following");

        dbContext.Users.AddRange(follower, following);
        dbContext.Follows.Add(new Follow
        {
            FollowId = Guid.NewGuid(),
            FollowerId = follower.Id,
            FollowingId = following.Id,
            CreatedAt = DateTime.UtcNow,
            Follower = follower,
            Following = following
        });

        await dbContext.SaveChangesAsync();

        var service = new FollowService(dbContext);

        await service.FollowAsync(follower.Id, following.Id);

        dbContext.Follows.Should().HaveCount(1);
    }

    [Fact]
    public async Task FollowAsync_ShouldThrow_WhenTryingToFollowSelf()
    {
        var dbContext = CreateDbContext();

        var user = CreateUser("self_follow", "self_follow@example.com", "Self Follow");
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var service = new FollowService(dbContext);

        var act = async () => await service.FollowAsync(user.Id, user.Id);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*cannot follow yourself*");
    }

    [Fact]
    public async Task FollowAsync_ShouldThrow_WhenTargetUserDoesNotExist()
    {
        var dbContext = CreateDbContext();

        var follower = CreateUser("missing_target", "missing_target@example.com", "Missing Target");
        dbContext.Users.Add(follower);
        await dbContext.SaveChangesAsync();

        var service = new FollowService(dbContext);

        var act = async () => await service.FollowAsync(follower.Id, Guid.NewGuid());

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*User was not found*");
    }

    [Fact]
    public async Task UnfollowAsync_ShouldRemoveExistingFollow()
    {
        var dbContext = CreateDbContext();

        var follower = CreateUser("remove_follow", "remove_follow@example.com", "Remove Follow");
        var following = CreateUser("remove_target", "remove_target@example.com", "Remove Target");

        dbContext.Users.AddRange(follower, following);
        dbContext.Follows.Add(new Follow
        {
            FollowId = Guid.NewGuid(),
            FollowerId = follower.Id,
            FollowingId = following.Id,
            CreatedAt = DateTime.UtcNow,
            Follower = follower,
            Following = following
        });

        await dbContext.SaveChangesAsync();

        var service = new FollowService(dbContext);

        await service.UnfollowAsync(follower.Id, following.Id);

        dbContext.Follows.Should().BeEmpty();
    }

    [Fact]
    public async Task UnfollowAsync_ShouldBeNoOp_WhenFollowDoesNotExist()
    {
        var dbContext = CreateDbContext();

        var follower = CreateUser("noop_follower", "noop_follower@example.com", "Noop Follower");
        var following = CreateUser("noop_following", "noop_following@example.com", "Noop Following");

        dbContext.Users.AddRange(follower, following);
        await dbContext.SaveChangesAsync();

        var service = new FollowService(dbContext);

        var act = async () => await service.UnfollowAsync(follower.Id, following.Id);

        await act.Should().NotThrowAsync();
        dbContext.Follows.Should().BeEmpty();
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static ApplicationUser CreateUser(string userName, string email, string displayName)
    {
        return new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = userName,
            Email = email,
            DisplayName = displayName,
            CreatedAt = DateTime.UtcNow
        };
    }
}