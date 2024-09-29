using AggregationService.Abstractions;
using AggregationService.Models.Response;
using Newtonsoft.Json;

namespace AggregationService.Services;

public class WeatherService(
    IHttpClientFactory clientFactory,
    ILogger<WeatherService> logger,
    ICacheService cacheService) : IWeatherService
{
    private readonly IHttpClientFactory _clientFactory = clientFactory;
    private readonly ILogger<WeatherService> _logger = logger;
    private readonly ICacheService _cacheService = cacheService;

    private readonly string _apiKey = Environment.GetEnvironmentVariable("WeatherApiKey");
    private const string WeatherApiUrl = "https://api.openweathermap.org/data/2.5/weather";
    public async Task<IEnumerable<AggregatedData>> FetchWeatherDataAsync()
    {
        var client = _clientFactory.CreateClient();
        var requestUrl = $"{WeatherApiUrl}?q=London&appid={_apiKey}&units=metric";

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

            var weatherData = JsonConvert.DeserializeObject<WeatherData>(content);

            var result = new[]
            {
                new AggregatedData
                {
                    Source = "OpenWeatherMap",
                    Category = "Weather",
                    Date = DateTime.UtcNow,
                    Data = $"City: {weatherData.CityName}, " +
                           $"Temperature: {weatherData.Main.Temperature}°C, " +
                           $"Humidity: {weatherData.Main.Humidity}%, " +
                           $"Description: {weatherData.Weather[0].Description}"
                }
            };

            _cacheService.Set(requestUrl, result, TimeSpan.FromHours(2));

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching weather data");
            return Enumerable.Empty<AggregatedData>();
        }
    }
}