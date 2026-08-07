namespace Modules.AIEngine.Application.DTOs;

public record AiExecutionLogDto(
    long Id,
    int TenantId,
    string Prompt,
    string Response,
    string ModelUsed,
    int PromptTokens,
    int CompletionTokens,
    int TotalTokens,
    long DurationMs,
    bool Success,
    string? ErrorMessage,
    DateTimeOffset ExecutedAt);

public record AiSummaryDto(
    bool Configurada,
    string Provider,
    string ModelId,
    int TotalExecucoes,
    int ExecucoesComSucesso,
    int ExecucoesComFalha,
    long TotalTokensConsumidos,
    double LatenciaMediaMs);
