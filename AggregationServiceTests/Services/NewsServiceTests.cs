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
    public class NewsServiceTests
    {
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock = new();
        private readonly Mock<ILogger<NewsService>> _loggerMock = new();
        private readonly Mock<ICacheService> _cacheServiceMock = new();
        private readonly NewsService _service;

        public NewsServiceTests()
        {
            _service = new NewsService(
                _httpClientFactoryMock.Object,
                _loggerMock.Object,
                _cacheServiceMock.Object);
        }

        [Fact]
        public async Task FetchNewsDataAsync_CachedData_ReturnsCachedResult()
        {
            // Arrange
            var cachedData = new List<AggregatedData>
            {
                new AggregatedData { Source = "NewsAPI", Category = "News", Date = DateTime.Now, Data = "Test News" }
            };
            _cacheServiceMock.Setup(c => c.Get<IEnumerable<AggregatedData>>(It.IsAny<string>())).Returns(cachedData);

            // Act
            var result = await _service.FetchNewsDataAsync();

            // Assert
            Assert.Equal(cachedData, result);
            _httpClientFactoryMock.Verify(f => f.CreateClient(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task FetchNewsDataAsync_NoCachedData_FetchesAndCachesData()
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
                    Content = new StringContent(JsonConvert.SerializeObject(new NewsApiResponse
                    {
                        Articles = new List<NewsArticle>
                        {
                            new NewsArticle { Title = "Test Title", Description = "Test Description", PublishedAt = DateTime.Now }
                        }
                    }))
                });

            var client = new HttpClient(mockHttpMessageHandler.Object);
            _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(client);

            _cacheServiceMock.Setup(c => c.Get<IEnumerable<AggregatedData>>(It.IsAny<string>())).Returns((IEnumerable<AggregatedData>)null);

            // Act
            var result = await _service.FetchNewsDataAsync();

            // Assert
            Assert.Single(result);
            Assert.Contains("Test Title", result.First().Data);
            _cacheServiceMock.Verify(c => c.Set(It.IsAny<string>(), It.IsAny<IEnumerable<AggregatedData>>(), It.IsAny<TimeSpan>()), Times.Once);
        }

        [Fact]
        public async Task FetchNewsDataAsync_ApiError_ReturnsEmptyResult()
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
                    StatusCode = HttpStatusCode.InternalServerError,
                    Content = new StringContent("Error message")
                });

            var client = new HttpClient(mockHttpMessageHandler.Object);
            _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(client);

            _cacheServiceMock.Setup(c => c.Get<IEnumerable<AggregatedData>>(It.IsAny<string>())).Returns((IEnumerable<AggregatedData>)null);

            // Act
            var result = await _service.FetchNewsDataAsync();

            // Assert
            Assert.Empty(result);
            _loggerMock.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("News API returned status code")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
