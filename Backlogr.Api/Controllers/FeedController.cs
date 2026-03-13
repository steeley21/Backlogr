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
    public async Task<ActionResult<IReadOnlyList<FeedItemResponseDto>>> GetFeed([FromQuery] int take = 25)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var items = await _feedService.GetFeedAsync(userId.Value, take);
        return Ok(items);
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