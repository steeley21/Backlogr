namespace Backlogr.Api.DTOs.Admin;

public sealed class AdminUpdateUserRoleRequestDto
{
    public AdminAssignableRole Role { get; set; } = AdminAssignableRole.User;
}
