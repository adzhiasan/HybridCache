using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace HybridCache.Tests;

[TestClass]
public class HybridCacheTests
{
    private const string cacheKey = "Test:Cache";
    private static TimeSpan ttl = TimeSpan.FromSeconds(10);

    private readonly IMemoryCache _memoryCache;
    private Mock<IRedisCacheAdapter> _redisCacheMock;
    private Mock<IDataStorage> _dataStorageMock;

    private HybridCache _hybridCache;

    public HybridCacheTests()
    {
        var services = new ServiceCollection();
        services.AddMemoryCache();
        var serviceProvider = services.BuildServiceProvider();

        _memoryCache = serviceProvider.GetService<IMemoryCache>();
    }

    [TestInitialize]
    public void Init()
    {
        _redisCacheMock = new Mock<IRedisCacheAdapter>();
        _dataStorageMock = new Mock<IDataStorage>();

        _hybridCache = new HybridCache(_memoryCache, _redisCacheMock.Object);
    }

    [TestMethod]
    public void ShouldReturnValueFromMemoryCache_WhenValueWithTargetKeyExistsThere()
    {
        // Arrange
        object cachedValue = "42";
        _memoryCache.Set(cacheKey, cachedValue);

        // Act
        var resultValue = _hybridCache
            .GetOrAdd(
                cacheKey,
                () => _dataStorageMock.Object.TryGetValueByUuid(Guid.NewGuid().ToString()),
                ttl);

        // Assert
        resultValue.Should().Be(cachedValue);
    }

    [TestMethod]
    public void ShouldReturnValueFromRedisCache_WhenValueWithTargetKeyDoesNotExistInMemoryCache()
    {
        // Arrange
        object cachedValue = "42";

        _redisCacheMock
            .Setup(rc => rc.TryGetValue(cacheKey, out cachedValue))
            .Returns(true)
            .Verifiable(Times.Once);

        // Act
        var resultValue = _hybridCache.GetOrAdd(cacheKey,
            () => _dataStorageMock.Object.TryGetValueByUuid(Guid.NewGuid().ToString()),
            ttl);

        // Assert
        resultValue.Should().Be(cachedValue);
        _memoryCache.TryGetValue(cacheKey, out cachedValue).Should().BeTrue();
        _redisCacheMock.Verify();
        _dataStorageMock.Verify(
            ds => ds.TryGetValueByUuid(It.IsAny<string>()),
            Times.Never);
    }


    [TestMethod]
    public void ShouldReturnValueFromDataStorage_WhenValueDoesNotExistNeitherInMemoryCacheNorInRedis()
    {
        // Arrange
        object valueFromDataStorage = "42";
        _redisCacheMock
            .Setup(rc => rc.TryGetValue(cacheKey, out It.Ref<object>.IsAny))
            .Returns(false).Verifiable(Times.Once);
        _redisCacheMock
            .Setup(rc => rc.Set(cacheKey, valueFromDataStorage, ttl))
            .Verifiable(Times.Once);
        _dataStorageMock
            .Setup(ds => ds.TryGetValueByUuid(It.IsAny<string>()))
            .Returns(valueFromDataStorage)
            .Verifiable(Times.Once);

        // Act
        var resultValue = _hybridCache.GetOrAdd(cacheKey,
            () => _dataStorageMock.Object.TryGetValueByUuid(Guid.NewGuid().ToString()), ttl);

        // Assert
        resultValue.Should().Be(valueFromDataStorage);
        _memoryCache.TryGetValue(cacheKey, out valueFromDataStorage).Should().BeTrue();
        _redisCacheMock.Verify();
        _dataStorageMock.Verify();
    }

    [TestMethod]
    public void ShouldReturnNull_WhenValueDoesNotExistAnywhere()
    {
        // Arrange
        _redisCacheMock
            .Setup(rc => rc.TryGetValue(cacheKey, out It.Ref<object>.IsAny))
            .Returns(false)
            .Verifiable(Times.Once);
        _redisCacheMock
            .Setup(rc => rc.Set(cacheKey, null, ttl))
            .Verifiable(Times.Once);
        _dataStorageMock
            .Setup(ds => ds.TryGetValueByUuid(It.IsAny<string>()))
            .Returns(null)
            .Verifiable(Times.Once);

        // Act
        var resultValue = _hybridCache.GetOrAdd(cacheKey,
            () => _dataStorageMock.Object.TryGetValueByUuid(Guid.NewGuid().ToString()), ttl);

        // Assert
        resultValue.Should().BeNull();
        _memoryCache.TryGetValue(cacheKey, out var cachedValue).Should().BeTrue();
        cachedValue.Should().BeNull();
        _redisCacheMock.Verify();
        _dataStorageMock.Verify();
    }
}