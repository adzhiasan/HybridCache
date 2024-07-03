namespace HybridCache;

public interface IHybridCache
{
    Task<T?> GetOrAddAsync<T>(string key, Func<Task<T?>> itemProvider, TimeSpan ttl);
}