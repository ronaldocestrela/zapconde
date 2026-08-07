using BuildingBlocks.Shared.MultiTenancy;
using Modules.AIEngine.Domain.Enums;
using Modules.AIEngine.Domain.Exceptions;

namespace Modules.AIEngine.Domain.Entities;

/// <summary>
/// Entidade de configuração do Semantic Kernel por condomínio/tenant.
/// </summary>
public class AiKernelConfig : ITenantScoped
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public int CondoId { get; set; }
    public AiProvider Provider { get; private set; } = AiProvider.MockLocal;
    public string ModelId { get; private set; } = "gpt-4o-mini";
    public string EmbeddingModelId { get; private set; } = "text-embedding-3-small";
    public string ApiKey { get; private set; } = string.Empty;
    public string? Endpoint { get; private set; }
    public string? OrgId { get; private set; }
    public double Temperature { get; private set; } = 0.7;
    public int MaxTokens { get; private set; } = 1500;
    public bool IsActive { get; private set; } = true;
    public DateTimeOffset CriadoEm { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? AtualizadoEm { get; private set; }

    // Construtor privado para EF Core
    private AiKernelConfig() { }

    /// <summary>
    /// Factory Method para criar a configuração do Kernel de IA.
    /// </summary>
    public static AiKernelConfig Criar(
        int tenantId,
        int condoId,
        AiProvider provider,
        string modelId,
        string embeddingModelId,
        string apiKey,
        string? endpoint = null,
        string? orgId = null,
        double temperature = 0.7,
        int maxTokens = 1500)
    {
        if (tenantId <= 0)
            throw new AiEngineDomainException("TenantId é obrigatório.");

        if (condoId <= 0)
            throw new AiEngineDomainException("CondoId é obrigatório.");

        if (string.IsNullOrWhiteSpace(modelId))
            throw new AiEngineDomainException("O ModelId é obrigatório.");

        if (provider != AiProvider.MockLocal && string.IsNullOrWhiteSpace(apiKey))
            throw new AiEngineDomainException("A ApiKey é obrigatória para provedores externos OpenAI/Azure.");

        if (temperature < 0 || temperature > 2.0)
            throw new AiEngineDomainException("A temperatura deve estar entre 0.0 e 2.0.");

        if (maxTokens <= 0)
            throw new AiEngineDomainException("MaxTokens deve ser superior a zero.");

        return new AiKernelConfig
        {
            TenantId = tenantId,
            CondoId = condoId,
            Provider = provider,
            ModelId = modelId.Trim(),
            EmbeddingModelId = string.IsNullOrWhiteSpace(embeddingModelId) ? "text-embedding-3-small" : embeddingModelId.Trim(),
            ApiKey = apiKey?.Trim() ?? string.Empty,
            Endpoint = endpoint?.Trim(),
            OrgId = orgId?.Trim(),
            Temperature = temperature,
            MaxTokens = maxTokens,
            IsActive = true,
            CriadoEm = DateTimeOffset.UtcNow
        };
    }

    /// <summary>
    /// Atualiza as configurações do Kernel
    /// </summary>
    public void Atualizar(
        AiProvider provider,
        string modelId,
        string embeddingModelId,
        string apiKey,
        string? endpoint,
        string? orgId,
        double temperature,
        int maxTokens,
        bool isActive)
    {
        if (provider != AiProvider.MockLocal && string.IsNullOrWhiteSpace(apiKey))
            throw new AiEngineDomainException("A ApiKey é obrigatória para provedores externos OpenAI/Azure.");

        if (string.IsNullOrWhiteSpace(modelId))
            throw new AiEngineDomainException("O ModelId é obrigatório.");

        if (temperature < 0 || temperature > 2.0)
            throw new AiEngineDomainException("A temperatura deve estar entre 0.0 e 2.0.");

        if (maxTokens <= 0)
            throw new AiEngineDomainException("MaxTokens deve ser superior a zero.");

        Provider = provider;
        ModelId = modelId.Trim();
        EmbeddingModelId = string.IsNullOrWhiteSpace(embeddingModelId) ? "text-embedding-3-small" : embeddingModelId.Trim();
        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            ApiKey = apiKey.Trim();
        }
        Endpoint = endpoint?.Trim();
        OrgId = orgId?.Trim();
        Temperature = temperature;
        MaxTokens = maxTokens;
        IsActive = isActive;
        AtualizadoEm = DateTimeOffset.UtcNow;
    }

    public void AlternarAtivo()
    {
        IsActive = !IsActive;
        AtualizadoEm = DateTimeOffset.UtcNow;
    }
}
