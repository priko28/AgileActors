using AggregationService.Abstractions;
using AggregationService.Models.Response;
using Newtonsoft.Json;

namespace AggregationService.Services;

public class GitHubService(
    IHttpClientFactory clientFactory,
    ILogger<GitHubService> logger,
    ICacheService cacheService) : IGitHubService
{
    private readonly IHttpClientFactory _clientFactory = clientFactory;
    private readonly ILogger<GitHubService> _logger = logger;
    private readonly ICacheService _cacheService = cacheService;

    public async Task<IEnumerable<AggregatedData>> FetchGitHubDataAsync()
    {
        var client = _clientFactory.CreateClient();
        client.DefaultRequestHeaders.Add("User-Agent", "ApiAggregationService");
        var requestUrl = "https://api.github.com/search/repositories?q=language:csharp&sort=stars&order=desc";

        var cachedResult = _cacheService.Get<IEnumerable<AggregatedData>>(requestUrl);

        if (cachedResult is not null)
        {
            return cachedResult;
        }

        try
        {
            var response = await client.GetAsync(requestUrl);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();

            var githubData = JsonConvert.DeserializeObject<GitHubSearchResponse>(content);

            var result = githubData.Items.Select(repo =>
                new AggregatedData
                {
                    Source = "GitHub",
                    Category = "Repository",
                    Date = repo.CreatedAt,
                    Data = $"{repo.Name} - {repo.Description} (Stars: {repo.StargazersCount})"
                });

            _cacheService.Set(requestUrl, result, TimeSpan.FromHours(2));

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching GitHub data");
            return Enumerable.Empty<AggregatedData>();
        }
    }
}