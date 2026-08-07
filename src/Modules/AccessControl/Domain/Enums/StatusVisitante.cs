namespace Modules.AccessControl.Domain.Enums;

/// <summary>
/// Ciclo de vida e status de fluxo de acesso de visitantes e prestadores.
/// </summary>
public enum StatusVisitante
{
    Agendado = 1,
    Presente = 2,
    Finalizado = 3,
    Cancelado = 4,
    Negado = 5
}
