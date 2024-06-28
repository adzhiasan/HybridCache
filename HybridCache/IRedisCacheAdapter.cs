using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

namespace HybridCache;

public interface IRedisCacheAdapter
{
    bool TryGetValue<T>(string key, out T? value);
    void Set(string key, object? value, TimeSpan ttl);
}

internal class RedisCacheAdapter(IDistributedCache distributedCache) : IRedisCacheAdapter
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
                value = JsonSerializer.Deserialize<T?>(cachedValue);
                return true;
        }
    }

    public void Set(string key, object? value, TimeSpan ttl)
    {
        if (value == null)
        {
            distributedCache.SetString(key, "__NULL__");
        }
        else
        {
            var serializedValue = JsonSerializer.Serialize(value);
            distributedCache.SetStringAsync(key, serializedValue,
                new DistributedCacheEntryOptions() { AbsoluteExpirationRelativeToNow = ttl });
        }
    }
}