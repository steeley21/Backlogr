namespace Backlogr.Api.DTOs.Admin;

public sealed class AdminDemoSeedRequestDto
{
    public bool RunAiBackfill { get; set; } = true;
}
