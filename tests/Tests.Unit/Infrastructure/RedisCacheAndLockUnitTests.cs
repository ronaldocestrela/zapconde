using BuildingBlocks.Infrastructure.Caching;
using BuildingBlocks.Shared.Caching;
using BuildingBlocks.Shared.MultiTenancy;
using FluentAssertions;
using Moq;
using StackExchange.Redis;
using System.Text.Json;

namespace Tests.Unit.Infrastructure;

public class RedisCacheAndLockUnitTests
{
    [Fact]
    public async Task RedisCacheService_BuildTenantKey_ShouldPrefixWithTenantId_WhenTenantIsSet()
    {
        // Arrange
        var mockTenantService = new Mock<ICurrentTenantService>();
        mockTenantService.Setup(t => t.TenantId).Returns(42);

        var mockMultiplexer = new Mock<IConnectionMultiplexer>();
        var mockDatabase = new Mock<IDatabase>();
        mockMultiplexer.Setup(m => m.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(mockDatabase.Object);

        var cacheService = new RedisCacheService(mockMultiplexer.Object, mockTenantService.Object);

        // Act
        await cacheService.SetAsync("minha_chave", new { Nome = "Teste" });

        // Assert
        mockDatabase.Verify(d => d.StringSetAsync(
            It.Is<RedisKey>(k => k == "tenant:42:minha_chave"),
            It.IsAny<RedisValue>(),
            It.IsAny<TimeSpan?>(),
            It.IsAny<bool>(),
            It.IsAny<When>(),
            It.IsAny<CommandFlags>()
        ), Times.Once);
    }

    [Fact]
    public async Task RedisCacheService_BuildTenantKey_ShouldPrefixWithGlobal_WhenTenantIsNotSet()
    {
        // Arrange
        var mockTenantService = new Mock<ICurrentTenantService>();
        mockTenantService.Setup(t => t.TenantId).Returns((int?)null);

        var mockMultiplexer = new Mock<IConnectionMultiplexer>();
        var mockDatabase = new Mock<IDatabase>();
        mockMultiplexer.Setup(m => m.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(mockDatabase.Object);

        var cacheService = new RedisCacheService(mockMultiplexer.Object, mockTenantService.Object);

        // Act
        await cacheService.SetAsync("config_sistema", new { Versao = "1.0" });

        // Assert
        mockDatabase.Verify(d => d.StringSetAsync(
            It.Is<RedisKey>(k => k == "global:config_sistema"),
            It.IsAny<RedisValue>(),
            It.IsAny<TimeSpan?>(),
            It.IsAny<bool>(),
            It.IsAny<When>(),
            It.IsAny<CommandFlags>()
        ), Times.Once);
    }

    [Fact]
    public async Task RedisDistributedLockHandle_DisposeAsync_ShouldReleaseLockInDatabase()
    {
        // Arrange
        var mockDatabase = new Mock<IDatabase>();
        mockDatabase.Setup(d => d.LockReleaseAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        var handle = new RedisDistributedLockHandle(mockDatabase.Object, "lock:area:1", "guid-value-123", isAcquired: true);

        // Act
        await handle.DisposeAsync();

        // Assert
        handle.IsAcquired.Should().BeFalse();
        mockDatabase.Verify(d => d.LockReleaseAsync("lock:area:1", "guid-value-123", It.IsAny<CommandFlags>()), Times.Once);
    }
}
