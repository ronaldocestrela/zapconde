using FluentAssertions;
using Modules.WhatsApp.Domain.Entities;
using Modules.WhatsApp.Domain.Enums;
using Modules.WhatsApp.Domain.Exceptions;
using Xunit;

namespace Tests.Unit.WhatsApp;

public class WhatsAppDomainTests
{
    [Fact]
    public void Should_CreateWhatsAppWebhookLog_When_ValidParameters()
    {
        // Act
        var log = WhatsAppWebhookLog.Registrar(
            tenantId: 1,
            condoId: 10,
            instanceName: "condo-central",
            provider: WhatsAppProvider.EvolutionApi,
            messageId: "MSG123456",
            senderPhone: "5575999999999",
            pushName: "João da Silva",
            messageType: WhatsAppMessageType.Text,
            messageText: "Segunda via do boleto por favor",
            mediaUrl: null,
            rawPayloadJson: "{\"event\":\"messages.upsert\"}"
        );

        // Assert
        log.Should().NotBeNull();
        log.TenantId.Should().Be(1);
        log.CondoId.Should().Be(10);
        log.InstanceName.Should().Be("condo-central");
        log.Provider.Should().Be(WhatsAppProvider.EvolutionApi);
        log.MessageId.Should().Be("MSG123456");
        log.SenderPhone.Should().Be("+5575999999999");
        log.Status.Should().Be(WhatsAppWebhookStatus.Received);
    }

    [Fact]
    public void Should_ThrowDomainException_When_TenantIdIsInvalid()
    {
        // Act
        Action act = () => WhatsAppWebhookLog.Registrar(
            tenantId: 0,
            condoId: 10,
            instanceName: "condo-central",
            provider: WhatsAppProvider.EvolutionApi,
            messageId: "MSG123456",
            senderPhone: "5575999999999",
            pushName: "João da Silva",
            messageType: WhatsAppMessageType.Text,
            messageText: "Teste",
            mediaUrl: null,
            rawPayloadJson: "{}"
        );

        // Assert
        act.Should().Throw<WhatsAppDomainException>()
           .WithMessage("*TenantId é obrigatório*");
    }

    [Fact]
    public void Should_MarkAsProcessed_Correctly()
    {
        // Arrange
        var log = WhatsAppWebhookLog.Registrar(
            tenantId: 1, condoId: 1, instanceName: "inst1", provider: WhatsAppProvider.EvolutionApi,
            messageId: "M1", senderPhone: "5575999999999", pushName: "User",
            messageType: WhatsAppMessageType.Text, messageText: "Text", mediaUrl: null, rawPayloadJson: "{}"
        );

        // Act
        log.MarcarComoProcessado();

        // Assert
        log.Status.Should().Be(WhatsAppWebhookStatus.Processed);
        log.ProcessedAt.Should().NotBeNull();
    }

    [Fact]
    public void Should_CreateWhatsAppInstanceConfig_When_ValidParameters()
    {
        // Act
        var instance = WhatsAppInstanceConfig.Criar(
            tenantId: 1,
            condoId: 2,
            instanceName: "condo-central",
            provider: WhatsAppProvider.EvolutionApi,
            baseUrl: "https://api.evolution.com",
            apiKey: "SECRET_KEY_123",
            webhookSecret: "TOKEN_WEBHOOK"
        );

        // Assert
        instance.Should().NotBeNull();
        instance.InstanceName.Should().Be("condo-central");
        instance.BaseUrl.Should().Be("https://api.evolution.com");
        instance.ApiKey.Should().Be("SECRET_KEY_123");
        instance.IsActive.Should().BeTrue();
        instance.Status.Should().Be("Connected");
    }
}
