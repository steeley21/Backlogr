using System.Net;
using FluentAssertions;

namespace Backlogr.Api.Tests.Integration;

public sealed class FeedControllerAuthTests : IClassFixture<BacklogrApiFactory>
{
    private readonly HttpClient _client;

    public FeedControllerAuthTests(BacklogrApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetFeed_ShouldReturnUnauthorized_WhenNoTokenProvided()
    {
        var response = await _client.GetAsync("/api/feed");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}