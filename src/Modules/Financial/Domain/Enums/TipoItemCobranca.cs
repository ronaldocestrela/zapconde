namespace Modules.Financial.Domain.Enums;

/// <summary>
/// Tipo de lançamento/item de cobrança de uma fatura.
/// </summary>
public enum TipoItemCobranca
{
    TaxaCondominial = 1,
    FundoReserva = 2,
    Agua = 3,
    Gas = 4,
    Multa = 5,
    Juros = 6,
    ReservaAreaComum = 7,
    Outros = 8
}
