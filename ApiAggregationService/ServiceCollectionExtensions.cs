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
                .AddSingleton<IMemoryCache, MemoryCache>()
                .AddSingleton<ICacheService, CacheService>()
                .AddScoped<IWeatherService, WeatherService>()
                .AddScoped<INewsService, NewsService>()
                .AddScoped<IGitHubService, GitHubService>()
                .AddScoped<IAggregatorService, AggregatorService>();

            return services;
        }
    }
}