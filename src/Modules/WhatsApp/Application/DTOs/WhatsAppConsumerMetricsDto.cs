namespace Modules.WhatsApp.Application.DTOs;

public record WhatsAppConsumerMetricsDto(
    int TotalProcessed,
    int IdentifiedResidents,
    int UnidentifiedCount,
    int FailedCount,
    double ResidentIdentificationRate,
    double RedisCacheHitRate,
    double AverageLatencyMs);

public record WhatsAppInboundProcessingResultDto(
    bool Success,
    int WebhookLogId,
    int TenantId,
    int CondoId,
    int? MoradorId,
    bool IsResidentIdentified,
    bool CacheHit,
    string Status,
    string? ErrorMessage);
