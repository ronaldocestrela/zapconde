using BuildingBlocks.Shared;
using Modules.Operations.Application.DTOs;
using Modules.Operations.Domain.Enums;

namespace Modules.Operations.Application.Services;

public interface IReservaApplicationService
{
    Task<Result<ReservaDto>> CriarReservaAsync(CreateReservaRequest request, CancellationToken ct = default);
    Task<Result<ReservaDto>> ObterPorIdAsync(int id, CancellationToken ct = default);
    Task<Result<IEnumerable<ReservaDto>>> ListarReservasAsync(int condoId, int? areaComumId = null, int? moradorId = null, StatusReserva? status = null, DateTime? dataInicio = null, DateTime? dataFim = null, CancellationToken ct = default);
    Task<Result<ReservaDto>> CancelarReservaAsync(int id, CancelarReservaRequest request, CancellationToken ct = default);
    Task<Result<ReservaDto>> AprovarReservaAsync(int id, CancellationToken ct = default);
    Task<Result<ReservaDto>> RejeitarReservaAsync(int id, RejeitarReservaRequest request, CancellationToken ct = default);
    Task<Result<ReservaSummaryDto>> ObterResumoAsync(int condoId, CancellationToken ct = default);
    Task<Result<IEnumerable<ReservaCalendarSlotDto>>> ObterCalendarioAsync(int condoId, int? areaComumId, DateTime inicio, DateTime fim, CancellationToken ct = default);
}
