using Backlogr.Api.Data;
using Backlogr.Api.Models.Entities;
using Backlogr.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backlogr.Api.Services.Implementations;

public sealed class FollowService : IFollowService
{
    private readonly ApplicationDbContext _dbContext;

    public FollowService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task FollowAsync(Guid followerId, Guid followingUserId)
    {
        if (followerId == followingUserId)
        {
            throw new ArgumentException("You cannot follow yourself.");
        }

        var targetUserExists = await _dbContext.Users
            .AnyAsync(u => u.Id == followingUserId);

        if (!targetUserExists)
        {
            throw new KeyNotFoundException("User was not found.");
        }

        var existingFollow = await _dbContext.Follows
            .AnyAsync(f => f.FollowerId == followerId && f.FollowingId == followingUserId);

        if (existingFollow)
        {
            return;
        }

        var follow = new Follow
        {
            FollowId = Guid.NewGuid(),
            FollowerId = followerId,
            FollowingId = followingUserId,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Follows.Add(follow);
        await _dbContext.SaveChangesAsync();
    }

    public async Task UnfollowAsync(Guid followerId, Guid followingUserId)
    {
        var existingFollow = await _dbContext.Follows
            .SingleOrDefaultAsync(f => f.FollowerId == followerId && f.FollowingId == followingUserId);

        if (existingFollow is null)
        {
            return;
        }

        _dbContext.Follows.Remove(existingFollow);
        await _dbContext.SaveChangesAsync();
    }
}