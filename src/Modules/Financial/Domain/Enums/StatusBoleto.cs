namespace Modules.Financial.Domain.Enums;

/// <summary>
/// Status do boleto bancário.
/// </summary>
public enum StatusBoleto
{
    Gerado = 1,
    Registrado = 2,
    Pago = 3,
    Cancelado = 4,
    Baixado = 5
}
