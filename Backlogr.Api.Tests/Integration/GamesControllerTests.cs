using System.Net;
using System.Net.Http.Json;
using Backlogr.Api.DTOs.Games;
using Backlogr.Api.Data;
using FluentAssertions;

namespace Backlogr.Api.Tests.Integration;

public sealed class GamesControllerTests : IClassFixture<BacklogrApiFactory>
{
    private readonly HttpClient _client;

    public GamesControllerTests(BacklogrApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetGames_ShouldReturnSeededTestGame()
    {
        var response = await _client.GetAsync("/api/games");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var games = await response.Content.ReadFromJsonAsync<List<GameSummaryResponseDto>>();
        games.Should().NotBeNull();
        games!.Should().Contain(g => g.GameId == DevelopmentDataSeeder.TestGameId);
    }

    [Fact]
    public async Task GetGames_ShouldFilterByQuery()
    {
        var response = await _client.GetAsync("/api/games?query=Test");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var games = await response.Content.ReadFromJsonAsync<List<GameSummaryResponseDto>>();
        games.Should().NotBeNull();
        games!.Should().ContainSingle(g => g.GameId == DevelopmentDataSeeder.TestGameId);
    }

    [Fact]
    public async Task GetGameById_ShouldReturnGame_WhenFound()
    {
        var response = await _client.GetAsync($"/api/games/{DevelopmentDataSeeder.TestGameId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var game = await response.Content.ReadFromJsonAsync<GameDetailResponseDto>();
        game.Should().NotBeNull();
        game!.GameId.Should().Be(DevelopmentDataSeeder.TestGameId);
        game.Title.Should().Be("Test Game");
    }

    [Fact]
    public async Task GetGameById_ShouldReturnNotFound_WhenMissing()
    {
        var response = await _client.GetAsync($"/api/games/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}