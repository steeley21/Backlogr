using System;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Backlogr.Api.Options;
using Backlogr.Api.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Backlogr.Api.Services.Implementations;

public sealed class TwitchTokenService : ITwitchTokenService
{
    private const string CacheKey = "igdb:twitch-app-access-token";

    private static readonly JsonSerializerOptions JsonSerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true,
    };

    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _memoryCache;
    private readonly IgdbOptions _igdbOptions;
    private readonly ILogger<TwitchTokenService> _logger;

    public TwitchTokenService(
        HttpClient httpClient,
        IMemoryCache memoryCache,
        IOptions<IgdbOptions> igdbOptions,
        ILogger<TwitchTokenService> logger)
    {
        _httpClient = httpClient;
        _memoryCache = memoryCache;
        _igdbOptions = igdbOptions.Value;
        _logger = logger;
    }

    public async Task<string> GetAppAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        if (_memoryCache.TryGetValue(CacheKey, out string? cachedToken) &&
            !string.IsNullOrWhiteSpace(cachedToken))
        {
            return cachedToken;
        }

        var requestUri =
            $"{_igdbOptions.TokenUrl}" +
            $"?client_id={Uri.EscapeDataString(_igdbOptions.ClientId)}" +
            $"&client_secret={Uri.EscapeDataString(_igdbOptions.ClientSecret)}" +
            "&grant_type=client_credentials";

        using var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
        using var response = await _httpClient.SendAsync(request, cancellationToken);

        response.EnsureSuccessStatusCode();

        await using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);

        var tokenResponse = await JsonSerializer.DeserializeAsync<TwitchTokenResponse>(
            responseStream,
            JsonSerializerOptions,
            cancellationToken);

        if (tokenResponse is null || string.IsNullOrWhiteSpace(tokenResponse.AccessToken))
        {
            throw new InvalidOperationException("Twitch token response did not include an access token.");
        }

        var cacheSeconds = Math.Max(60, tokenResponse.ExpiresIn - 300);

        _memoryCache.Set(
            CacheKey,
            tokenResponse.AccessToken,
            TimeSpan.FromSeconds(cacheSeconds));

        _logger.LogInformation("Fetched and cached a Twitch app access token for IGDB.");

        return tokenResponse.AccessToken;
    }

    private sealed class TwitchTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonPropertyName("token_type")]
        public string TokenType { get; set; } = string.Empty;
    }
}