namespace BuildingBlocks.Shared.Messaging;

/// <summary>
/// Contrato base para eventos de integração entre módulos e serviços.
/// </summary>
public interface IIntegrationEvent
{
    Guid EventId { get; }
    DateTime OccurredOnUtc { get; }
}
