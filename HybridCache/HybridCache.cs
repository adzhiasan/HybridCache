using Microsoft.Extensions.Caching.Memory;

namespace HybridCache;

public class HybridCache(
    IMemoryCache memoryCache,
    IRedisCacheAdapter redisCache) : IHybridCache
{
    public T? GetOrAdd<T>(string key, Func<T?> itemProvider, TimeSpan ttl)
    {
        if (memoryCache.TryGetValue(key, out T? value))
            return value;
        if (redisCache.TryGetValue(key, out value))
        {
            memoryCache.Set(key, value, ttl);
            return value;
        }

        value = itemProvider();
        memoryCache.Set(key, value, ttl);
        redisCache.Set(key, value, ttl);
        return value;
    }
}