using System.Threading;
using System.Threading.Tasks;

namespace Backlogr.Api.Services.Interfaces;

public interface ITwitchTokenService
{
    Task<string> GetAppAccessTokenAsync(CancellationToken cancellationToken = default);
}