using Backlogr.Api.DTOs.AI;
using Backlogr.Api.Services.Interfaces;

namespace Backlogr.Api.Services.Implementations;

public sealed class StubReviewAssistantService : IReviewAssistantService
{
    public Task<ReviewAssistantResponseDto> GenerateAsync(ReviewAssistantRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Prompt) && string.IsNullOrWhiteSpace(dto.ExistingText))
        {
            throw new ArgumentException("Prompt or existing text is required.");
        }

        var mode = dto.Mode.Trim().ToLowerInvariant();
        var prompt = dto.Prompt.Trim();
        var existing = dto.ExistingText?.Trim();

        var resultText = mode switch
        {
            "draft" =>
                $"Draft review based on your notes: {prompt}",

            "rewrite" =>
                $"Rewritten review: {(string.IsNullOrWhiteSpace(existing) ? prompt : existing)}",

            "shorten" =>
                $"Shortened version: {(string.IsNullOrWhiteSpace(existing) ? prompt : existing)}",

            "expand" =>
                $"Expanded version: {(string.IsNullOrWhiteSpace(existing) ? prompt : existing)}. This version adds a little more detail and flow.",

            "spoiler-safe-summary" =>
                $"Spoiler-safe summary: {prompt}",

            _ =>
                $"Assistant output: {(string.IsNullOrWhiteSpace(existing) ? prompt : existing)}"
        };

        return Task.FromResult(new ReviewAssistantResponseDto
        {
            Mode = dto.Mode,
            ResultText = resultText
        });
    }
}