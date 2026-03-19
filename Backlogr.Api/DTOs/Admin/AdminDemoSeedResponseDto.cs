namespace Backlogr.Api.DTOs.Admin;

public sealed class AdminDemoSeedResponseDto
{
    public string DemoAdminUserName { get; set; } = string.Empty;

    public string DemoAdminEmail { get; set; } = string.Empty;

    public int UsersCreated { get; set; }

    public int UsersUpdated { get; set; }

    public int GamesImported { get; set; }

    public int GamesMatchedExistingCatalog { get; set; }

    public int LogsCreated { get; set; }

    public int LogsUpdated { get; set; }

    public int ReviewsCreated { get; set; }

    public int ReviewsUpdated { get; set; }

    public int LikesCreated { get; set; }

    public int CommentsCreated { get; set; }

    public int FollowsCreated { get; set; }

    public bool AiBackfillRun { get; set; }

    public IList<string> SeededUsers { get; set; } = [];

    public IList<string> SeededGames { get; set; } = [];

    public IList<string> Notes { get; set; } = [];
}
