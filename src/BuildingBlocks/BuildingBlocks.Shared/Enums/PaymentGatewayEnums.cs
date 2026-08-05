namespace BuildingBlocks.Shared.Enums;

/// <summary>
/// Provedores de Gateway de Pagamento integrados ao sistema.
/// </summary>
public enum PaymentGatewayProvider
{
    None = 0,
    Asaas = 1,
    Manual = 2,
    Mock = 3
}

/// <summary>
/// Status da cobrança retornado pelo gateway externo.
/// </summary>
public enum GatewayChargeStatus
{
    Pending = 1,
    Received = 2,
    Confirmed = 3,
    Overdue = 4,
    Refunded = 5,
    Canceled = 6
}
