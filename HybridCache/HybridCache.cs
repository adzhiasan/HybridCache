using Microsoft.Extensions.Caching.Memory;

namespace HybridCache;

public class HybridCache(
    IMemoryCache memoryCache,
    IRedisCache redisCache)
{
    public async Task<object> GetOrAddAsync(object key, Func<object> itemProvider)
    {
        if (memoryCache.TryGetValue(key, out var obj)) 
            return obj;
        if (await redisCache.TryGetValue(key, out obj))
        {
            memoryCache.Set(key, obj);
            return obj;
        }
        obj = itemProvider();
        memoryCache.Set(key, obj);
        redisCache.Set(key, obj);
        return obj;
    }
}