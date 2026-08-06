using BuildingBlocks.Shared;
using Modules.Operations.Application.DTOs;
using Modules.Operations.Domain.Enums;

namespace Modules.Operations.Application.Services;

public interface IOcorrenciaApplicationService
{
    Task<Result<OcorrenciaDto>> CriarOcorrenciaAsync(CriarOcorrenciaRequest request, CancellationToken cancellationToken = default);
    Task<Result<OcorrenciaDto>> AtualizarStatusAsync(Guid id, AtualizarStatusOcorrenciaRequest request, CancellationToken cancellationToken = default);
    Task<Result<AnexoOcorrenciaDto>> AdicionarAnexoAsync(Guid id, AdicionarAnexoOcorrenciaRequest request, CancellationToken cancellationToken = default);
    Task<Result<OcorrenciaDto>> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<OcorrenciaDto>>> ListarAsync(
        int condoId,
        StatusOcorrencia? status = null,
        CategoriaOcorrencia? categoria = null,
        PrioridadeOcorrencia? prioridade = null,
        string? moradorId = null,
        CancellationToken cancellationToken = default);

    Task<Result<OcorrenciaSummaryDto>> ObterResumoMetricasAsync(int condoId, CancellationToken cancellationToken = default);
}
