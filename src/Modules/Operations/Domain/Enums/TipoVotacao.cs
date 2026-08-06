namespace Modules.Operations.Domain.Enums;

/// <summary>
/// Regra de maioria exigida para a aprovação de uma pauta.
/// </summary>
public enum TipoVotacao
{
    /// <summary>
    /// Maioria simples (50% + 1 dos votantes presentes).
    /// </summary>
    MaioriaSimples = 1,

    /// <summary>
    /// Maioria qualificada (ex: 2/3 dos condôminos).
    /// </summary>
    MaioriaQualificada = 2,

    /// <summary>
    /// Unanimidade (100% dos condôminos).
    /// </summary>
    Unanimidade = 3
}
