namespace HybridCache;

public interface IHybridCache
{
    T? GetOrAdd<T>(string key, Func<T?> itemProvider, TimeSpan ttl);
}