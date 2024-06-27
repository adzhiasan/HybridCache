using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

namespace HybridCache;

public interface IRedisCacheAdapter
{
    bool TryGetValue(string key, out object value);
    void Set(string key, object? value);
}

internal class RedisCacheAdapter(IDistributedCache distributedCache) : IRedisCacheAdapter
{
    public bool TryGetValue(string key, out object? value)
    {
        var cachedValue = distributedCache.GetString(key);
        switch (cachedValue)
        {
            case null:
                value = null;
                return false;
            case "__NULL__":
                value = null;
                return true;
            default:
                value = JsonSerializer.Deserialize<object>(cachedValue);
                return true;
        }
    }

    public void Set(string key, object? value)
    {
        if (value == null)
        {
            distributedCache.SetString(key, "__NULL__");
        }
        else
        {
            var serializedValue = JsonSerializer.Serialize(value);
            distributedCache.SetStringAsync(key, serializedValue);
        }
    }
}