namespace Modules.Operations.Domain.Enums;

/// <summary>
/// Define o status do ciclo de vida de uma Assembleia Virtual.
/// </summary>
public enum StatusAssembleia
{
    /// <summary>
    /// Assembleia agendada para início futuro.
    /// </summary>
    Agendada = 1,

    /// <summary>
    /// Assembleia aberta e em andamento para votação dos moradores.
    /// </summary>
    EmAndamento = 2,

    /// <summary>
    /// Assembleia finalizada com apuração de votos e Ata gerada.
    /// </summary>
    Encerrada = 3,

    /// <summary>
    /// Assembleia cancelada pela administração.
    /// </summary>
    Cancelada = 4
}
