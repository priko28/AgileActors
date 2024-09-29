using AggregationService.Abstractions;
using AggregationService.Services;
using Microsoft.Extensions.Caching.Memory;

namespace AggregationService
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection ConfigureServices(IServiceCollection services)
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
            services.AddControllers();
            services.AddHttpClient();
            services.AddMemoryCache();

            services
                .AddScoped<IMemoryCache, MemoryCache>()
                .AddScoped<ICacheService, CacheService>()
                .AddSingleton<IWeatherService, WeatherService>()
                .AddSingleton<INewsService, NewsService>()
                .AddSingleton<IGitHubService, GitHubService>()
                .AddScoped<IAggregatorService, AggregatorService>();

            return services;
        }
    }
}
