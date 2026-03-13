using FluentAssertions;
using System.Net;
using System.Net.Http.Json;

namespace Backlogr.Api.Tests.Integration;

public sealed class LibraryControllerAuthTests : IClassFixture<BacklogrApiFactory>
{
    private readonly HttpClient _client;

    public LibraryControllerAuthTests(BacklogrApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetMyLibrary_ShouldReturnUnauthorized_WhenNoTokenProvided()
    {
        var response = await _client.GetAsync("/api/library/me");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpsertLibraryLog_ShouldReturnUnauthorized_WhenNoTokenProvided()
    {
        var content = JsonContent.Create(new
        {
            gameId = Guid.NewGuid(),
            status = "Backlog",
            rating = 4.5,
            platform = "PC",
            hours = 10.0,
            startedAt = DateTime.UtcNow,
            finishedAt = (DateTime?)null,
            notes = "Unauthorized test"
        });

        var response = await _client.PostAsync("/api/library", content);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteLibraryLog_ShouldReturnUnauthorized_WhenNoTokenProvided()
    {
        var response = await _client.DeleteAsync($"/api/library/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}