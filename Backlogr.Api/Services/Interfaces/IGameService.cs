using Backlogr.Api.DTOs.Games;

namespace Backlogr.Api.Services.Interfaces;

public interface IGameService
{
    Task<IReadOnlyList<GameSummaryResponseDto>> SearchGamesAsync(string? query, int take = 25);

    Task<IReadOnlyList<GameBrowseResultDto>> SearchBrowseGamesAsync(string? query, int take = 25);

    Task<GameDetailResponseDto?> GetGameByIdAsync(Guid gameId);
}