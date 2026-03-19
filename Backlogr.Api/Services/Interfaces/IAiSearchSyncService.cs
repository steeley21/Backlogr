using System.Threading;
using System.Threading.Tasks;

namespace Backlogr.Api.Services.Interfaces;

public interface IAiSearchSyncService
{
    Task BackfillGamesAsync(CancellationToken cancellationToken = default);
}