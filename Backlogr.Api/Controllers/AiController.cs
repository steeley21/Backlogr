using System.Security.Claims;
using Backlogr.Api.DTOs.AI;
using Backlogr.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backlogr.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class AiController : ControllerBase
{
    private readonly IRecommendationService _recommendationService;
    private readonly IReviewAssistantService _reviewAssistantService;
    private readonly ISemanticSearchService _semanticSearchService;

    public AiController(
        IRecommendationService recommendationService,
        IReviewAssistantService reviewAssistantService,
        ISemanticSearchService semanticSearchService)
    {
        _recommendationService = recommendationService;
        _reviewAssistantService = reviewAssistantService;
        _semanticSearchService = semanticSearchService;
    }

    [HttpPost("recommendations")]
    public async Task<ActionResult<RecommendationResponseDto>> GetRecommendations(
        RecommendationRequestDto dto)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await _recommendationService.GetRecommendationsAsync(userId.Value, dto.Take);
        return Ok(result);
    }

    [HttpPost("review-assistant")]
    public async Task<ActionResult<ReviewAssistantResponseDto>> ReviewAssistant(
        ReviewAssistantRequestDto dto)
    {
        try
        {
            var result = await _reviewAssistantService.GenerateAsync(dto);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("semantic-search")]
    public async Task<ActionResult<IReadOnlyList<SemanticSearchResultDto>>> SemanticSearch(
        [FromQuery] string query,
        [FromQuery] int take = 10)
    {
        try
        {
            var result = await _semanticSearchService.SearchAsync(query, take);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
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