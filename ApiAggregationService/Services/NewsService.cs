using AggregationService.Abstractions;
using AggregationService.Models.Response;
using Newtonsoft.Json;

namespace AggregationService.Services;

public class NewsService(
    IHttpClientFactory clientFactory,
    ILogger<NewsService> logger,
    ICacheService cacheService) : INewsService
{
    private readonly IHttpClientFactory _clientFactory = clientFactory;
    private readonly ILogger<NewsService> _logger = logger;
    private readonly ICacheService _cacheService = cacheService;

    private const string NewsApiUrl = "https://newsapi.org/v2";
    private readonly string _apiKey = Environment.GetEnvironmentVariable("NewsApiKey");

    public async Task<IEnumerable<AggregatedData>> FetchNewsDataAsync()
    {
        var requestUrl = $"https://newsapi.org/v2/top-headlines?country=us&apiKey={_apiKey}";

        var cachedResult = _cacheService.Get<IEnumerable<AggregatedData>>(requestUrl);

        if (cachedResult is not null)
        {
            return cachedResult;
        }

        var client = _clientFactory.CreateClient();

        try
        {
            var response = await client.GetAsync(requestUrl);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError($"News API returned status code {response.StatusCode}. Error content: {errorContent}");
                return Enumerable.Empty<AggregatedData>();
            }

            var content = await response.Content.ReadAsStringAsync();
            var newsData = JsonConvert.DeserializeObject<NewsApiResponse>(content);

            if (newsData?.Articles == null)
            {
                _logger.LogWarning("News API returned null or empty article list.");
                return Enumerable.Empty<AggregatedData>();
            }

            var result = newsData.Articles.Select(article => new AggregatedData
            {
                Source = "NewsAPI",
                Category = "News",
                Date = article.PublishedAt,
                Data = $"{article.Title} - {article.Description}"
            });

            _cacheService.Set(requestUrl, result, TimeSpan.FromHours(2));

            return result;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error occurred while fetching news data");
            return Enumerable.Empty<AggregatedData>();
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Error deserializing news data");
            return Enumerable.Empty<AggregatedData>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while fetching news data");
            return Enumerable.Empty<AggregatedData>();
        }
    }
}