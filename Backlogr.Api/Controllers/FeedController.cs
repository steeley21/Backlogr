using System.Security.Claims;
using Backlogr.Api.DTOs.Feed;
using Backlogr.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backlogr.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class FeedController : ControllerBase
{
    private readonly IFeedService _feedService;

    public FeedController(IFeedService feedService)
    {
        _feedService = feedService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<FeedItemResponseDto>>> GetFeed(
        [FromQuery] int take = 25,
        [FromQuery] string scope = "for-you")
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        if (!TryParseFeedScope(scope, out var feedScope))
        {
            return BadRequest("Invalid feed scope. Use 'for-you' or 'following'.");
        }

        var items = await _feedService.GetFeedAsync(userId.Value, feedScope, take);
        return Ok(items);
    }

    private static bool TryParseFeedScope(string? value, out FeedScope scope)
    {
        switch (value?.Trim().ToLowerInvariant())
        {
            case "for-you":
                scope = FeedScope.ForYou;
                return true;
            case "following":
                scope = FeedScope.Following;
                return true;
            default:
                scope = FeedScope.ForYou;
                return false;
        }
    }

    private Guid? GetCurrentUserId()
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (Guid.TryParse(userIdValue, out var userId))
        {
            return userId;
        }

        return null;
    }
}