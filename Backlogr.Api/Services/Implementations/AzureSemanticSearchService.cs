using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using Backlogr.Api.DTOs.AI;
using Backlogr.Api.Options;
using Backlogr.Api.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace Backlogr.Api.Services.Implementations;

public sealed class AzureSemanticSearchService : ISemanticSearchService
{
    private readonly IEmbeddingService _embeddingService;
    private readonly SearchClient _searchClient;

    public AzureSemanticSearchService(
        IOptions<AzureAiSearchOptions> searchOptions,
        IEmbeddingService embeddingService)
    {
        var options = searchOptions.Value;

        _embeddingService = embeddingService;
        _searchClient = new SearchClient(
            new Uri(options.Endpoint),
            options.GamesIndexName,
            new AzureKeyCredential(options.ApiKey));
    }

    public async Task<IReadOnlyList<SemanticSearchResultDto>> SearchAsync(string query, int take = 10)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            throw new ArgumentException("Query is required.", nameof(query));
        }

        take = Math.Clamp(take, 1, 20);

        var queryVector = await _embeddingService.CreateEmbeddingAsync(query);

        var searchOptions = new SearchOptions
        {
            Size = take,
            IncludeTotalCount = false,
            VectorSearch = new()
            {
                Queries =
            {
                new VectorizedQuery(queryVector)
                {
                    KNearestNeighborsCount = Math.Max(take * 4, 40),
                    Weight = 3.0f,
                    Fields = { nameof(GameSearchDocument.ContentVector) }
                }
            }
            }
        };

        searchOptions.SearchFields.Add(nameof(GameSearchDocument.Title));
        searchOptions.SearchFields.Add(nameof(GameSearchDocument.Summary));
        searchOptions.SearchFields.Add(nameof(GameSearchDocument.Genres));
        searchOptions.SearchFields.Add(nameof(GameSearchDocument.Platforms));

        searchOptions.Select.Add(nameof(GameSearchDocument.GameId));
        searchOptions.Select.Add(nameof(GameSearchDocument.Title));
        searchOptions.Select.Add(nameof(GameSearchDocument.CoverImageUrl));
        searchOptions.Select.Add(nameof(GameSearchDocument.Summary));
        searchOptions.Select.Add(nameof(GameSearchDocument.Genres));
        searchOptions.Select.Add(nameof(GameSearchDocument.Platforms));

        var response = await _searchClient.SearchAsync<GameSearchDocument>(query, searchOptions);

        var results = new List<SemanticSearchResultDto>();

        await foreach (var result in response.Value.GetResultsAsync())
        {
            var doc = result.Document;
            if (doc is null || !Guid.TryParse(doc.GameId, out var gameId))
            {
                continue;
            }

            results.Add(new SemanticSearchResultDto
            {
                GameId = gameId,
                Title = doc.Title ?? string.Empty,
                CoverImageUrl = doc.CoverImageUrl,
                Summary = doc.Summary,
                Genres = doc.Genres,
                Platforms = doc.Platforms,
                Score = result.Score ?? 0,
                Why = "Matched by hybrid semantic search over title, summary, genres, and platforms."
            });
        }

        return results;
    }

    private sealed class GameSearchDocument
    {
        public string GameId { get; set; } = string.Empty;

        public string? Title { get; set; }

        public string? CoverImageUrl { get; set; }

        public string? Summary { get; set; }

        public string? Genres { get; set; }

        public string? Platforms { get; set; }

        public float[]? ContentVector { get; set; }
    }
}