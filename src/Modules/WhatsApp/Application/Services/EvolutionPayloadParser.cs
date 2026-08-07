using System.Text.Json;
using Modules.WhatsApp.Application.DTOs;
using Modules.WhatsApp.Domain.Enums;

namespace Modules.WhatsApp.Application.Services;

public interface IEvolutionPayloadParser
{
    WhatsAppInboundMessage? Parse(string rawJson);
}

public class EvolutionPayloadParser : IEvolutionPayloadParser
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public WhatsAppInboundMessage? Parse(string rawJson)
    {
        if (string.IsNullOrWhiteSpace(rawJson))
        {
            return null;
        }

        try
        {
            var payload = JsonSerializer.Deserialize<EvolutionApiPayload>(rawJson, JsonOptions);
            if (payload == null || payload.Data == null)
            {
                return null;
            }

            var instanceName = payload.Instance ?? payload.Data.Owner ?? "unknown-instance";
            var key = payload.Data.Key;
            var messageId = key?.Id ?? Guid.NewGuid().ToString("N");
            var rawJid = key?.RemoteJid ?? payload.Sender ?? string.Empty;
            var phone = ExtractPhoneFromJid(rawJid);

            var pushName = payload.Data.PushName;
            var fromMe = key?.FromMe ?? false;

            var msgObj = payload.Data.Message;
            string? text = null;
            string? mediaUrl = null;
            var msgType = WhatsAppMessageType.Unknown;

            if (msgObj != null)
            {
                if (!string.IsNullOrWhiteSpace(msgObj.Conversation))
                {
                    text = msgObj.Conversation;
                    msgType = WhatsAppMessageType.Text;
                }
                else if (msgObj.ExtendedTextMessage != null && !string.IsNullOrWhiteSpace(msgObj.ExtendedTextMessage.Text))
                {
                    text = msgObj.ExtendedTextMessage.Text;
                    msgType = WhatsAppMessageType.Text;
                }
                else if (msgObj.ImageMessage != null)
                {
                    text = msgObj.ImageMessage.Caption;
                    mediaUrl = msgObj.ImageMessage.Url;
                    msgType = WhatsAppMessageType.Image;
                }
                else if (msgObj.AudioMessage != null)
                {
                    mediaUrl = msgObj.AudioMessage.Url;
                    msgType = WhatsAppMessageType.Audio;
                }
                else if (msgObj.DocumentMessage != null)
                {
                    text = msgObj.DocumentMessage.Caption;
                    mediaUrl = msgObj.DocumentMessage.Url;
                    msgType = WhatsAppMessageType.Document;
                }
            }

            if (msgType == WhatsAppMessageType.Unknown && !string.IsNullOrWhiteSpace(payload.Data.MessageType))
            {
                msgType = MapMessageTypeString(payload.Data.MessageType);
            }

            var timestamp = payload.Data.MessageTimestamp.HasValue
                ? DateTimeOffset.FromUnixTimeSeconds(payload.Data.MessageTimestamp.Value)
                : DateTimeOffset.UtcNow;

            return new WhatsAppInboundMessage(
                InstanceName: instanceName,
                Provider: WhatsAppProvider.EvolutionApi,
                MessageId: messageId,
                SenderPhone: phone,
                PushName: pushName,
                MessageType: msgType,
                MessageText: text,
                MediaUrl: mediaUrl,
                FromMe: fromMe,
                Timestamp: timestamp,
                RawJson: rawJson
            );
        }
        catch
        {
            return null;
        }
    }

    private static string ExtractPhoneFromJid(string jid)
    {
        if (string.IsNullOrWhiteSpace(jid))
        {
            return string.Empty;
        }

        var atIndex = jid.IndexOf('@');
        var clean = atIndex > 0 ? jid[..atIndex] : jid;
        var digitsOnly = new string(clean.Where(char.IsDigit).ToArray());

        if (string.IsNullOrWhiteSpace(digitsOnly))
        {
            return jid;
        }

        return digitsOnly.StartsWith('+') ? digitsOnly : "+" + digitsOnly;
    }

    private static WhatsAppMessageType MapMessageTypeString(string typeStr)
    {
        return typeStr.ToLowerInvariant() switch
        {
            "conversation" or "extendedtextmessage" => WhatsAppMessageType.Text,
            "imagemessage" => WhatsAppMessageType.Image,
            "audiomessage" => WhatsAppMessageType.Audio,
            "documentmessage" => WhatsAppMessageType.Document,
            "locationmessage" => WhatsAppMessageType.Location,
            "contactmessage" => WhatsAppMessageType.Contact,
            "stickermessage" => WhatsAppMessageType.Sticker,
            _ => WhatsAppMessageType.Unknown
        };
    }
}
