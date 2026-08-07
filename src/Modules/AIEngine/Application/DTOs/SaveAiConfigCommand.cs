using Modules.AIEngine.Domain.Enums;

namespace Modules.AIEngine.Application.DTOs;

public record SaveAiConfigCommand(
    AiProvider Provider,
    string ModelId,
    string? EmbeddingModelId,
    string? ApiKey,
    string? Endpoint,
    string? OrgId,
    double Temperature,
    int MaxTokens,
    bool IsActive);
