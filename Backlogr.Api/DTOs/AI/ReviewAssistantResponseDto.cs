namespace Backlogr.Api.DTOs.AI;

public sealed class ReviewAssistantResponseDto
{
    public string Mode { get; set; } = string.Empty;

    public string ResultText { get; set; } = string.Empty;
}