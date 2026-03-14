using Backlogr.Api.DTOs.Admin;

namespace Backlogr.Api.Services.Interfaces;

public interface IAdminService
{
    Task<IReadOnlyList<AdminUserSummaryDto>> GetUsersAsync(CancellationToken cancellationToken = default);

    Task<AdminUserSummaryDto> CreateUserAsync(
        Guid currentUserId,
        AdminCreateUserRequestDto dto,
        CancellationToken cancellationToken = default);

    Task<AdminUserSummaryDto> UpdateUserRoleAsync(
        Guid currentUserId,
        Guid targetUserId,
        AdminUpdateUserRoleRequestDto dto,
        CancellationToken cancellationToken = default);
}
