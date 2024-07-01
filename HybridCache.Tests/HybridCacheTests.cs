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
    public void ShouldReturnValueFromMemoryCache_WhenValueWithTargetKeyExistsThere()
    {
        // Arrange
        object cachedValue = "42";
        _memoryCache.Set(CacheKey, cachedValue);

        // Act
        var resultValue = _hybridCache
            .GetOrAdd(
                CacheKey,
                () => _dataStorageMock.Object.TryGetValueByUuid(Guid.NewGuid().ToString()),
                Ttl);

        // Assert
        resultValue.Should().Be(cachedValue);
    }

    [TestMethod]
    public void ShouldReturnValueFromRedisCache_WhenValueWithTargetKeyDoesNotExistInMemoryCache()
    {
        // Arrange
        object? cachedValue = "42";

        _redisCacheMock
            .Setup(rc => rc.TryGetValue(CacheKey, out cachedValue))
            .Returns(true)
            .Verifiable(Times.Once);

        // Act
        var resultValue = _hybridCache.GetOrAdd(CacheKey,
            () => _dataStorageMock.Object.TryGetValueByUuid(Guid.NewGuid().ToString()),
            Ttl);

        // Assert
        resultValue.Should().Be(cachedValue);
        _memoryCache.TryGetValue(CacheKey, out cachedValue).Should().BeTrue();
        _redisCacheMock.Verify();
        _dataStorageMock.Verify(
            ds => ds.TryGetValueByUuid(It.IsAny<string>()),
            Times.Never);
    }


    [TestMethod]
    public void ShouldReturnValueFromDataStorage_WhenValueDoesNotExistNeitherInMemoryCacheNorInRedis()
    {
        // Arrange
        object? valueFromDataStorage = "42";
        _redisCacheMock
            .Setup(rc => rc.TryGetValue(CacheKey, out It.Ref<object?>.IsAny))
            .Returns(false).Verifiable(Times.Once);
        _redisCacheMock
            .Setup(rc => rc.Set(CacheKey, valueFromDataStorage, Ttl))
            .Verifiable(Times.Once);
        _dataStorageMock
            .Setup(ds => ds.TryGetValueByUuid(It.IsAny<string>()))
            .Returns(valueFromDataStorage)
            .Verifiable(Times.Once);

        // Act
        var resultValue = _hybridCache.GetOrAdd(CacheKey,
            () => _dataStorageMock.Object.TryGetValueByUuid(Guid.NewGuid().ToString()), Ttl);

        // Assert
        resultValue.Should().Be(valueFromDataStorage);
        _memoryCache.TryGetValue(CacheKey, out valueFromDataStorage).Should().BeTrue();
        _redisCacheMock.Verify();
        _dataStorageMock.Verify();
    }

    [TestMethod]
    public void ShouldReturnNull_WhenValueDoesNotExistAnywhere()
    {
        // Arrange
        _redisCacheMock
            .Setup(rc => rc.TryGetValue(CacheKey, out It.Ref<object?>.IsAny))
            .Returns(false)
            .Verifiable(Times.Once);
        _redisCacheMock
            .Setup(rc => rc.Set(CacheKey, null, Ttl))
            .Verifiable(Times.Once);
        _dataStorageMock
            .Setup(ds => ds.TryGetValueByUuid(It.IsAny<string>()))
            .Returns(null!)
            .Verifiable(Times.Once);

        // Act
        var resultValue = _hybridCache.GetOrAdd(CacheKey,
            () => _dataStorageMock.Object.TryGetValueByUuid(Guid.NewGuid().ToString()), Ttl);

        // Assert
        resultValue.Should().BeNull();
        _memoryCache.TryGetValue(CacheKey, out var cachedValue).Should().BeTrue();
        cachedValue.Should().BeNull();
        _redisCacheMock.Verify();
        _dataStorageMock.Verify();
    }
}