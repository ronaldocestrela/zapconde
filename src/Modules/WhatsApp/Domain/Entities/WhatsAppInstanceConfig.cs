using BuildingBlocks.Shared.MultiTenancy;
using Modules.WhatsApp.Domain.Enums;
using Modules.WhatsApp.Domain.Exceptions;

namespace Modules.WhatsApp.Domain.Entities;

/// <summary>
/// Entidade de configuração de instância do WhatsApp vinculada a um condomínio/tenant.
/// </summary>
public class WhatsAppInstanceConfig : ITenantScoped
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public int CondoId { get; set; }
    public string InstanceName { get; private set; } = string.Empty;
    public WhatsAppProvider Provider { get; private set; } = WhatsAppProvider.EvolutionApi;
    public string BaseUrl { get; private set; } = string.Empty;
    public string ApiKey { get; private set; } = string.Empty;
    public string? WebhookSecret { get; private set; }
    public bool IsActive { get; private set; } = true;
    public string Status { get; private set; } = "Connected";
    public DateTimeOffset CriadoEm { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? AtualizadoEm { get; private set; }
    public DateTimeOffset? UltimaConexaoEm { get; private set; }

    // Construtor EF Core
    private WhatsAppInstanceConfig() { }

    /// <summary>
    /// Factory Method para criar configuração de uma nova instância WhatsApp.
    /// </summary>
    public static WhatsAppInstanceConfig Criar(
        int tenantId,
        int condoId,
        string instanceName,
        WhatsAppProvider provider,
        string baseUrl,
        string apiKey,
        string? webhookSecret)
    {
        if (tenantId <= 0)
            throw new WhatsAppDomainException("TenantId é obrigatório.");

        if (condoId <= 0)
            throw new WhatsAppDomainException("CondoId é obrigatório.");

        if (string.IsNullOrWhiteSpace(instanceName))
            throw new WhatsAppDomainException("O nome da instância é obrigatório.");

        if (string.IsNullOrWhiteSpace(baseUrl))
            throw new WhatsAppDomainException("A URL base do servidor do WhatsApp é obrigatória.");

        if (string.IsNullOrWhiteSpace(apiKey))
            throw new WhatsAppDomainException("A chave de API (ApiKey) é obrigatória.");

        return new WhatsAppInstanceConfig
        {
            TenantId = tenantId,
            CondoId = condoId,
            InstanceName = instanceName.Trim(),
            Provider = provider,
            BaseUrl = baseUrl.Trim().TrimEnd('/'),
            ApiKey = apiKey.Trim(),
            WebhookSecret = webhookSecret?.Trim(),
            IsActive = true,
            Status = "Connected",
            CriadoEm = DateTimeOffset.UtcNow,
            UltimaConexaoEm = DateTimeOffset.UtcNow
        };
    }

    public void AtualizarStatus(string status, bool isActive)
    {
        Status = status;
        IsActive = isActive;
        AtualizadoEm = DateTimeOffset.UtcNow;
        if (isActive)
        {
            UltimaConexaoEm = DateTimeOffset.UtcNow;
        }
    }

    public void AlternarAtivo()
    {
        IsActive = !IsActive;
        Status = IsActive ? "Connected" : "Disconnected";
        AtualizadoEm = DateTimeOffset.UtcNow;
    }
}
