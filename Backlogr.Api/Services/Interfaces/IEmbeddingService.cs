namespace Backlogr.Api.Services.Interfaces;

public interface IEmbeddingService
{
    Task<float[]> CreateEmbeddingAsync(
        string input,
        CancellationToken cancellationToken = default);
}