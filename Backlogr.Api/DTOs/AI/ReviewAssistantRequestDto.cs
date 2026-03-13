namespace Backlogr.Api.DTOs.AI;

public sealed class ReviewAssistantRequestDto
{
    public string Mode { get; set; } = "draft";

    public string Prompt { get; set; } = string.Empty;

    public string? ExistingText { get; set; }
}