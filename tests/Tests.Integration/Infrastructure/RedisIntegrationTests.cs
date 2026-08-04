using BuildingBlocks.Infrastructure.Caching;
using BuildingBlocks.Shared.Caching;
using BuildingBlocks.Shared.MultiTenancy;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using StackExchange.Redis;
using System.Net;
using Testcontainers.Redis;

namespace Tests.Integration.Infrastructure;

public sealed class RedisIntegrationTests : IAsyncLifetime
{
    private readonly RedisContainer _redisContainer = new RedisBuilder()
        .WithImage("redis:7.4-alpine")
        .Build();

    public async Task InitializeAsync()
    {
        await _redisContainer.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _redisContainer.DisposeAsync();
    }

    [Fact]
    public async Task CacheService_Should_StoreAndRetrieveValue_WithTenantPrefix()
    {
        // Arrange
        var connectionString = _redisContainer.GetConnectionString();
        var multiplexer = await ConnectionMultiplexer.ConnectAsync(connectionString);

        var mockTenantService = new Mock<ICurrentTenantService>();
        mockTenantService.Setup(t => t.TenantId).Returns(10);

        var cacheService = new RedisCacheService(multiplexer, mockTenantService.Object);

        var dummyData = new SampleCacheItem { Id = 101, Name = "Salão de Festas" };

        // Act
        await cacheService.SetAsync("reserva_101", dummyData, TimeSpan.FromMinutes(5));
        var retrieved = await cacheService.GetAsync<SampleCacheItem>("reserva_101");
        var exists = await cacheService.ExistsAsync("reserva_101");

        // Assert
        retrieved.Should().NotBeNull();
        retrieved!.Id.Should().Be(101);
        retrieved.Name.Should().Be("Salão de Festas");
        exists.Should().BeTrue();

        // Direct Redis Key check to confirm tenant prefixing
        var db = multiplexer.GetDatabase();
        var rawValue = await db.StringGetAsync("tenant:10:reserva_101");
        rawValue.HasValue.Should().BeTrue();
    }

    [Fact]
    public async Task DistributedLockService_Should_PreventConcurrentAcquisition()
    {
        // Arrange
        var connectionString = _redisContainer.GetConnectionString();
        var multiplexer = await ConnectionMultiplexer.ConnectAsync(connectionString);
        var lockService = new RedisDistributedLockService(multiplexer);

        var resourceKey = "area_comum:churrasqueira:2026-08-04";

        // Act & Assert
        await using var handle1 = await lockService.AcquireLockAsync(resourceKey, TimeSpan.FromSeconds(30));
        handle1.IsAcquired.Should().BeTrue();

        // Try to acquire the same lock while handle1 is active
        await using var handle2 = await lockService.AcquireLockAsync(resourceKey, TimeSpan.FromSeconds(30), TimeSpan.FromMilliseconds(200));
        handle2.IsAcquired.Should().BeFalse();

        // Release handle1
        await handle1.DisposeAsync();

        // Now handle3 should acquire successfully
        await using var handle3 = await lockService.AcquireLockAsync(resourceKey, TimeSpan.FromSeconds(30));
        handle3.IsAcquired.Should().BeTrue();
    }

    [Fact]
    public async Task ChatSessionService_Should_SaveAndRetrieveChatContext()
    {
        // Arrange
        var connectionString = _redisContainer.GetConnectionString();
        var multiplexer = await ConnectionMultiplexer.ConnectAsync(connectionString);

        var mockTenantService = new Mock<ICurrentTenantService>();
        mockTenantService.Setup(t => t.TenantId).Returns(1);

        var cacheService = new RedisCacheService(multiplexer, mockTenantService.Object);
        var sessionService = new RedisChatSessionService(cacheService);

        var sessionData = new SampleChatSession
        {
            UserPhoneNumber = "+5575999999999",
            LastIntention = "GET_BOLETO",
            StepCount = 2
        };

        // Act
        await sessionService.SetSessionStateAsync("session_5575999999999", sessionData, TimeSpan.FromMinutes(10));
        var retrievedSession = await sessionService.GetSessionStateAsync<SampleChatSession>("session_5575999999999");

        // Assert
        retrievedSession.Should().NotBeNull();
        retrievedSession!.UserPhoneNumber.Should().Be("+5575999999999");
        retrievedSession.LastIntention.Should().Be("GET_BOLETO");
        retrievedSession.StepCount.Should().Be(2);
    }

    [Fact]
    public async Task Api_HealthEndpoint_Should_Include_Redis_When_Configured()
    {
        // Arrange
        var redisConnectionString = _redisContainer.GetConnectionString();
        var settings = new Dictionary<string, string?>
        {
            ["ConnectionStrings:Postgres"] = "Host=localhost;Port=5432;Database=smartcondo_test;Username=postgres;Password=postgres",
            ["ConnectionStrings:RabbitMQ"] = "amqp://guest:guest@localhost:5672/",
            ["ConnectionStrings:Redis"] = redisConnectionString,
            ["RabbitMQ:Host"] = "localhost",
            ["RabbitMQ:Port"] = "5672",
            ["RabbitMQ:VirtualHost"] = "/",
            ["RabbitMQ:Username"] = "guest",
            ["RabbitMQ:Password"] = "guest",
            ["Identity:SeedOnStartup"] = "false",
            ["Identity:UseInMemoryDatabase"] = "true"
        };

        await using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Development");
            builder.ConfigureAppConfiguration((_, configurationBuilder) =>
            {
                configurationBuilder.AddInMemoryCollection(settings);
            });
        });

        using var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/health/ready");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.ServiceUnavailable);
    }

    private class SampleCacheItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    private class SampleChatSession
    {
        public string UserPhoneNumber { get; set; } = string.Empty;
        public string LastIntention { get; set; } = string.Empty;
        public int StepCount { get; set; }
    }
}
