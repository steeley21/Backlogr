using Backlogr.Api.Models.Enums;

namespace Backlogr.Api.DTOs.Library;

public sealed class UpsertLibraryLogRequestDto
{
    public Guid GameId { get; set; }

    public LibraryStatus Status { get; set; }

    public decimal? Rating { get; set; }

    public string? Platform { get; set; }

    public decimal? Hours { get; set; }

    public DateTime? StartedAt { get; set; }

    public DateTime? FinishedAt { get; set; }

    public string? Notes { get; set; }
}