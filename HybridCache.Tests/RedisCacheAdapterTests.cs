using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Moq;

namespace HybridCache.Tests;

[TestClass]
public class RedisCacheAdapterTests
{
    private const string CacheKey = "Test:Cache";

    [TestMethod]
    public void ShouldReturnTrueAndSpecifyValue_WhenExists()
    {
        // Arrange
        var distributedCacheMock = new Mock<IDistributedCache>();
        distributedCacheMock
            .Setup(dc => dc.Get(CacheKey))
            .Returns("{\"Value\": \"test\"}"u8.ToArray());

        // Act
        var result =
            new RedisCacheAdapter(distributedCacheMock.Object).TryGetValue<Test>(CacheKey, out var valueFromCache);

        // Assert
        result.Should().BeTrue();
        valueFromCache.Value.Should().Be("test");
    }

    [TestMethod]
    public void ShouldReturnTrue_WhenNullValueExists()
    {
        // Arrange
        var distributedCacheMock = new Mock<IDistributedCache>();
        distributedCacheMock
            .Setup(dc => dc.Get(CacheKey))
            .Returns("__NULL__"u8.ToArray());

        // Act
        var result =
            new RedisCacheAdapter(distributedCacheMock.Object).TryGetValue<Test>(CacheKey, out var valueFromCache);

        // Assert
        result.Should().BeTrue();
        valueFromCache.Should().BeNull();
    }

    [TestMethod]
    public void ShouldReturnFalse_WhenValueDoesNotExist()
    {
        // Arrange
        var distributedCacheMock = new Mock<IDistributedCache>();
        distributedCacheMock
            .Setup(dc => dc.Get(CacheKey))
            .Returns((byte[])null);

        // Act
        var result =
            new RedisCacheAdapter(distributedCacheMock.Object).TryGetValue<Test>(CacheKey, out var valueFromCache);

        // Assert
        result.Should().BeFalse();
        valueFromCache.Should().BeNull();
    }

    [TestMethod]
    public async Task ShouldSetNullValue_WhenValueIsNull()
    {
        // Arrange
        object? value = null;
        var distributedCacheMock = new Mock<IDistributedCache>();

        // Act
        await 
            new RedisCacheAdapter(distributedCacheMock.Object).SetAsync(CacheKey, value, TimeSpan.FromMinutes(1));

        // Assert
        distributedCacheMock.Verify(dc => dc.SetAsync(CacheKey, Encoding.UTF8.GetBytes("__NULL__"), It.IsAny<DistributedCacheEntryOptions>(), CancellationToken.None));
    }

    [TestMethod] public async Task ShouldSetSerializedValue_WhenValueIsNotNull()
    {
        // Arrange
        object? value = new Test("test");
        var serializedValue = JsonSerializer.Serialize(value);
        var distributedCacheMock = new Mock<IDistributedCache>(); ;

        // Act
        await 
            new RedisCacheAdapter(distributedCacheMock.Object).SetAsync(CacheKey, value, TimeSpan.FromMinutes(1));

        // Assert
        distributedCacheMock.Verify(dc => dc.SetAsync(CacheKey, Encoding.UTF8.GetBytes(serializedValue), It.IsAny<DistributedCacheEntryOptions>(), CancellationToken.None));
    }

    private record Test(string Value);
}