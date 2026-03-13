using Backlogr.Api.DTOs.AI;

namespace Backlogr.Api.Services.Interfaces;

public interface ISemanticSearchService
{
    Task<IReadOnlyList<SemanticSearchResultDto>> SearchAsync(string query, int take = 10);
}