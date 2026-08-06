namespace Modules.Financial.Domain.Enums;

/// <summary>
/// Status do ciclo de vida da pasta digital de prestação de contas.
/// </summary>
public enum StatusPastaDigital
{
    Rascunho = 1,
    EmAnaliseConselho = 2,
    Aprovada = 3,
    Rejeitada = 4,
    Publicada = 5
}
