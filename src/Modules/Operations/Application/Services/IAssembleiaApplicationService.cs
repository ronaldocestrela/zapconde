using BuildingBlocks.Shared;
using Modules.Operations.Application.DTOs;
using Modules.Operations.Domain.Enums;

namespace Modules.Operations.Application.Services;

public interface IAssembleiaApplicationService
{
    Task<Result<AssembleiaDto>> CriarAssembleiaAsync(CreateAssembleiaRequest request, CancellationToken cancellationToken = default);
    Task<Result<AssembleiaDto>> AdicionarPautaAsync(Guid assembleiaId, CreatePautaInput request, CancellationToken cancellationToken = default);
    Task<Result<AssembleiaDto>> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<AssembleiaDto>>> ListarAsync(
        int condoId,
        StatusAssembleia? status = null,
        TipoAssembleia? tipo = null,
        CancellationToken cancellationToken = default);

    Task<Result<AssembleiaDto>> AtualizarStatusAsync(Guid id, StatusAssembleia novoStatus, CancellationToken cancellationToken = default);
    Task<Result<AssembleiaDto>> RegistrarVotoAsync(Guid assembleiaId, Guid pautaId, RegistrarVotoRequest request, CancellationToken cancellationToken = default);
    Task<Result<AssembleiaDto>> EncerrarEGerarAtaAsync(Guid assembleiaId, CancellationToken cancellationToken = default);
    Task<Result<AssembleiaSummaryDto>> ObterResumoKpiAsync(int condoId, CancellationToken cancellationToken = default);
}
