namespace Backlogr.Api.Options;

public sealed class BootstrapSuperAdminOptions
{
    public const string SectionName = "BootstrapSuperAdmin";

    public bool Enabled { get; set; }

    public string TargetEmail { get; set; } = string.Empty;

    public string Secret { get; set; } = string.Empty;
}
