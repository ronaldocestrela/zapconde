using BuildingBlocks.Shared;
using Modules.Operations.Application.DTOs;
using Modules.Operations.Domain.Enums;

namespace Modules.Operations.Application.Services;

public interface IPlanoManutencaoApplicationService
{
    Task<Result<PlanoManutencaoDto>> CriarPlanoAsync(CreatePlanoManutencaoRequest request, CancellationToken cancellationToken = default);
    Task<Result<PlanoManutencaoDto>> AtualizarPlanoAsync(Guid id, UpdatePlanoManutencaoRequest request, CancellationToken cancellationToken = default);
    Task<Result<PlanoManutencaoDto>> ConcluirManutencaoAsync(Guid id, ConcluirManutencaoRequest request, CancellationToken cancellationToken = default);
    Task<Result<PlanoManutencaoDto>> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<PlanoManutencaoDto>>> ListarAsync(
        int condoId,
        CategoriaManutencao? categoria = null,
        StatusManutencao? status = null,
        PeriodicidadeManutencao? periodicidade = null,
        DateTime? inicio = null,
        DateTime? fim = null,
        CancellationToken cancellationToken = default);

    Task<Result<PlanoManutencaoSummaryDto>> ObterResumoMetricasAsync(int condoId, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<ManutencaoCalendarEventDto>>> ObterEventosCalendarioAsync(
        int condoId,
        DateTime? inicio = null,
        DateTime? fim = null,
        CancellationToken cancellationToken = default);
}
