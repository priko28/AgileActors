using AggregationService.Abstractions;
using AggregationService.Models.Response;
using Newtonsoft.Json;
using System.Diagnostics;

namespace AggregationService.Services;

public class NewsService(
    IHttpClientFactory clientFactory,
    ILogger<NewsService> logger,
    IConfiguration configuration) : INewsService
{
    private readonly IHttpClientFactory _clientFactory = clientFactory;
    private readonly ILogger<NewsService> _logger = logger;
    private const string NewsApiUrl = "https://newsapi.org/v2";
    private readonly string _apiKey = Environment.GetEnvironmentVariable("NewsApiKey");

    public async Task<IEnumerable<AggregatedData>> FetchNewsDataAsync()
    {
        var client = _clientFactory.CreateClient();

        try
        {
            if (string.IsNullOrEmpty(_apiKey))
            {
                throw new InvalidOperationException("News API key is not configured.");
            }

            var url = $"https://newsapi.org/v2/top-headlines?country=us&apiKey={_apiKey}";
            var response = await client.GetAsync(url);

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

            return newsData.Articles.Select(article => new AggregatedData
            {
                Source = "NewsAPI",
                Category = "News",
                Date = article.PublishedAt,
                Data = $"{article.Title} - {article.Description}"
            });
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
