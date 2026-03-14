using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Backlogr.Api.Data;
using Backlogr.Api.DTOs.Igdb;
using Backlogr.Api.Models.Entities;
using Backlogr.Api.Options;
using Backlogr.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Backlogr.Api.Services.Implementations;

public sealed class IgdbService : IIgdbService
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true,
    };

    private readonly HttpClient _httpClient;
    private readonly ITwitchTokenService _twitchTokenService;
    private readonly ApplicationDbContext _dbContext;
    private readonly IgdbOptions _igdbOptions;
    private readonly ILogger<IgdbService> _logger;

    public IgdbService(
        HttpClient httpClient,
        ITwitchTokenService twitchTokenService,
        ApplicationDbContext dbContext,
        IOptions<IgdbOptions> igdbOptions,
        ILogger<IgdbService> logger)
    {
        _httpClient = httpClient;
        _twitchTokenService = twitchTokenService;
        _dbContext = dbContext;
        _igdbOptions = igdbOptions.Value;
        _logger = logger;
    }

    public async Task<IReadOnlyList<IgdbGameSearchResultDto>> SearchGamesAsync(string query, int take = 10)
    {
        take = Math.Clamp(take, 1, 50);

        var trimmedQuery = query.Trim();

        if (string.IsNullOrWhiteSpace(trimmedQuery))
        {
            return [];
        }

        var searchBody =
            $"{BuildFieldsClause()}" +
            $"search \"{EscapeApicalypseString(trimmedQuery)}\";" +
            $" limit {take};";

        var games = await SendGamesQueryAsync(searchBody, CancellationToken.None);

        return games
            .Select(MapToSearchResultDto)
            .ToList();
    }

    public async Task<ImportedGameResponseDto> ImportGameAsync(long igdbId)
    {
        var igdbGame = await GetGameByIgdbIdAsync(igdbId, CancellationToken.None);

        var existingGame = await _dbContext.Games
            .SingleOrDefaultAsync(g => g.IgdbId == igdbId);

        if (existingGame is null)
        {
            existingGame = new Game
            {
                GameId = Guid.NewGuid(),
                IgdbId = igdbGame.Id,
                CreatedAt = DateTime.UtcNow,
            };

            ApplyIgdbValues(existingGame, igdbGame);

            _dbContext.Games.Add(existingGame);
        }
        else
        {
            ApplyIgdbValues(existingGame, igdbGame);
        }

        await _dbContext.SaveChangesAsync();

        return new ImportedGameResponseDto
        {
            GameId = existingGame.GameId,
            IgdbId = existingGame.IgdbId ?? igdbId,
            Title = existingGame.Title,
            Slug = existingGame.Slug,
            Summary = existingGame.Summary,
            CoverImageUrl = existingGame.CoverImageUrl,
            ReleaseDate = existingGame.ReleaseDate,
            Developer = existingGame.Developer,
            Publisher = existingGame.Publisher,
            Platforms = existingGame.Platforms,
            Genres = existingGame.Genres,
            UpdatedAt = existingGame.UpdatedAt,
        };
    }

    private async Task<IReadOnlyList<IgdbGameApiModel>> SendGamesQueryAsync(
        string requestBody,
        CancellationToken cancellationToken)
    {
        var accessToken = await _twitchTokenService.GetAppAccessTokenAsync(cancellationToken);

        using var request = new HttpRequestMessage(HttpMethod.Post, "games");
        request.Headers.Add("Client-ID", _igdbOptions.ClientId);
        request.Headers.Add("Authorization", $"Bearer {accessToken}");
        request.Content = new StringContent(requestBody, Encoding.UTF8, "text/plain");

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        response.EnsureSuccessStatusCode();

        await using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);

        var games = await JsonSerializer.DeserializeAsync<List<IgdbGameApiModel>>(
            responseStream,
            JsonSerializerOptions,
            cancellationToken);

        return games ?? [];
    }

    private async Task<IgdbGameApiModel> GetGameByIgdbIdAsync(long igdbId, CancellationToken cancellationToken)
    {
        var requestBody =
            $"{BuildFieldsClause()}" +
            $"where id = {igdbId};" +
            " limit 1;";

        var games = await SendGamesQueryAsync(requestBody, cancellationToken);

        var game = games.SingleOrDefault();

        if (game is null)
        {
            throw new KeyNotFoundException("IGDB game was not found.");
        }

        return game;
    }

    private static string BuildFieldsClause()
    {
        return "fields " +
               "id," +
               "name," +
               "slug," +
               "summary," +
               "first_release_date," +
               "cover.url," +
               "platforms.name," +
               "genres.name," +
               "involved_companies.company.name," +
               "involved_companies.developer," +
               "involved_companies.publisher;";
    }

    private static string EscapeApicalypseString(string value)
    {
        return value
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("\"", "\\\"", StringComparison.Ordinal);
    }

    private static IgdbGameSearchResultDto MapToSearchResultDto(IgdbGameApiModel game)
    {
        return new IgdbGameSearchResultDto
        {
            IgdbId = game.Id,
            Title = game.Name,
            Summary = game.Summary,
            CoverImageUrl = NormalizeCoverUrl(game.Cover?.Url),
            ReleaseDate = ToUtcDateTime(game.FirstReleaseDate),
            Developer = JoinNames(
                game.InvolvedCompanies?
                    .Where(company => company.Developer)
                    .Select(company => company.Company?.Name)),
            Publisher = JoinNames(
                game.InvolvedCompanies?
                    .Where(company => company.Publisher)
                    .Select(company => company.Company?.Name)),
            Platforms = JoinNames(game.Platforms?.Select(platform => platform.Name)),
            Genres = JoinNames(game.Genres?.Select(genre => genre.Name)),
        };
    }

    private static void ApplyIgdbValues(Game target, IgdbGameApiModel source)
    {
        target.IgdbId = source.Id;
        target.Title = source.Name;
        target.Slug = !string.IsNullOrWhiteSpace(source.Slug)
            ? source.Slug
            : ToSlug(source.Name);
        target.Summary = source.Summary;
        target.CoverImageUrl = NormalizeCoverUrl(source.Cover?.Url);
        target.ReleaseDate = ToUtcDateTime(source.FirstReleaseDate);
        target.Developer = JoinNames(
            source.InvolvedCompanies?
                .Where(company => company.Developer)
                .Select(company => company.Company?.Name));
        target.Publisher = JoinNames(
            source.InvolvedCompanies?
                .Where(company => company.Publisher)
                .Select(company => company.Company?.Name));
        target.Platforms = JoinNames(source.Platforms?.Select(platform => platform.Name));
        target.Genres = JoinNames(source.Genres?.Select(genre => genre.Name));
        target.UpdatedAt = DateTime.UtcNow;
    }

    private static string? NormalizeCoverUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return null;
        }

        var normalized = url.StartsWith("//", StringComparison.Ordinal)
            ? $"https:{url}"
            : url;

        return normalized.Replace("t_thumb", "t_cover_big", StringComparison.OrdinalIgnoreCase);
    }

    private static DateTime? ToUtcDateTime(long? unixSeconds)
    {
        if (unixSeconds is null)
        {
            return null;
        }

        return DateTimeOffset.FromUnixTimeSeconds(unixSeconds.Value).UtcDateTime;
    }

    private static string? JoinNames(IEnumerable<string?>? values)
    {
        var names = values?
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value!.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return names is { Length: > 0 }
            ? string.Join(", ", names)
            : null;
    }

    private static string ToSlug(string title)
    {
        return title
            .Trim()
            .ToLowerInvariant()
            .Replace("'", string.Empty, StringComparison.Ordinal)
            .Replace(" ", "-", StringComparison.Ordinal);
    }

    private sealed class IgdbGameApiModel
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("slug")]
        public string? Slug { get; set; }

        [JsonPropertyName("summary")]
        public string? Summary { get; set; }

        [JsonPropertyName("first_release_date")]
        public long? FirstReleaseDate { get; set; }

        [JsonPropertyName("cover")]
        public IgdbCoverApiModel? Cover { get; set; }

        [JsonPropertyName("platforms")]
        public List<IgdbNamedItemApiModel>? Platforms { get; set; }

        [JsonPropertyName("genres")]
        public List<IgdbNamedItemApiModel>? Genres { get; set; }

        [JsonPropertyName("involved_companies")]
        public List<IgdbInvolvedCompanyApiModel>? InvolvedCompanies { get; set; }
    }

    private sealed class IgdbCoverApiModel
    {
        [JsonPropertyName("url")]
        public string? Url { get; set; }
    }

    private sealed class IgdbNamedItemApiModel
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }

    private sealed class IgdbInvolvedCompanyApiModel
    {
        [JsonPropertyName("developer")]
        public bool Developer { get; set; }

        [JsonPropertyName("publisher")]
        public bool Publisher { get; set; }

        [JsonPropertyName("company")]
        public IgdbCompanyApiModel? Company { get; set; }
    }

    private sealed class IgdbCompanyApiModel
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }
}