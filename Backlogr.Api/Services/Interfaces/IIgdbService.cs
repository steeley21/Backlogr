using Backlogr.Api.DTOs.Igdb;

namespace Backlogr.Api.Services.Interfaces;

public interface IIgdbService
{
    Task<IReadOnlyList<IgdbGameSearchResultDto>> SearchGamesAsync(string query, int take = 10);

    Task<ImportedGameResponseDto> ImportGameAsync(long igdbId);
}