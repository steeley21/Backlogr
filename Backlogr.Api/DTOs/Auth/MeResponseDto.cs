namespace Backlogr.Api.DTOs.Auth;

public sealed class MeResponseDto
{
    public Guid UserId { get; set; }

    public string UserName { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? AvatarUrl { get; set; }

    public string? Bio { get; set; }

    public IList<string> Roles { get; set; } = new List<string>();
}