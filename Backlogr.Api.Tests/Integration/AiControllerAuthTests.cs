using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace Backlogr.Api.Tests.Integration;

public sealed class AiControllerAuthTests : IClassFixture<BacklogrApiFactory>
{
    private readonly HttpClient _client;

    public AiControllerAuthTests(BacklogrApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Recommendations_ShouldReturnUnauthorized_WhenNoTokenProvided()
    {
        var response = await _client.PostAsJsonAsync("/api/ai/recommendations", new
        {
            take = 5
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ReviewAssistant_ShouldReturnUnauthorized_WhenNoTokenProvided()
    {
        var response = await _client.PostAsJsonAsync("/api/ai/review-assistant", new
        {
            mode = "draft",
            prompt = "great atmosphere"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SemanticSearch_ShouldReturnUnauthorized_WhenNoTokenProvided()
    {
        var response = await _client.GetAsync("/api/ai/semantic-search?query=cozy+crafting");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}