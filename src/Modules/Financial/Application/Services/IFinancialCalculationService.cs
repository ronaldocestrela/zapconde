using BuildingBlocks.Shared;
using Modules.Financial.Application.DTOs;

namespace Modules.Financial.Application.Services;

/// <summary>
/// Serviço de aplicação para orquestração de simulações e cálculos de encargos financeiros.
/// </summary>
public interface IFinancialCalculationService
{
    Task<Result<CalculoFinanceiroDto>> CalcularSimulacaoAsync(SimularCalculoRequestDto dto, CancellationToken ct = default);
    Task<Result<CalculoFinanceiroDto>> SimularFaturaExistenteAsync(int faturaId, DateTime dataSimulacao, int tenantId, CancellationToken ct = default);
    Task<Result<IEnumerable<ProjecaoCalculoDto>>> ObterProjecaoFuturaAsync(int faturaId, int tenantId, CancellationToken ct = default);
}
