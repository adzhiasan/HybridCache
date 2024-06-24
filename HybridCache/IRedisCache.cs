using StackExchange.Redis;

namespace HybridCache;

public interface IRedisCache
{
    Task<bool> TryGetValue(object key, out object value);
    void Set(object key, object o);
}

// public class RedisCache : IRedisCache
// {
//     private readonly IDatabase _redis;
//
//     public RedisCache(IDatabase redis)
//     {
//         _redis = redis;
//     }
//
//     public Task<bool> TryGetValue(object key, out object value)
//     {
//         _redis.StringGet()
//     }
//
//     public void Set(object key, object o)
//     {
//         throw new NotImplementedException();
//     }
// }