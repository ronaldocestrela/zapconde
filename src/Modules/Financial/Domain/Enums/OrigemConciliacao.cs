namespace Modules.Financial.Domain.Enums;

/// <summary>
/// Tipo de origem da transação do sistema vinculada na conciliação bancária.
/// </summary>
public enum OrigemConciliacao
{
    Fatura = 1,
    ParcelaAcordo = 2,
    DespesaBalancete = 3,
    Outro = 99
}
