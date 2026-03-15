using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace Backlogr.Api.Tests.Integration;

public sealed class ReviewInteractionsAuthTests : IClassFixture<BacklogrApiFactory>
{
    private readonly HttpClient _client;

    public ReviewInteractionsAuthTests(BacklogrApiFactory factory)
    {
        _client = factory.CreateClient();
    }


    [Fact]
    public async Task GetComments_ShouldReturnUnauthorized_WhenNoTokenProvided()
    {
        var response = await _client.GetAsync($"/api/reviews/{Guid.NewGuid()}/comments");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task LikeReview_ShouldReturnUnauthorized_WhenNoTokenProvided()
    {
        var response = await _client.PostAsync($"/api/reviews/{Guid.NewGuid()}/like", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UnlikeReview_ShouldReturnUnauthorized_WhenNoTokenProvided()
    {
        var response = await _client.DeleteAsync($"/api/reviews/{Guid.NewGuid()}/like");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AddComment_ShouldReturnUnauthorized_WhenNoTokenProvided()
    {
        var response = await _client.PostAsJsonAsync(
            $"/api/reviews/{Guid.NewGuid()}/comments",
            new { text = "Unauthorized comment" });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteComment_ShouldReturnUnauthorized_WhenNoTokenProvided()
    {
        var response = await _client.DeleteAsync($"/api/comments/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}