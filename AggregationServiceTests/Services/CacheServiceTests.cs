using AggregationService.Services;
using Microsoft.Extensions.Caching.Memory;
using Moq;

namespace AggregationService.Tests
{
    public class CacheServiceTests
    {
        private readonly Mock<IMemoryCache> _memoryCacheMock = new();
        private readonly CacheService _cacheService;

        public CacheServiceTests()
        {
            _cacheService = new CacheService(_memoryCacheMock.Object);
        }

        [Fact]
        public void Get_GivenCachedDataThatExist_ReturnsCachedData()
        {
            // Arrange
            var cacheKey = "testKey";
            var expectedData = "testData";
            object cachedValue = expectedData;

            _memoryCacheMock
                .Setup(m => m.TryGetValue(cacheKey, out cachedValue))
                .Returns(true);

            // Act
            var result = _cacheService.Get<string>(cacheKey);

            // Assert
            Assert.Equal(expectedData, result);
        }

        [Fact]
        public void Get_GivenNonExistingCachedData_ReturnsDefault()
        {
            // Arrange
            var cacheKey = "testKey";

            _memoryCacheMock
                .Setup(m => m.TryGetValue(cacheKey, out It.Ref<object>.IsAny))
                .Returns(false);

            // Act
            var result = _cacheService.Get<string>(cacheKey);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Set_GivenValidInput_CachesData()
        {
            // Arrange
            var cacheKey = "testKey";
            var dataToCache = "testData";
            var expiration = TimeSpan.FromMinutes(10);
            var mockCacheEntry = new Mock<ICacheEntry>();

            _memoryCacheMock
                .Setup(m => m.CreateEntry(cacheKey))
                .Returns(mockCacheEntry.Object);

            // Act
            _cacheService.Set(cacheKey, dataToCache, expiration);

            // Assert
            _memoryCacheMock.Verify(m => m.CreateEntry(cacheKey), Times.Once);
            mockCacheEntry.VerifySet(e => e.AbsoluteExpirationRelativeToNow = expiration);
            mockCacheEntry.VerifySet(e => e.Value = dataToCache);
        }
    }
}
