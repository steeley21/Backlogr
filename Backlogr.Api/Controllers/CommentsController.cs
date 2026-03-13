using System.Security.Claims;
using Backlogr.Api.Common;
using Backlogr.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backlogr.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class CommentsController : ControllerBase
{
    private readonly IReviewInteractionService _reviewInteractionService;

    public CommentsController(IReviewInteractionService reviewInteractionService)
    {
        _reviewInteractionService = reviewInteractionService;
    }

    [HttpDelete("{reviewCommentId:guid}")]
    public async Task<IActionResult> DeleteComment(Guid reviewCommentId)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        try
        {
            var deleted = await _reviewInteractionService.DeleteCommentAsync(
                userId.Value,
                reviewCommentId,
                User.IsInRole(RoleNames.Admin));

            if (!deleted)
            {
                return NotFound();
            }

            return NoContent();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
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