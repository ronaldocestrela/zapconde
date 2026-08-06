namespace Modules.Operations.Domain.Enums;

/// <summary>
/// Status da pauta de votação.
/// </summary>
public enum StatusPauta
{
    /// <summary>
    /// Pauta aberta para recebimento de votos.
    /// </summary>
    Aberta = 1,

    /// <summary>
    /// Pauta encerrada e votos congelados.
    /// </summary>
    Encerrada = 2
}
