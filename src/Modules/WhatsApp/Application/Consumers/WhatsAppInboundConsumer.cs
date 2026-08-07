using BuildingBlocks.Shared.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using Modules.WhatsApp.Application.Services;

namespace Modules.WhatsApp.Application.Consumers;

/// <summary>
/// Consumidor em Background (MassTransit) para processar os eventos WhatsAppMessageReceivedEvent
/// e realizar a resolução de tenant/morador com Redis e PostgreSQL.
/// </summary>
public class WhatsAppInboundConsumer : IConsumer<WhatsAppMessageReceivedEvent>
{
    private readonly IWhatsAppInboundProcessorService _processorService;
    private readonly ILogger<WhatsAppInboundConsumer> _logger;

    public WhatsAppInboundConsumer(
        IWhatsAppInboundProcessorService processorService,
        ILogger<WhatsAppInboundConsumer> logger)
    {
        _processorService = processorService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<WhatsAppMessageReceivedEvent> context)
    {
        _logger.LogInformation("Consumindo WhatsAppMessageReceivedEvent para MessageId: {MessageId}", context.Message.MessageId);

        await _processorService.ProcessInboundMessageAsync(context.Message, context.CancellationToken);
    }
}
