namespace Backlogr.Api.DTOs.Reviews;

public sealed class UpdateReviewRequestDto
{
    public string Text { get; set; } = string.Empty;

    public bool HasSpoilers { get; set; }
}