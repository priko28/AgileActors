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
    public class GitHubServiceTests
    {
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock = new();
        private readonly Mock<ILogger<GitHubService>> _loggerMock = new();
        private readonly Mock<ICacheService> _cacheServiceMock = new();
        private readonly GitHubService _service;

        public GitHubServiceTests()
        {
            _service = new GitHubService(
                _httpClientFactoryMock.Object,
                _loggerMock.Object,
                _cacheServiceMock.Object);
        }

        [Fact]
        public async Task FetchGitHubDataAsync_CachedData_ReturnsCachedResult()
        {
            // Arrange
            var cachedData = new List<AggregatedData>
            {
                new AggregatedData { Source = "GitHub", Category = "Repository", Date = DateTime.Now, Data = "Test Repo" }
            };
            _cacheServiceMock.Setup(c => c.Get<IEnumerable<AggregatedData>>(It.IsAny<string>())).Returns(cachedData);

            // Act
            var result = await _service.FetchGitHubDataAsync();

            // Assert
            Assert.Equal(cachedData, result);
        }

        [Fact]
        public async Task FetchGitHubDataAsync_NoCachedData_FetchesAndCachesData()
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
                    Content = new StringContent(JsonConvert.SerializeObject(new GitHubSearchResponse
                    {
                        Items = new List<GitHubRepository>
                        {
                            new GitHubRepository { Name = "TestRepo", Description = "Test Description", StargazersCount = 100, CreatedAt = DateTime.Now }
                        }
                    }))
                });

            var client = new HttpClient(mockHttpMessageHandler.Object);
            _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(client);

            _cacheServiceMock.Setup(c => c.Get<IEnumerable<AggregatedData>>(It.IsAny<string>())).Returns((IEnumerable<AggregatedData>)null);

            // Act
            var result = await _service.FetchGitHubDataAsync();

            // Assert
            Assert.Single(result);
            Assert.Contains("TestRepo", result.First().Data);
            _cacheServiceMock.Verify(c => c.Set(It.IsAny<string>(), It.IsAny<IEnumerable<AggregatedData>>(), It.IsAny<TimeSpan>()), Times.Once);
        }

        [Fact]
        public async Task FetchGitHubDataAsync_ApiError_ReturnsEmptyResult()
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
            var result = await _service.FetchGitHubDataAsync();

            // Assert
            Assert.Empty(result);
            _loggerMock.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Error fetching GitHub data")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}