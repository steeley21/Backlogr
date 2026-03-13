using System.Net;
using FluentAssertions;

namespace Backlogr.Api.Tests.Integration;

public sealed class FollowsControllerAuthTests : IClassFixture<BacklogrApiFactory>
{
    private readonly HttpClient _client;

    public FollowsControllerAuthTests(BacklogrApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task FollowUser_ShouldReturnUnauthorized_WhenNoTokenProvided()
    {
        var response = await _client.PostAsync($"/api/follows/{Guid.NewGuid()}", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UnfollowUser_ShouldReturnUnauthorized_WhenNoTokenProvided()
    {
        var response = await _client.DeleteAsync($"/api/follows/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}