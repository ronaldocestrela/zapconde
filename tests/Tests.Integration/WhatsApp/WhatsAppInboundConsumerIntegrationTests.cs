using BuildingBlocks.Infrastructure.Caching;
using BuildingBlocks.Shared.Caching;
using BuildingBlocks.Shared.Events;
using BuildingBlocks.Shared.MultiTenancy;
using FluentAssertions;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Modules.Identity.Application.Services;
using Modules.WhatsApp.Application.Services;
using Modules.WhatsApp.Domain.Entities;
using Modules.WhatsApp.Domain.Enums;
using Modules.WhatsApp.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace Tests.Integration.WhatsApp;

public sealed class WhatsAppInboundConsumerIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder()
        .WithImage("postgres:17-alpine")
        .WithDatabase("smartcondo_whatsapp_consumer_test")
        .WithUsername("smartcondo")
        .WithPassword("smartcondo")
        .Build();

    public async Task InitializeAsync()
    {
        await _postgresContainer.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _postgresContainer.DisposeAsync();
    }

    [Fact]
    public async Task ProcessInboundMessageAsync_Should_Update_Database_And_Status_In_PostgreSQL()
    {
        // Arrange
        var services = new ServiceCollection();
        var tenantService = new TestCurrentTenantService { TenantId = 1, CondoId = 10 };
        services.AddSingleton<ICurrentTenantService>(tenantService);
        services.AddLogging();
        services.AddSingleton<ICacheService, InMemoryCacheService>();
        services.AddSingleton<IDistributedLockService, InMemoryDistributedLockService>();

        var connectionString = _postgresContainer.GetConnectionString();
        services.AddDbContext<WhatsAppDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
        });

        var residentLookupMock = new Mock<IResidentLookupService>();
        residentLookupMock
            .Setup(r => r.FindByPhoneE164Async("+5575999999999", 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResidentLookupResultDto(1, 10, 55, Guid.NewGuid(), "Carlos Morador", "+5575999999999"));

        services.AddSingleton(residentLookupMock.Object);

        var publishEndpointMock = new Mock<IPublishEndpoint>();
        services.AddSingleton(publishEndpointMock.Object);

        services.AddScoped<IWhatsAppInboundProcessorService, WhatsAppInboundProcessorService>();

        await using var provider = services.BuildServiceProvider();

        int logId;
        using (var scope = provider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<WhatsAppDbContext>();
            await db.Database.EnsureCreatedAsync();

            var log = WhatsAppWebhookLog.Registrar(
                tenantId: 1,
                condoId: 10,
                instanceName: "condo-central",
                provider: WhatsAppProvider.EvolutionApi,
                messageId: "MSG_INTEGRATION_001",
                senderPhone: "+5575999999999",
                pushName: "Carlos Morador",
                messageType: WhatsAppMessageType.Text,
                messageText: "Gostaria de ver as áreas comuns disponíveis",
                mediaUrl: null,
                rawPayloadJson: "{}"
            );
            db.WebhookLogs.Add(log);
            await db.SaveChangesAsync();
            logId = log.Id;
        }

        // Act
        using (var scope = provider.CreateScope())
        {
            var processor = scope.ServiceProvider.GetRequiredService<IWhatsAppInboundProcessorService>();

            var @event = new WhatsAppMessageReceivedEvent
            {
                TenantId = 1,
                CondoId = 10,
                WebhookLogId = logId,
                InstanceName = "condo-central",
                MessageId = "MSG_INTEGRATION_001",
                SenderPhone = "+5575999999999",
                MessageText = "Gostaria de ver as áreas comuns disponíveis"
            };

            var result = await processor.ProcessInboundMessageAsync(@event);
            result.Success.Should().BeTrue();
            result.MoradorId.Should().Be(55);
            result.Status.Should().Be("Processed");
        }

        // Assert
        using (var scope = provider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<WhatsAppDbContext>();
            var updatedLog = await db.WebhookLogs.FindAsync(logId);
            updatedLog.Should().NotBeNull();
            updatedLog!.Status.Should().Be(WhatsAppWebhookStatus.Processed);
            updatedLog.MoradorId.Should().Be(55);
            updatedLog.ProcessedAt.Should().NotBeNull();
        }
    }

    private sealed class TestCurrentTenantService : ICurrentTenantService
    {
        public int? TenantId { get; set; }
        public int? CondoId { get; set; }
        public int? UserId { get; set; }
        public string? UserRole { get; set; }

        public void SetTenantId(int tenantId) => TenantId = tenantId;
        public void SetCondoId(int condoId) => CondoId = condoId;
        public void Clear()
        {
            TenantId = null;
            CondoId = null;
            UserId = null;
            UserRole = null;
        }
    }
}
