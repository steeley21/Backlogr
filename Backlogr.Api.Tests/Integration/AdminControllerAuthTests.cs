using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace Backlogr.Api.Tests.Integration;

public sealed class AdminControllerAuthTests : IClassFixture<BacklogrApiFactory>
{
    private readonly HttpClient _client;

    public AdminControllerAuthTests(BacklogrApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetUsers_ShouldReturnUnauthorized_WhenNoAuthProvided()
    {
        var response = await _client.GetAsync("/api/admin/users");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateUser_ShouldReturnUnauthorized_WhenNoAuthProvided()
    {
        var response = await _client.PostAsJsonAsync("/api/admin/users", new
        {
            userName = "newuser",
            displayName = "New User",
            email = "newuser@example.com",
            password = "Password1",
            role = "User"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateUserRole_ShouldReturnUnauthorized_WhenNoAuthProvided()
    {
        var response = await _client.PutAsJsonAsync($"/api/admin/users/{Guid.NewGuid()}/role", new
        {
            role = "Admin"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
