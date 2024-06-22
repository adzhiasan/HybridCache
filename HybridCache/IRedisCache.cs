namespace HybridCache;

public interface IRedisCache
{
    Task<bool> TryGetValue(object key, out object value);
    void Set(object key, object o);
}