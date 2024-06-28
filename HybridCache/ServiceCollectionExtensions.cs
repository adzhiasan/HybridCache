using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.DependencyInjection;

namespace HybridCache;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSelfWrittenHybridCache(this IServiceCollection services,
        Action<RedisCacheOptions> redisSetupAction)
    {
        return services
            .AddMemoryCache()
            .AddSingleton<IRedisCacheAdapter, RedisCacheAdapter>()
            .AddSingleton<IHybridCache, HybridCache>()
            .AddStackExchangeRedisCache(redisSetupAction);
    }
}