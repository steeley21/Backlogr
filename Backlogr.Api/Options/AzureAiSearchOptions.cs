namespace Backlogr.Api.Options;

public sealed class AzureAiSearchOptions
{
    public const string SectionName = "AzureAiSearch";

    public string Endpoint { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string GamesIndexName { get; set; } = "games";
}