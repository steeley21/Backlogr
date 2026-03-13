using Backlogr.Api.DTOs.Auth;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Backlogr.Api.Common;
using Backlogr.Api.Data;
using Backlogr.Api.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Backlogr.Api.Tests.Integration;

public sealed class AuthControllerIntegrationTests : IClassFixture<AuthenticatedBacklogrApiFactory>
{
    private readonly AuthenticatedBacklogrApiFactory _factory;

    public AuthControllerIntegrationTests(AuthenticatedBacklogrApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Register_ShouldCreateUser_ReturnToken_AndAssignUserRole()
    {
        using var client = _factory.CreateClient();

        var suffix = Guid.NewGuid().ToString("N")[..8];
        var request = new RegisterRequestDto
        {
            UserName = $"register_{suffix}",
            DisplayName = "Register Test User",
            Email = $"register_{suffix}@example.com",
            Password = "Password1"
        };

        var response = await client.PostAsJsonAsync("/api/auth/register", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        body.Should().NotBeNull();
        body!.Token.Should().NotBeNullOrWhiteSpace();
        body.UserName.Should().Be(request.UserName);
        body.DisplayName.Should().Be(request.DisplayName);
        body.Email.Should().Be(request.Email);
        body.Roles.Should().Contain("User");
    }

    [Fact]
    public async Task Login_ShouldAllowUserName()
    {
        using var client = _factory.CreateClient();

        var suffix = Guid.NewGuid().ToString("N")[..8];
        var userName = $"loginuser_{suffix}";
        var email = $"{userName}@example.com";
        const string password = "Password1";

        await RegisterAsync(client, userName, email, password);

        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequestDto
        {
            EmailOrUserName = userName,
            Password = password
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        body.Should().NotBeNull();
        body!.Token.Should().NotBeNullOrWhiteSpace();
        body.UserName.Should().Be(userName);
        body.Email.Should().Be(email);
    }

    [Fact]
    public async Task Login_ShouldAllowEmail()
    {
        using var client = _factory.CreateClient();

        var suffix = Guid.NewGuid().ToString("N")[..8];
        var userName = $"loginemail_{suffix}";
        var email = $"{userName}@example.com";
        const string password = "Password1";

        await RegisterAsync(client, userName, email, password);

        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequestDto
        {
            EmailOrUserName = email,
            Password = password
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        body.Should().NotBeNull();
        body!.Token.Should().NotBeNullOrWhiteSpace();
        body.UserName.Should().Be(userName);
    }

    [Fact]
    public async Task Me_ShouldReturnCurrentUser_WhenAuthenticated()
    {
        using var client = _factory.CreateClient();

        var userId = Guid.Parse("33333333-3333-3333-3333-333333333333");

        await SeedUserAsync(
            userId,
            userName: "me_test_user",
            email: "me_test_user@example.com",
            displayName: "Me Test User");

        client.DefaultRequestHeaders.Add("X-Test-UserId", userId.ToString());

        var meResponse = await client.GetAsync("/api/auth/me");

        meResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var me = await meResponse.Content.ReadFromJsonAsync<MeResponseDto>();
        me.Should().NotBeNull();
        me!.UserId.Should().Be(userId);
        me.UserName.Should().Be("me_test_user");
        me.Email.Should().Be("me_test_user@example.com");
        me.DisplayName.Should().Be("Me Test User");
        me.Roles.Should().Contain("User");
    }

    [Fact]
    public async Task Register_ShouldReturnBadRequest_WhenEmailAlreadyExists()
    {
        using var client = _factory.CreateClient();

        var suffix = Guid.NewGuid().ToString("N")[..8];
        var email = $"duplicate_{suffix}@example.com";

        await RegisterAsync(client, $"first_{suffix}", email, "Password1");

        var response = await client.PostAsJsonAsync("/api/auth/register", new RegisterRequestDto
        {
            UserName = $"second_{suffix}",
            DisplayName = "Duplicate Email User",
            Email = email,
            Password = "Password1"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_ShouldReturnUnauthorized_WhenPasswordIsWrong()
    {
        using var client = _factory.CreateClient();

        var suffix = Guid.NewGuid().ToString("N")[..8];
        var userName = $"wrongpass_{suffix}";
        var email = $"{userName}@example.com";

        await RegisterAsync(client, userName, email, "Password1");

        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequestDto
        {
            EmailOrUserName = userName,
            Password = "WrongPassword1"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private static async Task RegisterAsync(
        HttpClient client,
        string userName,
        string email,
        string password)
    {
        var displayName = userName.StartsWith("me_")
            ? "Me Test User"
            : "Integration Test User";

        var response = await client.PostAsJsonAsync("/api/auth/register", new RegisterRequestDto
        {
            UserName = userName,
            DisplayName = displayName,
            Email = email,
            Password = password
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private async Task SeedUserAsync(Guid userId, string userName, string email, string displayName)
    {
        using var scope = _factory.Services.CreateScope();

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

        if (!await roleManager.RoleExistsAsync(RoleNames.User))
        {
            var createRoleResult = await roleManager.CreateAsync(new ApplicationRole
            {
                Name = RoleNames.User
            });

            createRoleResult.Succeeded.Should().BeTrue();
        }

        var existingUser = await userManager.FindByIdAsync(userId.ToString());
        if (existingUser is not null)
        {
            return;
        }

        var user = new ApplicationUser
        {
            Id = userId,
            UserName = userName,
            Email = email,
            DisplayName = displayName,
            CreatedAt = DateTime.UtcNow
        };

        var createUserResult = await userManager.CreateAsync(user, "Password1");
        createUserResult.Succeeded.Should().BeTrue();

        var addToRoleResult = await userManager.AddToRoleAsync(user, RoleNames.User);
        addToRoleResult.Succeeded.Should().BeTrue();
    }
}