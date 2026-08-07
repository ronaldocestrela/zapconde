using FluentAssertions;
using Modules.WhatsApp.Application.Services;
using Modules.WhatsApp.Domain.Enums;
using Xunit;

namespace Tests.Unit.WhatsApp;

public class EvolutionPayloadParserTests
{
    private readonly EvolutionPayloadParser _parser = new();

    [Fact]
    public void Should_ParseTextMessage_Successfully()
    {
        // Arrange
        var json = """
        {
          "event": "messages.upsert",
          "instance": "condo-central",
          "data": {
            "key": {
              "remoteJid": "5575999999999@s.whatsapp.net",
              "fromMe": false,
              "id": "MSG_EVO_001"
            },
            "pushName": "Roberto Morador",
            "message": {
              "conversation": "Olá, preciso do código PIX do boleto"
            },
            "messageType": "conversation",
            "messageTimestamp": 1723000000
          }
        }
        """;

        // Act
        var result = _parser.Parse(json);

        // Assert
        result.Should().NotBeNull();
        result!.InstanceName.Should().Be("condo-central");
        result.Provider.Should().Be(WhatsAppProvider.EvolutionApi);
        result.MessageId.Should().Be("MSG_EVO_001");
        result.SenderPhone.Should().Be("+5575999999999");
        result.PushName.Should().Be("Roberto Morador");
        result.MessageType.Should().Be(WhatsAppMessageType.Text);
        result.MessageText.Should().Be("Olá, preciso do código PIX do boleto");
        result.FromMe.Should().BeFalse();
    }

    [Fact]
    public void Should_ParseExtendedTextMessage_Successfully()
    {
        // Arrange
        var json = """
        {
          "event": "messages.upsert",
          "instance": "condo-central",
          "data": {
            "key": {
              "remoteJid": "5575988888888@s.whatsapp.net",
              "fromMe": false,
              "id": "MSG_EVO_002"
            },
            "pushName": "Ana Maria",
            "message": {
              "extendedTextMessage": {
                "text": "Quero autorizar um visitante para hoje"
              }
            }
          }
        }
        """;

        // Act
        var result = _parser.Parse(json);

        // Assert
        result.Should().NotBeNull();
        result!.SenderPhone.Should().Be("+5575988888888");
        result.MessageType.Should().Be(WhatsAppMessageType.Text);
        result.MessageText.Should().Be("Quero autorizar um visitante para hoje");
    }

    [Fact]
    public void Should_ParseImageMessage_WithCaptionAndUrl()
    {
        // Arrange
        var json = """
        {
          "event": "messages.upsert",
          "instance": "condo-central",
          "data": {
            "key": {
              "remoteJid": "5575977777777@s.whatsapp.net",
              "fromMe": false,
              "id": "MSG_EVO_003"
            },
            "pushName": "Carlos Zelador",
            "message": {
              "imageMessage": {
                "caption": "Foto da lâmpada queimada na garagem",
                "url": "https://storage.evolution.com/img123.jpeg"
              }
            }
          }
        }
        """;

        // Act
        var result = _parser.Parse(json);

        // Assert
        result.Should().NotBeNull();
        result!.MessageType.Should().Be(WhatsAppMessageType.Image);
        result.MessageText.Should().Be("Foto da lâmpada queimada na garagem");
        result.MediaUrl.Should().Be("https://storage.evolution.com/img123.jpeg");
    }

    [Fact]
    public void Should_ReturnNull_When_PayloadIsInvalidOrEmpty()
    {
        // Act & Assert
        _parser.Parse("").Should().BeNull();
        _parser.Parse("{ invalid json }").Should().BeNull();
    }
}
