using BuildingBlocks.Shared.Events;
using Modules.WhatsApp.Application.DTOs;

namespace Modules.WhatsApp.Application.Services;

public interface IWhatsAppInboundProcessorService
{
    /// <summary>
    /// Processa o evento enfileirado de mensagem do WhatsApp, resolvendo tenant e morador com Redis/Postgres
    /// e atualizando o status do webhook log.
    /// </summary>
    Task<WhatsAppInboundProcessingResultDto> ProcessInboundMessageAsync(
        WhatsAppMessageReceivedEvent @event,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retorna as métricas agregadas do consumidor em background.
    /// </summary>
    Task<WhatsAppConsumerMetricsDto> GetMetricsAsync(
        int? tenantId = null,
        CancellationToken cancellationToken = default);
}
