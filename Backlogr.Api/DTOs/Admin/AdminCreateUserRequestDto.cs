namespace Backlogr.Api.DTOs.Admin;

public sealed class AdminCreateUserRequestDto
{
    public string UserName { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public AdminAssignableRole Role { get; set; } = AdminAssignableRole.User;
}
