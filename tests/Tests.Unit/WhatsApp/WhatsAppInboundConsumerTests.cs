using BuildingBlocks.Shared.Caching;
using BuildingBlocks.Shared.Events;
using BuildingBlocks.Shared.MultiTenancy;
using FluentAssertions;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Modules.Identity.Application.Services;
using Modules.WhatsApp.Application.Consumers;
using Modules.WhatsApp.Application.Services;
using Modules.WhatsApp.Domain.Entities;
using Modules.WhatsApp.Domain.Enums;
using Modules.WhatsApp.Infrastructure.Persistence;

namespace Tests.Unit.WhatsApp;

public sealed class WhatsAppInboundConsumerTests
{
    private readonly Mock<ICurrentTenantService> _tenantServiceMock;
    private readonly Mock<IResidentLookupService> _residentLookupMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly Mock<IDistributedLockService> _lockServiceMock;
    private readonly Mock<IDistributedLockHandle> _lockHandleMock;
    private readonly Mock<IPublishEndpoint> _publishEndpointMock;
    private readonly Mock<ILogger<WhatsAppInboundProcessorService>> _loggerMock;
    private readonly Mock<ILogger<WhatsAppInboundConsumer>> _consumerLoggerMock;

    public WhatsAppInboundConsumerTests()
    {
        _tenantServiceMock = new Mock<ICurrentTenantService>();
        _tenantServiceMock.Setup(t => t.TenantId).Returns(1);
        _tenantServiceMock.Setup(t => t.CondoId).Returns(10);

        _residentLookupMock = new Mock<IResidentLookupService>();
        _cacheServiceMock = new Mock<ICacheService>();
        _lockServiceMock = new Mock<IDistributedLockService>();
        _lockHandleMock = new Mock<IDistributedLockHandle>();
        _publishEndpointMock = new Mock<IPublishEndpoint>();
        _loggerMock = new Mock<ILogger<WhatsAppInboundProcessorService>>();
        _consumerLoggerMock = new Mock<ILogger<WhatsAppInboundConsumer>>();

        _lockHandleMock.Setup(h => h.IsAcquired).Returns(true);
        _lockServiceMock
            .Setup(l => l.AcquireLockAsync(It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_lockHandleMock.Object);
    }

    private WhatsAppDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<WhatsAppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new WhatsAppDbContext(options, _tenantServiceMock.Object);
    }

    [Fact]
    public async Task ProcessInboundMessageAsync_Should_Use_Redis_Cache_When_Resident_Key_Exists()
    {
        // Arrange
        using var dbContext = CreateInMemoryDbContext();

        var webhookLog = WhatsAppWebhookLog.Registrar(
            tenantId: 1,
            condoId: 10,
            instanceName: "condo-central",
            provider: WhatsAppProvider.EvolutionApi,
            messageId: "MSG_CACHE_1001",
            senderPhone: "+5575999999999",
            pushName: "João Morador",
            messageType: WhatsAppMessageType.Text,
            messageText: "Gostaria de agendar o salão de festas",
            mediaUrl: null,
            rawPayloadJson: "{}"
        );
        dbContext.WebhookLogs.Add(webhookLog);
        await dbContext.SaveChangesAsync();

        var cachedItem = new MoradorCacheItem(
            TenantId: 1,
            CondoId: 10,
            MoradorId: 42,
            UserId: Guid.NewGuid(),
            TelefoneWhatsAppE164: "+5575999999999"
        );

        _cacheServiceMock
            .Setup(c => c.GetAsync<MoradorCacheItem>("wpp:morador:phone:+5575999999999", It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedItem);

        var service = new WhatsAppInboundProcessorService(
            dbContext,
            _residentLookupMock.Object,
            _cacheServiceMock.Object,
            _lockServiceMock.Object,
            _publishEndpointMock.Object,
            _loggerMock.Object
        );

        var @event = new WhatsAppMessageReceivedEvent
        {
            TenantId = 1,
            CondoId = 10,
            WebhookLogId = webhookLog.Id,
            InstanceName = "condo-central",
            MessageId = "MSG_CACHE_1001",
            SenderPhone = "+5575999999999",
            MessageText = "Gostaria de agendar o salão de festas"
        };

        // Act
        var result = await service.ProcessInboundMessageAsync(@event);

        // Assert
        result.Success.Should().BeTrue();
        result.MoradorId.Should().Be(42);
        result.CacheHit.Should().BeTrue();
        result.IsResidentIdentified.Should().BeTrue();

        _residentLookupMock.Verify(r => r.FindByPhoneE164Async(It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()), Times.Never);

        _publishEndpointMock.Verify(p => p.Publish(It.Is<WhatsAppMessageProcessedEvent>(e =>
            e.MoradorId == 42 && e.CacheHit == true && e.IsResidentIdentified == true
        ), It.IsAny<CancellationToken>()), Times.Once);

        var updatedLog = await dbContext.WebhookLogs.FindAsync(webhookLog.Id);
        updatedLog!.Status.Should().Be(WhatsAppWebhookStatus.Processed);
        updatedLog.MoradorId.Should().Be(42);
    }

    [Fact]
    public async Task ProcessInboundMessageAsync_Should_Query_Postgres_And_Populate_Redis_Cache_On_Cache_Miss()
    {
        // Arrange
        using var dbContext = CreateInMemoryDbContext();

        var webhookLog = WhatsAppWebhookLog.Registrar(
            tenantId: 1,
            condoId: 10,
            instanceName: "condo-central",
            provider: WhatsAppProvider.EvolutionApi,
            messageId: "MSG_MISS_2002",
            senderPhone: "+5575988887777",
            pushName: "Maria Moradora",
            messageType: WhatsAppMessageType.Text,
            messageText: "Segunda via do boleto por favor",
            mediaUrl: null,
            rawPayloadJson: "{}"
        );
        dbContext.WebhookLogs.Add(webhookLog);
        await dbContext.SaveChangesAsync();

        _cacheServiceMock
            .Setup(c => c.GetAsync<MoradorCacheItem>("wpp:morador:phone:+5575988887777", It.IsAny<CancellationToken>()))
            .ReturnsAsync((MoradorCacheItem?)null);

        var residentLookupDto = new ResidentLookupResultDto(
            TenantId: 1,
            CondoId: 10,
            MoradorId: 88,
            UserId: Guid.NewGuid(),
            Nome: "Maria Moradora",
            TelefoneWhatsAppE164: "+5575988887777"
        );

        _residentLookupMock
            .Setup(r => r.FindByPhoneE164Async("+5575988887777", 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(residentLookupDto);

        var service = new WhatsAppInboundProcessorService(
            dbContext,
            _residentLookupMock.Object,
            _cacheServiceMock.Object,
            _lockServiceMock.Object,
            _publishEndpointMock.Object,
            _loggerMock.Object
        );

        var @event = new WhatsAppMessageReceivedEvent
        {
            TenantId = 1,
            CondoId = 10,
            WebhookLogId = webhookLog.Id,
            InstanceName = "condo-central",
            MessageId = "MSG_MISS_2002",
            SenderPhone = "+5575988887777",
            MessageText = "Segunda via do boleto por favor"
        };

        // Act
        var result = await service.ProcessInboundMessageAsync(@event);

        // Assert
        result.Success.Should().BeTrue();
        result.MoradorId.Should().Be(88);
        result.CacheHit.Should().BeFalse();

        _cacheServiceMock.Verify(c => c.SetAsync(
            "wpp:morador:phone:+5575988887777",
            It.Is<MoradorCacheItem>(m => m.MoradorId == 88),
            TimeSpan.FromHours(24),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    [Fact]
    public async Task ProcessInboundMessageAsync_Should_Handle_Unregistered_Sender_Gracefully()
    {
        // Arrange
        using var dbContext = CreateInMemoryDbContext();

        var webhookLog = WhatsAppWebhookLog.Registrar(
            tenantId: 1,
            condoId: 10,
            instanceName: "condo-central",
            provider: WhatsAppProvider.EvolutionApi,
            messageId: "MSG_UNKNOWN_3003",
            senderPhone: "+5575977776666",
            pushName: "Visitante Desconhecido",
            messageType: WhatsAppMessageType.Text,
            messageText: "Quero entrar no condomínio",
            mediaUrl: null,
            rawPayloadJson: "{}"
        );
        dbContext.WebhookLogs.Add(webhookLog);
        await dbContext.SaveChangesAsync();

        _residentLookupMock
            .Setup(r => r.FindByPhoneE164Async("+5575977776666", 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ResidentLookupResultDto?)null);

        var service = new WhatsAppInboundProcessorService(
            dbContext,
            _residentLookupMock.Object,
            _cacheServiceMock.Object,
            _lockServiceMock.Object,
            _publishEndpointMock.Object,
            _loggerMock.Object
        );

        var @event = new WhatsAppMessageReceivedEvent
        {
            TenantId = 1,
            CondoId = 10,
            WebhookLogId = webhookLog.Id,
            InstanceName = "condo-central",
            MessageId = "MSG_UNKNOWN_3003",
            SenderPhone = "+5575977776666",
            MessageText = "Quero entrar no condomínio"
        };

        // Act
        var result = await service.ProcessInboundMessageAsync(@event);

        // Assert
        result.Success.Should().BeTrue();
        result.MoradorId.Should().BeNull();
        result.IsResidentIdentified.Should().BeFalse();

        var updatedLog = await dbContext.WebhookLogs.FindAsync(webhookLog.Id);
        updatedLog!.Status.Should().Be(WhatsAppWebhookStatus.Processed);
        updatedLog.MoradorId.Should().BeNull();
    }
}
