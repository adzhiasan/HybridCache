using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Moq;

namespace HybridCache.Tests;

[TestClass]
public class HybridCacheTests
{
    private const string cacheKey = "Test:Cache";

    private Mock<IMemoryCache> _memoryCacheMock;
    private Mock<IRedisCache> _redisCacheMock;
    private Mock<IDataStorage> _dataStorageMock;

    private HybridCache _hybridCache;

    [TestInitialize]
    public void Init()
    {
        _memoryCacheMock = new Mock<IMemoryCache>();
        _redisCacheMock = new Mock<IRedisCache>();
        _dataStorageMock = new Mock<IDataStorage>();

        _hybridCache = new HybridCache(_memoryCacheMock.Object, _redisCacheMock.Object);
    }

    [TestMethod]
    public async Task ShouldReturnValueFromMemoryCache_WhenValueWithTargetKeyExistsThere()
    {
        // Arrange
        object cachedValue = "42";
        _memoryCacheMock
            .Setup(mc => mc.TryGetValue(cacheKey, out cachedValue))
            .Returns(true);

        // Act
        var resultValue = await _hybridCache
            .GetOrAddAsync(
                cacheKey,
                () => _dataStorageMock.Object.TryGetValueByUuid(Guid.NewGuid().ToString()));

        // Assert
        resultValue.Should().Be(cachedValue);
        _memoryCacheMock.Verify(mc => mc.TryGetValue(cacheKey, out It.Ref<object>.IsAny), Times.Once);
        _redisCacheMock.Verify(
            rc => rc.TryGetValue(It.IsAny<object>(), out It.Ref<object>.IsAny),
            Times.Never);
        _dataStorageMock.Verify(
            ds => ds.TryGetValueByUuid(It.IsAny<string>()),
            Times.Never);
    }

    [TestMethod]
    public async Task ShouldReturnValueFromRedisCache_WhenValueWithTargetKeyDoesNotExistInMemoryCache()
    {
        // Arrange
        object cachedValue = "42";
        _memoryCacheMock
            .Setup(mc => mc.TryGetValue(cacheKey, out It.Ref<object>.IsAny))
            .Returns(false);
        _redisCacheMock
            .Setup(rc => rc.TryGetValue(cacheKey, out cachedValue))
            .ReturnsAsync(true);

        // Act
        var resultValue = await _hybridCache.GetOrAddAsync(cacheKey,
            () => _dataStorageMock.Object.TryGetValueByUuid(Guid.NewGuid().ToString()));

        // Assert
        resultValue.Should().Be(cachedValue);

        _memoryCacheMock.Verify(mc => mc.TryGetValue(cacheKey, out It.Ref<object>.IsAny), Times.Once);
        _redisCacheMock.Verify(
            rc => rc.TryGetValue(It.IsAny<object>(), out It.Ref<object>.IsAny),
            Times.Once);
        _dataStorageMock.Verify(
            ds => ds.TryGetValueByUuid(It.IsAny<string>()),
            Times.Never);
    }

    [TestMethod]
    public async Task ShouldReturnValueFromDataStorage_WhenValueDoesNotExistNeitherInMemoryCacheNorInRedis()
    {
        // Arrange
        object valueFromDataStorage = "42";
        _memoryCacheMock
            .Setup(mc => mc.TryGetValue(cacheKey, out It.Ref<object>.IsAny))
            .Returns(false);
        _redisCacheMock
            .Setup(rc => rc.TryGetValue(cacheKey, out It.Ref<object>.IsAny))
            .ReturnsAsync(false);
        _dataStorageMock
            .Setup(ds => ds.TryGetValueByUuid(It.IsAny<string>()))
            .Returns(valueFromDataStorage);

        // Act
        var resultValue = await _hybridCache.GetOrAddAsync(cacheKey,
            () => _dataStorageMock.Object.TryGetValueByUuid(Guid.NewGuid().ToString()));

        // Assert
        resultValue.Should().Be(valueFromDataStorage);

        _memoryCacheMock.Verify(mc => mc.TryGetValue(cacheKey, out It.Ref<object>.IsAny), Times.Once);
        _redisCacheMock.Verify(
            rc => rc.TryGetValue(It.IsAny<object>(), out It.Ref<object>.IsAny),
            Times.Once);
        _dataStorageMock.Verify(
            ds => ds.TryGetValueByUuid(It.IsAny<string>()),
            Times.Once);
    }
}