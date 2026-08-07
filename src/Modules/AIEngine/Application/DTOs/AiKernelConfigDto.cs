using Modules.AIEngine.Domain.Enums;

namespace Modules.AIEngine.Application.DTOs;

public record AiKernelConfigDto(
    int Id,
    int TenantId,
    int CondoId,
    AiProvider Provider,
    string ModelId,
    string EmbeddingModelId,
    string MaskedApiKey,
    string? Endpoint,
    string? OrgId,
    double Temperature,
    int MaxTokens,
    bool IsActive,
    DateTimeOffset CriadoEm,
    DateTimeOffset? AtualizadoEm);
