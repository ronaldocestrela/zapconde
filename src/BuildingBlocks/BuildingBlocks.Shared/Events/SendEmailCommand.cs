using BuildingBlocks.Shared.Email;
using BuildingBlocks.Shared.Messaging;
using BuildingBlocks.Shared.MultiTenancy;

namespace BuildingBlocks.Shared.Events;

/// <summary>
/// Comando de integração para envio assíncrono de e-mail via mensageria (MassTransit / RabbitMQ).
/// </summary>
public record SendEmailCommand : IIntegrationEvent, ITenantScoped
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredOnUtc { get; init; } = DateTime.UtcNow;

    public int TenantId { get; set; }
    public EmailMessage Message { get; init; }

    public SendEmailCommand(EmailMessage message, int tenantId = 0)
    {
        Message = message ?? throw new ArgumentNullException(nameof(message));
        TenantId = tenantId;
    }

    /// <summary>
    /// Construtor sem parâmetros para serializadores (System.Text.Json / MassTransit).
    /// </summary>
    public SendEmailCommand()
    {
        Message = new EmailMessage("placeholder@domain.com", "placeholder");
    }
}
