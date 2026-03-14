using Backlogr.Api.Common;
using Backlogr.Api.Data;
using Backlogr.Api.Models.Entities;
using Backlogr.Api.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Backlogr.Api.Services.Implementations;

public sealed class UserDeletionService : IUserDeletionService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;

    public UserDeletionService(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager)
    {
        _dbContext = dbContext;
        _userManager = userManager;
    }

    public async Task DeleteUserAsync(ApplicationUser user, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(user);

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        await _dbContext.ReviewLikes
            .Where(reviewLike => reviewLike.UserId == user.Id)
            .ExecuteDeleteAsync(cancellationToken);

        await _dbContext.ReviewComments
            .Where(reviewComment => reviewComment.UserId == user.Id)
            .ExecuteDeleteAsync(cancellationToken);

        await _dbContext.Follows
            .Where(follow => follow.FollowerId == user.Id || follow.FollowingId == user.Id)
            .ExecuteDeleteAsync(cancellationToken);

        var deleteResult = await _userManager.DeleteAsync(user);
        if (!deleteResult.Succeeded)
        {
            await transaction.RollbackAsync(cancellationToken);
            var errors = string.Join("; ", deleteResult.Errors.Select(error => error.Description));
            throw new InvalidOperationException($"Failed to delete the user. Errors: {errors}");
        }

        await transaction.CommitAsync(cancellationToken);
    }

    public async Task<bool> IsLastRemainingSuperAdminAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var superAdmins = await _userManager.GetUsersInRoleAsync(RoleNames.SuperAdmin);
        return superAdmins.Count == 1 && superAdmins[0].Id == userId;
    }
}
