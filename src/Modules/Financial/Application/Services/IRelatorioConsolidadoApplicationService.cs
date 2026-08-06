using BuildingBlocks.Shared;
using Modules.Financial.Application.DTOs;

namespace Modules.Financial.Application.Services;

public interface IRelatorioConsolidadoApplicationService
{
    Task<Result<RelatorioConsolidadoMulticondominioDto>> ObterRelatorioConsolidadoAsync(CancellationToken ct = default);
}
