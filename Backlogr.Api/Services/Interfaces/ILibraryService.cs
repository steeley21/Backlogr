using Backlogr.Api.DTOs.Library;

namespace Backlogr.Api.Services.Interfaces;

public interface ILibraryService
{
    Task<IReadOnlyList<LibraryLogResponseDto>> GetLibraryAsync(Guid userId);

    Task<LibraryLogResponseDto> UpsertLibraryLogAsync(Guid userId, UpsertLibraryLogRequestDto dto);

    Task<bool> DeleteLibraryLogAsync(Guid userId, Guid gameId);
}