using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace Backlogr.Api.Tests.Integration;

public sealed class ReviewsControllerAuthTests : IClassFixture<BacklogrApiFactory>
{
    private readonly HttpClient _client;

    public ReviewsControllerAuthTests(BacklogrApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateReview_ShouldReturnUnauthorized_WhenNoTokenProvided()
    {
        var content = JsonContent.Create(new
        {
            gameId = Guid.NewGuid(),
            text = "Unauthorized review attempt",
            hasSpoilers = false
        });

        var response = await _client.PostAsync("/api/reviews", content);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateReview_ShouldReturnUnauthorized_WhenNoTokenProvided()
    {
        var content = JsonContent.Create(new
        {
            text = "Unauthorized update attempt",
            hasSpoilers = true
        });

        var response = await _client.PutAsync($"/api/reviews/{Guid.NewGuid()}", content);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteReview_ShouldReturnUnauthorized_WhenNoTokenProvided()
    {
        var response = await _client.DeleteAsync($"/api/reviews/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}