using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

namespace HybridCache;

public interface IRedisCacheAdapter
{
    bool TryGetValue<T>(string key, out T? value);
    Task SetAsync(string key, object? value, TimeSpan ttl);
}

public class RedisCacheAdapter(IDistributedCache distributedCache) : IRedisCacheAdapter
{
    public bool TryGetValue<T>(string key, out T? value)
    {
        var cachedValue = distributedCache.GetString(key);
        switch (cachedValue)
        {
            case null:
                value = default;
                return false;
            case "__NULL__":
                value = default;
                return true;
            default:
                try
                {
                    value = JsonSerializer.Deserialize<T?>(cachedValue);
                    return true;
                }
                catch
                {
                    distributedCache.Remove(key);
                    value = default;
                    return false;
                }
        }
    }

    public async Task SetAsync(string key, object? value, TimeSpan ttl)
    {
        if (value == null)
        {
            await distributedCache.SetStringAsync(key, "__NULL__",
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = ttl });
        }
        else
        {
            var serializedValue = JsonSerializer.Serialize(value);
            await distributedCache.SetStringAsync(key, serializedValue,
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = ttl });
        }
    }
}