namespace AggregationService.Abstractions
{
    public interface ICacheService
    {
        T? Get<T>(string cacheKey);
        void Set<T>(string cacheKey, T data, TimeSpan expiration);
    }
}
