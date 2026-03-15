using Backlogr.Api.DTOs.Profiles;

namespace Backlogr.Api.Services.Interfaces;

public interface IProfileService
{
    Task<PublicProfileResponseDto> GetPublicProfileAsync(
        Guid currentUserId,
        string userName,
        int recentReviewCount = 6);

    Task<IReadOnlyList<PublicProfileLibraryItemResponseDto>> GetPublicLibraryAsync(string userName);
}
