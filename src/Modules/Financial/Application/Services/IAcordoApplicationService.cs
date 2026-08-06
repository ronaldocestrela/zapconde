using BuildingBlocks.Shared;
using Modules.Financial.Application.Dtos;
using Modules.Financial.Domain.Enums;

namespace Modules.Financial.Application.Services;

public interface IAcordoApplicationService
{
    Task<Result<SimulacaoAcordoResponse>> SimularAcordoAsync(SimulacaoAcordoRequest request, CancellationToken ct = default);
    Task<Result<AcordoDto>> CriarAcordoAsync(CriarAcordoRequest request, CancellationToken ct = default);
    Task<Result<IEnumerable<AcordoDto>>> ObterAcordosPorCondominioAsync(int condoId, int? unidadeId = null, StatusAcordo? status = null, CancellationToken ct = default);
    Task<Result<AcordoDto>> ObterDetalhesAcordoAsync(int acordoId, CancellationToken ct = default);
    Task<Result> CancelarAcordoAsync(int acordoId, string motivo, CancellationToken ct = default);
    Task<Result> RegistrarPagamentoParcelaAsync(int acordoId, int numeroParcela, DateTime dataPagamento, CancellationToken ct = default);
    Task<Result> MarcarAcordoDescumpridoAsync(int acordoId, CancellationToken ct = default);
}
