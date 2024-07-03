using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace HybridCache.Tests;

[TestClass]
public class HybridCacheTests
{
    private const string CacheKey = "Test:Cache";
    private static readonly TimeSpan Ttl = TimeSpan.FromSeconds(10);

    private readonly IMemoryCache _memoryCache;
    private readonly Mock<IRedisCacheAdapter> _redisCacheMock;
    private readonly Mock<IDataStorage> _dataStorageMock;

    private readonly HybridCache _hybridCache;

    public HybridCacheTests()
    {
        var services = new ServiceCollection();
        services.AddMemoryCache();
        var serviceProvider = services.BuildServiceProvider();

        _memoryCache = serviceProvider.GetRequiredService<IMemoryCache>();

        _redisCacheMock = new Mock<IRedisCacheAdapter>();
        _dataStorageMock = new Mock<IDataStorage>();

        _hybridCache = new HybridCache(_memoryCache, _redisCacheMock.Object);
    }

    [TestMethod]
    public async Task ShouldReturnValueFromMemoryCache_WhenValueWithTargetKeyExistsThere()
    {
        // Arrange
        object cachedValue = "42";
        _memoryCache.Set(CacheKey, cachedValue);

        // Act
        var resultValue = await _hybridCache
            .GetOrAddAsync(
                CacheKey,
                async () => await _dataStorageMock.Object.TryGetValueByUuidAsync(Guid.NewGuid().ToString()),
                Ttl);

        // Assert
        resultValue.Should().Be(cachedValue);
    }

    [TestMethod]
    public async Task ShouldReturnValueFromRedisCache_WhenValueWithTargetKeyDoesNotExistInMemoryCache()
    {
        // Arrange
        object? cachedValue = "42";

        _redisCacheMock
            .Setup(rc => rc.TryGetValue(CacheKey, out cachedValue))
            .Returns(true);

        // Act
        var resultValue = await _hybridCache.GetOrAddAsync(CacheKey,
            async () => await _dataStorageMock.Object.TryGetValueByUuidAsync(Guid.NewGuid().ToString()),
            Ttl);

        // Assert
        resultValue.Should().Be(cachedValue);
    }

    [TestMethod]
    public async Task ShouldReturnValueFromDataStorage_WhenValueDoesNotExistNeitherInMemoryCacheNorInRedis()
    {
        // Arrange
        object? valueFromDataStorage = "42";
        _redisCacheMock
            .Setup(rc => rc.TryGetValue(CacheKey, out It.Ref<object?>.IsAny))
            .Returns(false);
        _dataStorageMock
            .Setup(ds => ds.TryGetValueByUuidAsync(It.IsAny<string>()))
            .ReturnsAsync(valueFromDataStorage);

        // Act
        var resultValue = await _hybridCache.GetOrAddAsync(CacheKey,
            async () => await _dataStorageMock.Object.TryGetValueByUuidAsync(Guid.NewGuid().ToString()), Ttl);

        // Assert
        resultValue.Should().Be(valueFromDataStorage);
    }

    [TestMethod]
    public async Task ShouldReturnNull_WhenValueDoesNotExistAnywhere()
    {
        // Arrange
        _redisCacheMock
            .Setup(rc => rc.TryGetValue(CacheKey, out It.Ref<object?>.IsAny))
            .Returns(false);
        _dataStorageMock
            .Setup(ds => ds.TryGetValueByUuidAsync(It.IsAny<string>()))
            .ReturnsAsync(null);

        // Act
        var resultValue = await _hybridCache.GetOrAddAsync(CacheKey,
            async () => await _dataStorageMock.Object.TryGetValueByUuidAsync(Guid.NewGuid().ToString()), Ttl);

        // Assert
        resultValue.Should().BeNull();
    }
    
    [TestMethod]
    public async Task ShouldReturnNull_WhenNullCached()
    {
        // Arrange
        object value = null;
        _redisCacheMock
            .Setup(rc => rc.TryGetValue(CacheKey, out value))
            .Returns(true);

        // Act
        var resultValue = await _hybridCache.GetOrAddAsync(CacheKey,
            async () => await _dataStorageMock.Object.TryGetValueByUuidAsync(Guid.NewGuid().ToString()), Ttl);

        // Assert
        resultValue.Should().BeNull();
    }
}