using BuildingBlocks.Shared.Email;
using BuildingBlocks.Shared.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Infrastructure.Email;

/// <summary>
/// Consumidor MassTransit para processar comandos de envio assíncrono de e-mail (SendEmailCommand).
/// </summary>
public sealed class SendEmailConsumer : IConsumer<SendEmailCommand>
{
    private readonly IEmailService _emailService;
    private readonly ILogger<SendEmailConsumer> _logger;

    public SendEmailConsumer(
        IEmailService emailService,
        ILogger<SendEmailConsumer> logger)
    {
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Consume(ConsumeContext<SendEmailCommand> context)
    {
        var command = context.Message;
        _logger.LogInformation("Processando SendEmailCommand para EventId {EventId}, Assunto: '{Subject}'",
            command.EventId, command.Message.Subject);

        var result = await _emailService.SendEmailAsync(command.Message, context.CancellationToken);

        if (!result.IsSuccess)
        {
            _logger.LogError("Falha ao consumir SendEmailCommand (EventId {EventId}): {ErrorMessage}",
                command.EventId, result.Message);
            
            throw new InvalidOperationException($"Falha ao enviar e-mail via consumidor MassTransit: {result.Message}");
        }

        _logger.LogInformation("SendEmailCommand (EventId {EventId}) processado com sucesso.", command.EventId);
    }
}
