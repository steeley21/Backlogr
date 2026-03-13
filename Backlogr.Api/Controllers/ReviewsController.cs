using System.Security.Claims;
using Backlogr.Api.DTOs.Reviews;
using Backlogr.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backlogr.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class ReviewsController : ControllerBase
{
    private readonly IReviewService _reviewService;
    private readonly IReviewInteractionService _reviewInteractionService;

    public ReviewsController(
        IReviewService reviewService,
        IReviewInteractionService reviewInteractionService)
    {
        _reviewService = reviewService;
        _reviewInteractionService = reviewInteractionService;
    }

    [HttpPost]
    public async Task<ActionResult<ReviewResponseDto>> CreateReview(CreateReviewRequestDto dto)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        try
        {
            var result = await _reviewService.CreateReviewAsync(userId.Value, dto);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPut("{reviewId:guid}")]
    public async Task<ActionResult<ReviewResponseDto>> UpdateReview(
        Guid reviewId,
        UpdateReviewRequestDto dto)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        try
        {
            var result = await _reviewService.UpdateReviewAsync(userId.Value, reviewId, dto);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpDelete("{reviewId:guid}")]
    public async Task<IActionResult> DeleteReview(Guid reviewId)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        try
        {
            var deleted = await _reviewService.DeleteReviewAsync(userId.Value, reviewId);

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


    [HttpPost("{reviewId:guid}/like")]
    public async Task<IActionResult> LikeReview(Guid reviewId)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        try
        {
            await _reviewInteractionService.AddLikeAsync(userId.Value, reviewId);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpDelete("{reviewId:guid}/like")]
    public async Task<IActionResult> UnlikeReview(Guid reviewId)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        await _reviewInteractionService.RemoveLikeAsync(userId.Value, reviewId);
        return NoContent();
    }

    [HttpPost("{reviewId:guid}/comments")]
    public async Task<ActionResult<ReviewCommentResponseDto>> AddComment(
        Guid reviewId,
        CreateReviewCommentRequestDto dto)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        try
        {
            var result = await _reviewInteractionService.AddCommentAsync(userId.Value, reviewId, dto);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }
}