using BuildingBlocks.Shared.Caching;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Tests.Architecture;

public class RedisConfigurationArchitectureTests
{
    [Fact]
    public void BuildingBlocksInfrastructure_ShouldReference_StackExchangeRedis()
    {
        // Arrange
        var assembly = Assembly.Load("BuildingBlocks.Infrastructure");

        // Act
        var referencedAssemblies = assembly.GetReferencedAssemblies();

        // Assert
        referencedAssemblies.Should().Contain(a => a.Name != null && a.Name.Contains("StackExchange.Redis"));
    }

    [Fact]
    public void BuildingBlocksShared_ShouldDefine_RedisAbstractionInterfaces()
    {
        // Assert
        typeof(ICacheService).Should().NotBeNull();
        typeof(IDistributedLockService).Should().NotBeNull();
        typeof(IDistributedLockHandle).Should().NotBeNull();
        typeof(IChatSessionService).Should().NotBeNull();
    }

    [Fact]
    public void SmartCondoApi_AppSettings_ShouldContain_RedisConnectionString()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        // Act
        var redisConnStr = configuration.GetConnectionString("Redis");

        // Assert
        redisConnStr.Should().NotBeNullOrWhiteSpace("ConnectionStrings:Redis deve estar configurada no appsettings.json");
    }
}
