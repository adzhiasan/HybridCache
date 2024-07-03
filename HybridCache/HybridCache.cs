using Microsoft.Extensions.Caching.Memory;

namespace HybridCache;

public class HybridCache(
    IMemoryCache memoryCache,
    IRedisCacheAdapter redisCache) : IHybridCache
{
    public async Task<T?> GetOrAddAsync<T>(string key, Func<Task<T?>> itemProvider, TimeSpan ttl)
    {
        if (memoryCache.TryGetValue(key, out T? value))
            return value;
        if (redisCache.TryGetValue(key, out value))
        {
            memoryCache.Set(key, value, ttl);
            return value;
        }

        value = await itemProvider();
        memoryCache.Set(key, value, ttl);
        await redisCache.SetAsync(key, value, ttl);
        return value;
    }
}