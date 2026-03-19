using Backlogr.Api.DTOs.Library;
using Backlogr.Api.DTOs.Reviews;

namespace Backlogr.Api.DTOs.Games;

public sealed class GameViewerStateResponseDto
{
    public LibraryLogResponseDto? Log { get; set; }

    public ReviewResponseDto? Review { get; set; }
}
