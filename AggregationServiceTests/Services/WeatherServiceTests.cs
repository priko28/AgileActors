using AggregationService.Abstractions;
using AggregationService.Models.Response;
using AggregationService.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using System.Net;

namespace AggregationService.Tests
{
    public class WeatherServiceTests
    {
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock = new();
        private readonly Mock<ILogger<WeatherService>> _loggerMock = new();
        private readonly Mock<ICacheService> _cacheServiceMock = new();
        private readonly WeatherService _service;

        public WeatherServiceTests()
        {
            _service = new WeatherService(
                _httpClientFactoryMock.Object,
                _loggerMock.Object,
                _cacheServiceMock.Object);
        }

        [Fact]
        public async Task FetchWeatherDataAsync_CachedData_ReturnsCachedResult()
        {
            // Arrange
            var cachedData = new List<AggregatedData>
            {
                new AggregatedData { Source = "OpenWeatherMap", Category = "Weather", Date = DateTime.Now, Data = "Test Weather" }
            };
            _cacheServiceMock.Setup(c => c.Get<IEnumerable<AggregatedData>>(It.IsAny<string>())).Returns(cachedData);

            // Act
            var result = await _service.FetchWeatherDataAsync();

            // Assert
            Assert.Equal(cachedData, result);
            _httpClientFactoryMock.Verify(f => f.CreateClient(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task FetchWeatherDataAsync_NoCachedData_FetchesAndCachesData()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonConvert.SerializeObject(new WeatherData
                    {
                        CityName = "London",
                        Main = new WeatherMain { Temperature = 20, Humidity = 60 },
                        Weather = new List<WeatherDescription> { new WeatherDescription { Description = "Clear sky" } }
                    }))
                });

            var client = new HttpClient(mockHttpMessageHandler.Object);
            _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(client);

            _cacheServiceMock.Setup(c => c.Get<IEnumerable<AggregatedData>>(It.IsAny<string>())).Returns((IEnumerable<AggregatedData>)null);

            // Act
            var result = await _service.FetchWeatherDataAsync();

            // Assert
            Assert.Single(result);
            Assert.Contains("London", result.First().Data);
            Assert.Contains("20°C", result.First().Data);
            Assert.Contains("60%", result.First().Data);
            Assert.Contains("Clear sky", result.First().Data);
            _cacheServiceMock.Verify(c => c.Set(It.IsAny<string>(), It.IsAny<IEnumerable<AggregatedData>>(), It.IsAny<TimeSpan>()), Times.Once);
        }

        [Fact]
        public async Task FetchWeatherDataAsync_ApiError_ReturnsEmptyResult()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError
                });

            var client = new HttpClient(mockHttpMessageHandler.Object);
            _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(client);

            _cacheServiceMock.Setup(c => c.Get<IEnumerable<AggregatedData>>(It.IsAny<string>())).Returns((IEnumerable<AggregatedData>)null);

            // Act
            var result = await _service.FetchWeatherDataAsync();

            // Assert
            Assert.Empty(result);
            _loggerMock.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Error fetching weather data")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
