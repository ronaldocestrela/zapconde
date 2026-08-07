using System.Text.Json.Serialization;

namespace Modules.WhatsApp.Application.DTOs;

/// <summary>
/// Mapeamento do JSON recebido via Webhook da Evolution API (evento messages.upsert).
/// </summary>
public record EvolutionApiPayload
{
    [JsonPropertyName("event")]
    public string? Event { get; init; }

    [JsonPropertyName("instance")]
    public string? Instance { get; init; }

    [JsonPropertyName("data")]
    public EvolutionData? Data { get; init; }

    [JsonPropertyName("destination")]
    public string? Destination { get; init; }

    [JsonPropertyName("date_time")]
    public string? DateTime { get; init; }

    [JsonPropertyName("sender")]
    public string? Sender { get; init; }

    [JsonPropertyName("server_url")]
    public string? ServerUrl { get; init; }

    [JsonPropertyName("apikey")]
    public string? ApiKey { get; init; }
}

public record EvolutionData
{
    [JsonPropertyName("key")]
    public EvolutionKey? Key { get; init; }

    [JsonPropertyName("pushName")]
    public string? PushName { get; init; }

    [JsonPropertyName("message")]
    public EvolutionMessage? Message { get; init; }

    [JsonPropertyName("messageType")]
    public string? MessageType { get; init; }

    [JsonPropertyName("messageTimestamp")]
    public long? MessageTimestamp { get; init; }

    [JsonPropertyName("owner")]
    public string? Owner { get; init; }
}

public record EvolutionKey
{
    [JsonPropertyName("remoteJid")]
    public string? RemoteJid { get; init; }

    [JsonPropertyName("fromMe")]
    public bool FromMe { get; init; }

    [JsonPropertyName("id")]
    public string? Id { get; init; }

    [JsonPropertyName("participant")]
    public string? Participant { get; init; }
}

public record EvolutionMessage
{
    [JsonPropertyName("conversation")]
    public string? Conversation { get; init; }

    [JsonPropertyName("extendedTextMessage")]
    public EvolutionExtendedText? ExtendedTextMessage { get; init; }

    [JsonPropertyName("imageMessage")]
    public EvolutionMediaMessage? ImageMessage { get; init; }

    [JsonPropertyName("audioMessage")]
    public EvolutionMediaMessage? AudioMessage { get; init; }

    [JsonPropertyName("documentMessage")]
    public EvolutionMediaMessage? DocumentMessage { get; init; }
}

public record EvolutionExtendedText
{
    [JsonPropertyName("text")]
    public string? Text { get; init; }
}

public record EvolutionMediaMessage
{
    [JsonPropertyName("caption")]
    public string? Caption { get; init; }

    [JsonPropertyName("url")]
    public string? Url { get; init; }

    [JsonPropertyName("mimetype")]
    public string? MimeType { get; init; }
}
