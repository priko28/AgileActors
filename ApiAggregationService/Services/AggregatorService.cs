using AggregationService.Abstractions;
using AggregationService.Models.Response;

namespace AggregationService.Services;

public class AggregatorService(IWeatherService weatherService, INewsService newsService, IGitHubService gitHubService) : IAggregatorService
{
    private readonly IWeatherService _weatherService = weatherService;
    private readonly INewsService _newsService = newsService;
    private readonly IGitHubService _gitHubService = gitHubService;

    public async Task<IEnumerable<AggregatedData>> GetAggregatedDataAsync(
        string filter = null,
        string sortBy = null)
    {
        var tasks = new List<Task<IEnumerable<AggregatedData>>>
        {
            _weatherService.FetchWeatherDataAsync(),
            _newsService.FetchNewsDataAsync(),
            _gitHubService.FetchGitHubDataAsync()
        };

        var results = await Task.WhenAll(tasks);

        var aggregatedData = results.SelectMany(r => r).ToList();

        // Apply filtering
        if (!string.IsNullOrEmpty(filter))
        {
            aggregatedData = aggregatedData.Where(d => d.Source.Contains(filter, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        // Apply sorting
        if (!string.IsNullOrEmpty(sortBy))
        {
            switch (sortBy.ToLower())
            {
                case "source":
                    aggregatedData = aggregatedData.OrderBy(d => d.Source).ToList();
                    break;
                case "date":
                    aggregatedData = aggregatedData.OrderBy(d => d.Date).ToList();
                    break;
                case "category":
                    aggregatedData = aggregatedData.OrderBy(d => d.Category).ToList();
                    break;
            }
        }

        return aggregatedData;
    }
}