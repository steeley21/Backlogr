using Backlogr.Api.DTOs.Admin;

namespace Backlogr.Api.Services.Interfaces;

public interface IDemoSeedService
{
    Task<AdminDemoSeedResponseDto> SeedAsync(
        Guid currentUserId,
        AdminDemoSeedRequestDto? request,
        CancellationToken cancellationToken = default);
}
