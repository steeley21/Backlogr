using Backlogr.Api.Data;
using Backlogr.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backlogr.Api.Services.Implementations;

public sealed class AiSearchSyncService : IAiSearchSyncService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IAiSearchIndexService _aiSearchIndexService;

    public AiSearchSyncService(
        ApplicationDbContext dbContext,
        IAiSearchIndexService aiSearchIndexService)
    {
        _dbContext = dbContext;
        _aiSearchIndexService = aiSearchIndexService;
    }

    public async Task BackfillGamesAsync(CancellationToken cancellationToken = default)
    {
        var games = await _dbContext.Games
            .AsNoTracking()
            .Where(game => game.Title != "Test Game")
            .OrderBy(game => game.GameId)
            .ToListAsync(cancellationToken);

        if (games.Count == 0)
        {
            return;
        }

        await _aiSearchIndexService.EnsureGamesIndexAsync(cancellationToken);
        await _aiSearchIndexService.UpsertGamesAsync(games, cancellationToken);
    }
}