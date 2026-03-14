using Backlogr.Api.DTOs.Admin;

namespace Backlogr.Api.Services.Interfaces;

public interface ISuperAdminBootstrapService
{
    Task<AdminUserSummaryDto> BootstrapAsync(string providedSecret, CancellationToken cancellationToken = default);
}
