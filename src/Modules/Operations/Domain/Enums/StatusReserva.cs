namespace Modules.Operations.Domain.Enums;

/// <summary>
/// Define os status do ciclo de vida de uma Reserva de Área Comum.
/// </summary>
public enum StatusReserva
{
    /// <summary>
    /// Aguardando aprovação do síndico/administradora.
    /// </summary>
    PendenteAprovacao = 1,

    /// <summary>
    /// Reserva confirmada e garantida na agenda.
    /// </summary>
    Confirmada = 2,

    /// <summary>
    /// Reserva cancelada pelo morador ou pela administração.
    /// </summary>
    Cancelada = 3,

    /// <summary>
    /// Reserva rejeitada pela administração.
    /// </summary>
    Rejeitada = 4
}
