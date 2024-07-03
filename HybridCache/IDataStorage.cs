namespace HybridCache;

public interface IDataStorage
{
    Task<object> TryGetValueByUuidAsync(string id);
}