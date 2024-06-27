using Microsoft.Extensions.Caching.Memory;

namespace HybridCache;

public class HybridCache(
    IMemoryCache memoryCache,
    IRedisCacheAdapter redisCache)
{
    public object? GetOrAdd(string key, Func<object> itemProvider)
    {
        if (memoryCache.TryGetValue(key, out var value))
            return value;
        if (redisCache.TryGetValue(key, out value))
        {
            memoryCache.Set(key, value);
            return value;
        }

        value = itemProvider();
        memoryCache.Set(key, value);
        redisCache.Set(key, value);
        return value;
    }
}