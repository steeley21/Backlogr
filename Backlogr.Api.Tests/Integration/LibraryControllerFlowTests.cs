using System.Net;
using System.Net.Http.Json;
using Backlogr.Api.DTOs.Library;
using Backlogr.Api.Models.Enums;
using FluentAssertions;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Backlogr.Api.Tests.Integration;

public sealed class LibraryControllerFlowTests : IClassFixture<AuthenticatedBacklogrApiFactory>
{
    private static readonly Guid TestGameId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid TestUserId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    private readonly HttpClient _client;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public LibraryControllerFlowTests(AuthenticatedBacklogrApiFactory factory)
    {
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Add("X-Test-UserId", TestUserId.ToString());
    }

    [Fact]
    public async Task LibraryFlow_ShouldCreateReadAndDeleteLog_ForAuthenticatedUser()
    {
        var upsertRequest = new UpsertLibraryLogRequestDto
        {
            GameId = TestGameId,
            Status = LibraryStatus.Backlog,
            Rating = 4.5m,
            Platform = "PC",
            Hours = 12.5m,
            StartedAt = new DateTime(2026, 3, 13, 0, 0, 0, DateTimeKind.Utc),
            FinishedAt = null,
            Notes = "Integration test log"
        };

        var postResponse = await _client.PostAsJsonAsync("/api/library", upsertRequest);
        postResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var createdLog = await postResponse.Content.ReadFromJsonAsync<LibraryLogResponseDto>(JsonOptions); createdLog.Should().NotBeNull();
        createdLog!.GameId.Should().Be(TestGameId);
        createdLog.GameTitle.Should().Be("Test Game");
        createdLog.Status.Should().Be(LibraryStatus.Backlog);
        createdLog.Rating.Should().Be(4.5m);

        var getResponse = await _client.GetAsync("/api/library/me");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var library = await getResponse.Content.ReadFromJsonAsync<List<LibraryLogResponseDto>>(JsonOptions); library.Should().NotBeNull();
        library!.Should().ContainSingle();
        library[0].GameId.Should().Be(TestGameId);
        library[0].GameTitle.Should().Be("Test Game");

        var deleteResponse = await _client.DeleteAsync($"/api/library/{TestGameId}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getAfterDeleteResponse = await _client.GetAsync("/api/library/me");
        getAfterDeleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var libraryAfterDelete =
            await getAfterDeleteResponse.Content.ReadFromJsonAsync<List<LibraryLogResponseDto>>(JsonOptions);

        libraryAfterDelete.Should().NotBeNull();
        libraryAfterDelete.Should().BeEmpty();
    }

    [Fact]
    public async Task UpsertLibraryLog_ShouldReturnBadRequest_WhenRatingIsInvalid()
    {
        var invalidRequest = new UpsertLibraryLogRequestDto
        {
            GameId = TestGameId,
            Status = LibraryStatus.Backlog,
            Rating = 4.3m
        };

        var response = await _client.PostAsJsonAsync("/api/library", invalidRequest);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var message = await response.Content.ReadAsStringAsync();
        message.Should().Contain("0.5 increments");
    }
}