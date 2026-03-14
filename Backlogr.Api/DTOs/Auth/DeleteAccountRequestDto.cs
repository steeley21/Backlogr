namespace Backlogr.Api.DTOs.Auth;

public sealed class DeleteAccountRequestDto
{
    public string Password { get; set; } = string.Empty;

    public string ConfirmationUserName { get; set; } = string.Empty;
}
