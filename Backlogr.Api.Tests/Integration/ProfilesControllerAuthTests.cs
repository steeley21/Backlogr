using System.Net;
using FluentAssertions;

namespace Backlogr.Api.Tests.Integration;

public sealed class ProfilesControllerAuthTests : IClassFixture<BacklogrApiFactory>
{
    private readonly HttpClient _client;

    public ProfilesControllerAuthTests(BacklogrApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetPublicProfile_ShouldReturnUnauthorized_WhenNoTokenProvided()
    {
        var response = await _client.GetAsync("/api/profiles/some_user");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetPublicLibrary_ShouldReturnUnauthorized_WhenNoTokenProvided()
    {
        var response = await _client.GetAsync("/api/profiles/some_user/library");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
