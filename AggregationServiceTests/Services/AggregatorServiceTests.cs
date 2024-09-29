using AggregationService.Abstractions;
using AggregationService.Models.Response;
using AggregationService.Services;
using AutoFixture;
using Moq;

namespace AggregationServiceTests.Services
{
    public class AggregatorServiceTests
    {
        private readonly Fixture _fixture = new();
        private readonly Mock<IWeatherService> _weatherServiceMock = new();
        private readonly Mock<INewsService> _newsServiceMock = new();
        private readonly Mock<IGitHubService> _gitHubServiceMock = new();
        private readonly AggregatorService _aggregatorService;
        public AggregatorServiceTests()
        {
            _aggregatorService = new AggregatorService(
                _weatherServiceMock.Object,
                _newsServiceMock.Object,
                _gitHubServiceMock.Object);
        }

        [Fact]
        public async Task GetAggregatedData_GivenAggregatorRequest_ShouldReturnCorrectResponse()
        {
            var weatherResponse = _fixture.Build<AggregatedData>()
                .With(x => x.Source, "OpenWeatherApi")
                .CreateMany(1);

            var newsResponse = _fixture.Build<AggregatedData>()
                .With(x => x.Source, "NewsApi")
                .CreateMany(1);

            var gitHubResponse = _fixture.Build<AggregatedData>()
                .With(x => x.Source, "GitHub")
                .CreateMany(1);

            _weatherServiceMock.Setup(x => x.FetchWeatherDataAsync()).ReturnsAsync(weatherResponse);
            _newsServiceMock.Setup(x => x.FetchNewsDataAsync()).ReturnsAsync(newsResponse);
            _gitHubServiceMock.Setup(x => x.FetchGitHubDataAsync()).ReturnsAsync(gitHubResponse);

            var result = await _aggregatorService.GetAggregatedDataAsync();

            var expectedResult = new List<AggregatedData>
            {
               weatherResponse.First(),
               newsResponse.First(),
               gitHubResponse.First(),
            };

            Assert.Equal(expectedResult, result);
        }
    }
}
