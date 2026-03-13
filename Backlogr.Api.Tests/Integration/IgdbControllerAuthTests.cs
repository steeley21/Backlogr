using System.Net;
using FluentAssertions;

namespace Backlogr.Api.Tests.Integration;

public sealed class IgdbControllerAuthTests : IClassFixture<BacklogrApiFactory>
{
    private readonly HttpClient _client;

    public IgdbControllerAuthTests(BacklogrApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task SearchGames_ShouldReturnUnauthorized_WhenNoAuthProvided()
    {
        var response = await _client.GetAsync("/api/igdb/search?query=hades");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ImportGame_ShouldReturnUnauthorized_WhenNoAuthProvided()
    {
        var response = await _client.PostAsync("/api/igdb/import/1001", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}