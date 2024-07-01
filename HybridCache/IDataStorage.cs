namespace HybridCache;

public interface IDataStorage
{
    object TryGetValueByUuid(string id);
}