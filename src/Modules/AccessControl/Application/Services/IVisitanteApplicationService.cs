using BuildingBlocks.Shared;
using Modules.AccessControl.Application.DTOs;
using Modules.AccessControl.Domain.Enums;

namespace Modules.AccessControl.Application.Services;

public interface IVisitanteApplicationService
{
    Task<Result<VisitanteDto>> AuthorizeVisitanteAsync(CreateVisitanteRequestDto request, CancellationToken ct = default);
    Task<Result<VisitanteDto>> RegistrarEntradaAsync(int id, int? operadorId = null, CancellationToken ct = default);
    Task<Result<VisitanteDto>> RegistrarSaidaAsync(int id, int? operadorId = null, CancellationToken ct = default);
    Task<Result<VisitanteDto>> CancelarAutorizacaoAsync(int id, string? motivo = null, CancellationToken ct = default);
    Task<Result<VisitanteDto>> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Result<IEnumerable<VisitanteDto>>> GetVisitantesAsync(
        TipoVisitante? tipo = null,
        StatusVisitante? status = null,
        int? unidadeId = null,
        string? busca = null,
        CancellationToken ct = default);
    Task<Result<VisitanteSummaryDto>> GetSummaryAsync(CancellationToken ct = default);
}
