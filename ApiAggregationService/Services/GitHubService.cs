using AggregationService.Abstractions;
using AggregationService.Models.Response;
using Newtonsoft.Json;
using System.Diagnostics;

namespace AggregationService.Services;

public class GitHubService(
    IHttpClientFactory clientFactory,
    ILogger<GitHubService> logger) : IGitHubService
{
    private readonly IHttpClientFactory _clientFactory = clientFactory;
    private readonly ILogger<GitHubService> _logger = logger;
    //private readonly IStatisticsService _statisticsService = statisticsService;

    public async Task<IEnumerable<AggregatedData>> FetchGitHubDataAsync()
    {
        var client = _clientFactory.CreateClient();
        client.DefaultRequestHeaders.Add("User-Agent", "ApiAggregationService");
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = await client.GetAsync("https://api.github.com/search/repositories?q=language:csharp&sort=stars&order=desc");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();

            stopwatch.Stop();
            //_statisticsService.RecordApiCall("GitHub", stopwatch.ElapsedMilliseconds);

            var githubData = JsonConvert.DeserializeObject<GitHubSearchResponse>(content);
            return githubData.Items.Select(repo => new AggregatedData
            {
                Source = "GitHub",
                Category = "Repository",
                Date = repo.CreatedAt,
                Data = $"{repo.Name} - {repo.Description} (Stars: {repo.StargazersCount})"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching GitHub data");
            return Enumerable.Empty<AggregatedData>();
        }
    }
}