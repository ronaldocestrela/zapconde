using BuildingBlocks.Shared.Events;
using BuildingBlocks.Shared.MultiTenancy;
using FluentAssertions;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Modules.WhatsApp.Application.DTOs;
using Modules.WhatsApp.Application.Services;
using Modules.WhatsApp.Domain.Enums;
using Modules.WhatsApp.Infrastructure.Persistence;

namespace Tests.Unit.WhatsApp;

public sealed class WhatsAppMessagingUnitTests
{
    private readonly Mock<ICurrentTenantService> _tenantServiceMock;
    private readonly Mock<IEvolutionPayloadParser> _parserMock;
    private readonly Mock<IPublishEndpoint> _publishEndpointMock;
    private readonly Mock<ILogger<WhatsAppApplicationService>> _loggerMock;

    public WhatsAppMessagingUnitTests()
    {
        _tenantServiceMock = new Mock<ICurrentTenantService>();
        _tenantServiceMock.Setup(t => t.TenantId).Returns(1);
        _tenantServiceMock.Setup(t => t.CondoId).Returns(10);

        _parserMock = new Mock<IEvolutionPayloadParser>();
        _publishEndpointMock = new Mock<IPublishEndpoint>();
        _loggerMock = new Mock<ILogger<WhatsAppApplicationService>>();
    }

    private WhatsAppDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<WhatsAppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new WhatsAppDbContext(options, _tenantServiceMock.Object);
    }

    [Fact]
    public async Task IngestEvolutionWebhookAsync_Should_Publish_WhatsAppMessageReceivedEvent_When_Valid_Payload_Received()
    {
        // Arrange
        using var dbContext = CreateInMemoryDbContext();

        var rawJson = @"{ ""event"": ""messages.upsert"", ""instance"": ""condo-central"" }";
        var parsedMessage = new WhatsAppInboundMessage(
            InstanceName: "condo-central",
            Provider: WhatsAppProvider.EvolutionApi,
            MessageId: "MSG_UNIT_1001",
            SenderPhone: "+5575999999999",
            PushName: "João Silva",
            MessageType: WhatsAppMessageType.Text,
            MessageText: "Olá, preciso do boleto de condomínio",
            MediaUrl: null,
            FromMe: false,
            Timestamp: DateTimeOffset.UtcNow,
            RawJson: rawJson
        );

        _parserMock.Setup(p => p.Parse(rawJson)).Returns(parsedMessage);

        var service = new WhatsAppApplicationService(
            dbContext,
            _tenantServiceMock.Object,
            _parserMock.Object,
            _publishEndpointMock.Object,
            _loggerMock.Object
        );

        // Act
        var result = await service.IngestEvolutionWebhookAsync(rawJson);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.IsDuplicate.Should().BeFalse();

        _publishEndpointMock.Verify(
            p => p.Publish(
                It.Is<WhatsAppMessageReceivedEvent>(e =>
                    e.MessageId == "MSG_UNIT_1001" &&
                    e.SenderPhone == "+5575999999999" &&
                    e.MessageText == "Olá, preciso do boleto de condomínio" &&
                    e.InstanceName == "condo-central" &&
                    e.TenantId == 1 &&
                    e.CondoId == 10
                ),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );

        var logInDb = await dbContext.WebhookLogs.FirstOrDefaultAsync(l => l.MessageId == "MSG_UNIT_1001");
        logInDb.Should().NotBeNull();
        logInDb!.Status.Should().Be(WhatsAppWebhookStatus.Received);
    }

    [Fact]
    public async Task IngestEvolutionWebhookAsync_Should_Not_Publish_Event_When_Message_Is_Duplicate()
    {
        // Arrange
        using var dbContext = CreateInMemoryDbContext();

        var existingLog = Modules.WhatsApp.Domain.Entities.WhatsAppWebhookLog.Registrar(
            tenantId: 1,
            condoId: 10,
            instanceName: "condo-central",
            provider: WhatsAppProvider.EvolutionApi,
            messageId: "MSG_DUPLICATE_2002",
            senderPhone: "+5575999999999",
            pushName: "Maria Santos",
            messageType: WhatsAppMessageType.Text,
            messageText: "Mensagem original",
            mediaUrl: null,
            rawPayloadJson: "{}"
        );
        dbContext.WebhookLogs.Add(existingLog);
        await dbContext.SaveChangesAsync();

        var rawJson = @"{ ""event"": ""messages.upsert"", ""instance"": ""condo-central"" }";
        var parsedMessage = new WhatsAppInboundMessage(
            InstanceName: "condo-central",
            Provider: WhatsAppProvider.EvolutionApi,
            MessageId: "MSG_DUPLICATE_2002",
            SenderPhone: "+5575999999999",
            PushName: "Maria Santos",
            MessageType: WhatsAppMessageType.Text,
            MessageText: "Mensagem duplicada",
            MediaUrl: null,
            FromMe: false,
            Timestamp: DateTimeOffset.UtcNow,
            RawJson: rawJson
        );

        _parserMock.Setup(p => p.Parse(rawJson)).Returns(parsedMessage);

        var service = new WhatsAppApplicationService(
            dbContext,
            _tenantServiceMock.Object,
            _parserMock.Object,
            _publishEndpointMock.Object,
            _loggerMock.Object
        );

        // Act
        var result = await service.IngestEvolutionWebhookAsync(rawJson);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.IsDuplicate.Should().BeTrue();

        _publishEndpointMock.Verify(
            p => p.Publish(It.IsAny<WhatsAppMessageReceivedEvent>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }
}
