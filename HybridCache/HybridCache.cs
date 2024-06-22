using Microsoft.Extensions.Caching.Memory;

namespace HybridCache;

public class HybridCache(
    IMemoryCache memoryCache,
    IRedisCache redisCache)
{
    public async Task<object> GetOrAddAsync(object key, Func<object> itemProvider)
    {
        object obj;
        if (!memoryCache.TryGetValue(key, out obj))
        {
            if (!await redisCache.TryGetValue(key, out obj))
            {
                obj = itemProvider();
            }
        }

        return obj;
    }
}