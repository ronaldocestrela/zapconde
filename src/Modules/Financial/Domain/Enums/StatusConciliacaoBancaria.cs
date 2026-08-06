namespace Modules.Financial.Domain.Enums;

/// <summary>
/// Status da conciliação do lançamento do extrato bancário.
/// </summary>
public enum StatusConciliacaoBancaria
{
    Pendente = 1,
    ConciliadoAutomatico = 2,
    ConciliadoManual = 3,
    Ignorado = 4,
    Divergencia = 5
}
