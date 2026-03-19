using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Backlogr.Api.Models.Entities;
using Backlogr.Api.Options;
using Backlogr.Api.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace Backlogr.Api.Services.Implementations;

public sealed class AzureAiSearchIndexService : IAiSearchIndexService
{
    private const string VectorProfileName = "games-vector-profile";
    private const string HnswAlgorithmName = "games-hnsw";
    private const int EmbeddingDimensions = 1536;

    private readonly IEmbeddingService _embeddingService;
    private readonly SearchIndexClient _indexClient;
    private readonly SearchClient _searchClient;
    private readonly string _indexName;

    public AzureAiSearchIndexService(
        IOptions<AzureAiSearchOptions> searchOptions,
        IEmbeddingService embeddingService)
    {
        var options = searchOptions.Value;

        _embeddingService = embeddingService;
        _indexName = options.GamesIndexName;

        var endpoint = new Uri(options.Endpoint);
        var credential = new AzureKeyCredential(options.ApiKey);

        _indexClient = new SearchIndexClient(endpoint, credential);
        _searchClient = _indexClient.GetSearchClient(_indexName);
    }

    public async Task EnsureGamesIndexAsync(CancellationToken cancellationToken = default)
    {
        var index = new SearchIndex(_indexName)
        {
            Fields =
            {
                new SimpleField(nameof(GameSearchDocument.Id), SearchFieldDataType.String)
                {
                    IsKey = true,
                    IsFilterable = true,
                    IsSortable = true,
                },
                new SimpleField(nameof(GameSearchDocument.GameId), SearchFieldDataType.String)
                {
                    IsFilterable = true,
                    IsSortable = true,
                },
                new SimpleField(nameof(GameSearchDocument.IgdbId), SearchFieldDataType.Int64)
                {
                    IsFilterable = true,
                    IsSortable = true,
                },
                new SearchableField(nameof(GameSearchDocument.Title))
                {
                    IsFilterable = true,
                    IsSortable = true,
                },
                new SearchableField(nameof(GameSearchDocument.Summary)),
                new SearchableField(nameof(GameSearchDocument.Genres))
                {
                    IsFilterable = true,
                    IsFacetable = true,
                },
                new SearchableField(nameof(GameSearchDocument.Platforms))
                {
                    IsFilterable = true,
                    IsFacetable = true,
                },
                new SearchableField(nameof(GameSearchDocument.Developer))
                {
                    IsFilterable = true,
                    IsFacetable = true,
                },
                new SearchableField(nameof(GameSearchDocument.Publisher))
                {
                    IsFilterable = true,
                    IsFacetable = true,
                },
                new SimpleField(nameof(GameSearchDocument.CoverImageUrl), SearchFieldDataType.String),
                new SearchableField(nameof(GameSearchDocument.SearchText)),
                new SearchField(
                    nameof(GameSearchDocument.ContentVector),
                    SearchFieldDataType.Collection(SearchFieldDataType.Single))
                {
                    IsSearchable = true,
                    VectorSearchDimensions = EmbeddingDimensions,
                    VectorSearchProfileName = VectorProfileName,
                },
            },
            VectorSearch = new VectorSearch
            {
                Profiles =
                {
                    new VectorSearchProfile(VectorProfileName, HnswAlgorithmName),
                },
                Algorithms =
                {
                    new HnswAlgorithmConfiguration(HnswAlgorithmName),
                },
            },
        };

        await _indexClient.CreateOrUpdateIndexAsync(index, cancellationToken: cancellationToken);
    }

    public Task UpsertGameAsync(
        Game game,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(game);

        return UpsertGamesAsync([game], cancellationToken);
    }

    public async Task UpsertGamesAsync(
        IEnumerable<Game> games,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(games);

        var gameList = games.ToList();
        if (gameList.Count == 0)
        {
            return;
        }

        var documents = new List<GameSearchDocument>(gameList.Count);

        foreach (var game in gameList)
        {
            var searchText = BuildSearchText(game);
            var embedding = await _embeddingService.CreateEmbeddingAsync(searchText, cancellationToken);

            documents.Add(new GameSearchDocument
            {
                Id = game.GameId.ToString(),
                GameId = game.GameId.ToString(),
                IgdbId = game.IgdbId,
                Title = game.Title,
                Summary = game.Summary,
                Genres = game.Genres,
                Platforms = game.Platforms,
                Developer = game.Developer,
                Publisher = game.Publisher,
                CoverImageUrl = game.CoverImageUrl,
                SearchText = searchText,
                ContentVector = embedding,
            });
        }

        await _searchClient.MergeOrUploadDocumentsAsync(
            documents,
            cancellationToken: cancellationToken);
    }

    private static string BuildSearchText(Game game)
    {
        var parts = new[]
        {
            game.Title,
            game.Summary,
            game.Genres,
            game.Platforms,
            game.Developer,
            game.Publisher,
        };

        return string.Join(
            " ",
            parts.Where(static part => !string.IsNullOrWhiteSpace(part)));
    }

    private sealed class GameSearchDocument
    {
        public string Id { get; set; } = string.Empty;

        public string GameId { get; set; } = string.Empty;

        public long? IgdbId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string? Summary { get; set; }

        public string? Genres { get; set; }

        public string? Platforms { get; set; }

        public string? Developer { get; set; }

        public string? Publisher { get; set; }

        public string? CoverImageUrl { get; set; }

        public string SearchText { get; set; } = string.Empty;

        public float[] ContentVector { get; set; } = [];
    }
}