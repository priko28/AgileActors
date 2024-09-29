using AggregationService.Models.Response;

namespace AggregationService.Abstractions
{
    public interface INewsService
    {
        Task<IEnumerable<AggregatedData>> FetchNewsDataAsync();
    }
}
