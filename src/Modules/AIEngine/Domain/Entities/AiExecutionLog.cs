using BuildingBlocks.Shared.MultiTenancy;
using Modules.AIEngine.Domain.Exceptions;

namespace Modules.AIEngine.Domain.Entities;

/// <summary>
/// Log de auditoria de execução de prompts no Semantic Kernel.
/// </summary>
public class AiExecutionLog : ITenantScoped
{
    public long Id { get; set; }
    public int TenantId { get; set; }
    public int CondoId { get; set; }
    public string Prompt { get; private set; } = string.Empty;
    public string Response { get; private set; } = string.Empty;
    public string ModelUsed { get; private set; } = string.Empty;
    public int PromptTokens { get; private set; }
    public int CompletionTokens { get; private set; }
    public int TotalTokens { get; private set; }
    public long DurationMs { get; private set; }
    public bool Success { get; private set; }
    public string? ErrorMessage { get; private set; }
    public DateTimeOffset ExecutedAt { get; private set; } = DateTimeOffset.UtcNow;

    // Construtor EF Core
    private AiExecutionLog() { }

    /// <summary>
    /// Registra um log de execução bem-sucedida de IA
    /// </summary>
    public static AiExecutionLog RegistrarSucesso(
        int tenantId,
        int condoId,
        string prompt,
        string response,
        string modelUsed,
        int promptTokens,
        int completionTokens,
        long durationMs)
    {
        if (tenantId <= 0)
            throw new AiEngineDomainException("TenantId é obrigatório.");

        return new AiExecutionLog
        {
            TenantId = tenantId,
            CondoId = condoId,
            Prompt = prompt ?? string.Empty,
            Response = response ?? string.Empty,
            ModelUsed = modelUsed ?? string.Empty,
            PromptTokens = promptTokens,
            CompletionTokens = completionTokens,
            TotalTokens = promptTokens + completionTokens,
            DurationMs = durationMs,
            Success = true,
            ExecutedAt = DateTimeOffset.UtcNow
        };
    }

    /// <summary>
    /// Registra uma falha de execução de IA
    /// </summary>
    public static AiExecutionLog RegistrarFalha(
        int tenantId,
        int condoId,
        string prompt,
        string errorMessage,
        string modelUsed,
        long durationMs)
    {
        if (tenantId <= 0)
            throw new AiEngineDomainException("TenantId é obrigatório.");

        return new AiExecutionLog
        {
            TenantId = tenantId,
            CondoId = condoId,
            Prompt = prompt ?? string.Empty,
            Response = string.Empty,
            ModelUsed = modelUsed ?? string.Empty,
            PromptTokens = 0,
            CompletionTokens = 0,
            TotalTokens = 0,
            DurationMs = durationMs,
            Success = false,
            ErrorMessage = errorMessage,
            ExecutedAt = DateTimeOffset.UtcNow
        };
    }
}
