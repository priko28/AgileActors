using AggregationService.Models.Response;

namespace AggregationService.Abstractions
{
    public interface IWeatherService
    {
        Task<IEnumerable<AggregatedData>> FetchWeatherDataAsync();
    }
}
