namespace Modules.Financial.Domain.Enums;

/// <summary>
/// Status da fatura condominial.
/// </summary>
public enum StatusFatura
{
    Pendente = 1,
    Pago = 2,
    Vencido = 3,
    Cancelado = 4,
    ParcialmentePago = 5,
    EmAcordo = 6
}
