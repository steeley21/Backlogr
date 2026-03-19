using Backlogr.Api.Options;
using Backlogr.Api.Services.Interfaces;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Embeddings;

namespace Backlogr.Api.Services.Implementations;

public sealed class OpenAiEmbeddingService : IEmbeddingService
{
    private readonly EmbeddingClient _embeddingClient;

    public OpenAiEmbeddingService(IOptions<OpenAiOptions> openAiOptions)
    {
        var options = openAiOptions.Value;

        var openAiClient = new OpenAIClient(options.ApiKey);
        _embeddingClient = openAiClient.GetEmbeddingClient(options.EmbeddingModel);
    }

    public Task<float[]> CreateEmbeddingAsync(
        string input,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            throw new ArgumentException("Embedding input is required.", nameof(input));
        }

        var result = _embeddingClient.GenerateEmbedding(input);
        var vector = result.Value.ToFloats().ToArray();

        return Task.FromResult(vector);
    }
}