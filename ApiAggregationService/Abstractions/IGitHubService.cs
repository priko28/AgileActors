using AggregationService.Models.Response;

namespace AggregationService.Abstractions
{
    public interface IGitHubService
    {
        Task<IEnumerable<AggregatedData>> FetchGitHubDataAsync();
    }
}
