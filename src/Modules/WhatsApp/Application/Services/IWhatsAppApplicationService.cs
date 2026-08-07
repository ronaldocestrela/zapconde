using BuildingBlocks.Shared;
using Modules.WhatsApp.Application.DTOs;

namespace Modules.WhatsApp.Application.Services;

public interface IWhatsAppApplicationService
{
    /// <summary>
    /// Processa a recepção de payload de webhook da Evolution API (messages.upsert)
    /// com suporte a idempotência e isolamento por tenant.
    /// </summary>
    Task<Result<WebhookIngestionResultDto>> IngestEvolutionWebhookAsync(
        string rawJson,
        string? headerApiKey = null,
        CancellationToken ct = default);

    /// <summary>
    /// Cadastra uma nova configuração de instância de WhatsApp para o condomínio.
    /// </summary>
    Task<Result<WhatsAppInstanceConfigDto>> CreateInstanceAsync(
        CreateWhatsAppInstanceCommand command,
        CancellationToken ct = default);

    /// <summary>
    /// Lista as instâncias ativas do condomínio atual.
    /// </summary>
    Task<Result<IEnumerable<WhatsAppInstanceConfigDto>>> GetInstancesAsync(
        int? condoId = null,
        CancellationToken ct = default);

    /// <summary>
    /// Alterna o status ativo/inativo de uma instância.
    /// </summary>
    Task<Result<WhatsAppInstanceConfigDto>> ToggleInstanceStatusAsync(
        int instanceId,
        CancellationToken ct = default);

    /// <summary>
    /// Lista os logs de Webhooks com filtros paginados.
    /// </summary>
    Task<Result<IEnumerable<WhatsAppWebhookLogDto>>> GetWebhookLogsAsync(
        string? instanceName = null,
        string? status = null,
        string? phone = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken ct = default);

    /// <summary>
    /// Obtém o resumo dos indicadores KPI do módulo de WhatsApp.
    /// </summary>
    Task<Result<WhatsAppWebhookSummaryDto>> GetSummaryAsync(
        int? condoId = null,
        CancellationToken ct = default);
}
