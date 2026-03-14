namespace Backlogr.Api.DTOs.Admin;

public sealed class AdminUserSummaryDto
{
    public Guid UserId { get; set; }

    public string UserName { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public IList<string> Roles { get; set; } = [];

    public DateTime? CreatedAtUtc { get; set; }
}
