namespace Backlogr.Api.DTOs.Reviews;

public sealed class CreateReviewRequestDto
{
    public Guid GameId { get; set; }

    public string Text { get; set; } = string.Empty;

    public bool HasSpoilers { get; set; }
}