using BuildingBlocks.Shared;
using Modules.Financial.Application.Dtos;

namespace Modules.Financial.Application.Services;

public interface IReguaInadimplenciaAppService
{
    Task<Result<IEnumerable<EtapaReguaDto>>> ObterConfiguracaoReguaAsync(int condoId, CancellationToken ct = default);
    Task<Result> SalvarConfiguracaoReguaAsync(int condoId, IEnumerable<SalvarEtapaReguaDto> etapas, CancellationToken ct = default);
    Task<Result<ProcessamentoReguaResultadoDto>> ProcessarReguaCobrancaAsync(int condoId, CancellationToken ct = default);
    Task<Result<DashboardInadimplenciaDto>> ObterDashboardInadimplenciaAsync(int condoId, CancellationToken ct = default);
}
