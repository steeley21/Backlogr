using Backlogr.Api.Models.Entities;

namespace Backlogr.Api.Services.Interfaces;

public interface IUserDeletionService
{
    Task DeleteUserAsync(ApplicationUser user, CancellationToken cancellationToken = default);

    Task<bool> IsLastRemainingSuperAdminAsync(Guid userId, CancellationToken cancellationToken = default);
}
