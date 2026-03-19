namespace Backlogr.Api.Options;

public sealed class OpenAiOptions
{
    public const string SectionName = "OpenAI";

    public string ApiKey { get; set; } = string.Empty;
    public string ChatModel { get; set; } = "gpt-5.4-mini";
    public string EmbeddingModel { get; set; } = "text-embedding-3-small";
}