using AggregationService.Abstractions;
using Microsoft.Extensions.Caching.Memory;

namespace AggregationService.Services
{
    public class CacheService(IMemoryCache cache) : ICacheService
    {
        private readonly IMemoryCache _cache = cache;

        // Get method to retrieve data from cache
        public T? Get<T>(string cacheKey)
        {
            return _cache.TryGetValue(cacheKey, out T cachedData) ? cachedData : default;
        }

        // Set method to store data in cache
        public void Set<T>(string cacheKey, T data, TimeSpan expiration)
        {
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration
            };

            _cache.Set(cacheKey, data, cacheOptions);
        }
    }
}
