using System.Text;
using System.Text.Json;
using Backlogr.Api.DTOs.AI;
using Backlogr.Api.Options;
using Backlogr.Api.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace Backlogr.Api.Services.Implementations;

public sealed class OpenAiReviewAssistantService : IReviewAssistantService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;
    private readonly OpenAiOptions _openAiOptions;

    public OpenAiReviewAssistantService(
        HttpClient httpClient,
        IOptions<OpenAiOptions> openAiOptions)
    {
        _httpClient = httpClient;
        _openAiOptions = openAiOptions.Value;
    }

    public async Task<ReviewAssistantResponseDto> GenerateAsync(ReviewAssistantRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Prompt) && string.IsNullOrWhiteSpace(dto.ExistingText))
        {
            throw new ArgumentException("Prompt or existing text is required.");
        }

        var normalizedMode = NormalizeMode(dto.Mode);
        var instructions = BuildInstructions(normalizedMode);
        var input = BuildInput(dto, normalizedMode);

        var requestBody = new
        {
            model = _openAiOptions.ChatModel,
            reasoning = new
            {
                effort = "low"
            },
            instructions,
            input,
            text = new
            {
                format = new
                {
                    type = "text"
                }
            }
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, "responses")
        {
            Content = new StringContent(
                JsonSerializer.Serialize(requestBody, JsonOptions),
                Encoding.UTF8,
                "application/json")
        };

        using var response = await _httpClient.SendAsync(request);
        var responseText = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"OpenAI review assistant request failed with status {(int)response.StatusCode}: {responseText}");
        }

        using var document = JsonDocument.Parse(responseText);

        var resultText = TryGetOutputText(document.RootElement);

        if (string.IsNullOrWhiteSpace(resultText))
        {
            throw new InvalidOperationException(
                $"OpenAI response did not include usable output text. Raw response: {responseText}");
        }

        return new ReviewAssistantResponseDto
        {
            Mode = normalizedMode,
            ResultText = resultText.Trim()
        };
    }

    private static string? TryGetOutputText(JsonElement root)
    {
        if (root.TryGetProperty("output_text", out var outputTextElement) &&
            outputTextElement.ValueKind == JsonValueKind.String)
        {
            var topLevelText = outputTextElement.GetString();
            if (!string.IsNullOrWhiteSpace(topLevelText))
            {
                return topLevelText;
            }
        }

        if (root.TryGetProperty("output", out var outputElement) &&
            outputElement.ValueKind == JsonValueKind.Array)
        {
            foreach (var outputItem in outputElement.EnumerateArray())
            {
                if (!outputItem.TryGetProperty("content", out var contentElement) ||
                    contentElement.ValueKind != JsonValueKind.Array)
                {
                    continue;
                }

                foreach (var contentItem in contentElement.EnumerateArray())
                {
                    if (!contentItem.TryGetProperty("type", out var typeElement) ||
                        typeElement.ValueKind != JsonValueKind.String ||
                        !string.Equals(typeElement.GetString(), "output_text", StringComparison.Ordinal))
                    {
                        continue;
                    }

                    if (contentItem.TryGetProperty("text", out var textElement) &&
                        textElement.ValueKind == JsonValueKind.String)
                    {
                        var nestedText = textElement.GetString();
                        if (!string.IsNullOrWhiteSpace(nestedText))
                        {
                            return nestedText;
                        }
                    }
                }
            }
        }

        return null;
    }

    private static string NormalizeMode(string? mode)
    {
        var normalized = mode?.Trim().ToLowerInvariant();

        return normalized switch
        {
            "draft" => "draft",
            "rewrite" => "rewrite",
            "shorten" => "shorten",
            "expand" => "expand",
            "spoiler-safe-summary" => "spoiler-safe-summary",
            _ => "draft"
        };
    }

    private static string BuildInstructions(string mode)
    {
        const string sharedRules = """
You are Backlogr's review-writing assistant for a social video game review app.

Rules:
- Help the user write or revise a game review draft.
- Keep the user's opinion and intent.
- Do not invent game facts, plot details, or playtime details.
- Do not mention that you are an AI.
- Keep the output ready to paste into the app.
- Avoid markdown, bullet points, and titles unless the user explicitly asks for them.
- Be concise and natural.
""";

        var modeInstructions = mode switch
        {
            "draft" => """
Mode: draft.
Write a polished first-person game review draft from the user's notes.
Keep it readable and personal.
Only use details the user provided.
""",
            "rewrite" => """
Mode: rewrite.
Rewrite the existing review for clarity and flow.
Preserve the user's overall opinion and specific points.
Do not add new claims.
""",
            "shorten" => """
Mode: shorten.
Condense the existing text while preserving the user's main opinion and strongest points.
""",
            "expand" => """
Mode: expand.
Expand the existing text into a fuller review while staying grounded in the user's original wording and notes.
Do not invent new facts.
""",
            "spoiler-safe-summary" => """
Mode: spoiler-safe-summary.
Create a spoiler-safe version of the user's review or notes.
Avoid plot reveals, late-game details, twists, endings, or hidden content.
""",
            _ => """
Mode: draft.
Write a polished review draft from the user's notes.
"""
        };

        return $"{sharedRules}\n{modeInstructions}";
    }

    private static string BuildInput(ReviewAssistantRequestDto dto, string mode)
    {
        var prompt = dto.Prompt?.Trim();
        var existingText = dto.ExistingText?.Trim();

        return mode switch
        {
            "draft" => $$"""
Create a review draft from these notes:

{{prompt}}
""",
            "rewrite" => $$"""
Rewrite this review for clarity and flow.

Existing review:
{{existingText}}

Extra notes:
{{prompt}}
""",
            "shorten" => $$"""
Shorten this review while keeping the same opinion and key points.

Existing review:
{{existingText}}

Extra notes:
{{prompt}}
""",
            "expand" => $$"""
Expand this review into a fuller version without inventing details.

Existing review:
{{existingText}}

Extra notes:
{{prompt}}
""",
            "spoiler-safe-summary" => $$"""
Turn this into a spoiler-safe version.

Existing review:
{{existingText}}

Notes:
{{prompt}}
""",
            _ => $$"""
Help with this review request.

Existing review:
{{existingText}}

Notes:
{{prompt}}
"""
        };
    }
}