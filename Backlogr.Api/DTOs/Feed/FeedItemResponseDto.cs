using Backlogr.Api.Models.Enums;

namespace Backlogr.Api.DTOs.Feed;

public sealed class FeedItemResponseDto
{
    public FeedItemType ItemType { get; set; }

    public DateTime ActivityAt { get; set; }

    public Guid UserId { get; set; }

    public string UserName { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public Guid GameId { get; set; }

    public string GameTitle { get; set; } = string.Empty;

    public string? CoverImageUrl { get; set; }

    public Guid? GameLogId { get; set; }

    public Guid? ReviewId { get; set; }

    public LibraryStatus? Status { get; set; }

    public decimal? Rating { get; set; }

    public string? Platform { get; set; }

    public decimal? Hours { get; set; }

    public string? ReviewText { get; set; }

    public bool? HasSpoilers { get; set; }
}