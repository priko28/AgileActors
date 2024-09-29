using AggregationService.Models.Response;

namespace AggregationService.Abstractions
{
    public interface IAggregatorService
    {
        Task<IEnumerable<AggregatedData>> GetAggregatedDataAsync(string filter = null, string sortBy = null);
    }
}
