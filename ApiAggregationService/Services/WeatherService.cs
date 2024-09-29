using AggregationService.Abstractions;
using AggregationService.Models.Response;
using Newtonsoft.Json;
using System.Diagnostics;

namespace AggregationService.Services;

public class WeatherService(
    IHttpClientFactory clientFactory,
    ILogger<WeatherService> logger,
    IConfiguration configuration) : IWeatherService
{
    private readonly IHttpClientFactory _clientFactory = clientFactory;
    private readonly ILogger<WeatherService> _logger = logger;
    private readonly string _apiKey = Environment.GetEnvironmentVariable("WeatherApiKey");
    private const string WeatherApiUrl = "https://api.openweathermap.org/data/2.5/weather";
    public async Task<IEnumerable<AggregatedData>> FetchWeatherDataAsync()
    {
        var client = _clientFactory.CreateClient();
        var stopwatch = Stopwatch.StartNew();
        try
        {
            var response = await client.GetAsync($"{WeatherApiUrl}?q=London&appid={_apiKey}&units=metric");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();

            stopwatch.Stop();
            //_statisticsService.RecordApiCall("OpenWeatherMap", stopwatch.ElapsedMilliseconds);

            var weatherData = JsonConvert.DeserializeObject<WeatherData>(content);
            return
            [
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
            ];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching weather data");
            return Enumerable.Empty<AggregatedData>();
        }
    }
}


