using BuildingBlocks.Shared;
using Modules.Financial.Application.Dtos;

namespace Modules.Financial.Application.Services;

/// <summary>
/// Contrato para comunicação com o Gateway de Pagamento / PIX.
/// </summary>
public interface IPaymentGatewayService
{
    /// <summary>
    /// Registra uma nova cobrança híbrida (Boleto + PIX) no gateway externo.
    /// </summary>
    Task<Result<GatewayEmissaoResultadoDto>> GerarCobrancaBoletoPixAsync(
        BoletoCobrancaRequestDto request, CancellationToken ct = default);

    /// <summary>
    /// Consulta o status atual de uma cobrança no gateway externo.
    /// </summary>
    Task<Result<GatewayCobrancaStatusDto>> ConsultarStatusCobrancaAsync(
        string externalChargeId, CancellationToken ct = default);

    /// <summary>
    /// Cancela uma cobrança no gateway externo.
    /// </summary>
    Task<Result<bool>> CancelarCobrancaAsync(
        string externalChargeId, CancellationToken ct = default);
}
