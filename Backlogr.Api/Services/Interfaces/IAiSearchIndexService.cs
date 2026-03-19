using Backlogr.Api.Models.Entities;

namespace Backlogr.Api.Services.Interfaces;

public interface IAiSearchIndexService
{
    Task EnsureGamesIndexAsync(CancellationToken cancellationToken = default);

    Task UpsertGameAsync(
        Game game,
        CancellationToken cancellationToken = default);

    Task UpsertGamesAsync(
        IEnumerable<Game> games,
        CancellationToken cancellationToken = default);
}