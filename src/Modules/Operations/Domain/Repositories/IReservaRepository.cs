using Modules.Operations.Domain.Entities;
using Modules.Operations.Domain.Enums;

namespace Modules.Operations.Domain.Repositories;

/// <summary>
/// Contrato do repositório de domínio para persistência e consulta de Reservas de Áreas Comuns.
/// </summary>
public interface IReservaRepository
{
    Task<Reserva?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<Reserva>> GetAllAsync(int condoId, int? areaComumId = null, int? moradorId = null, StatusReserva? status = null, DateTime? dataInicio = null, DateTime? dataFim = null, CancellationToken ct = default);
    Task<bool> HasOverlappingReservationAsync(int condoId, int areaComumId, DateTime dataInicio, DateTime dataFim, int? ignoreReservaId = null, CancellationToken ct = default);
    Task AddAsync(Reserva reserva, CancellationToken ct = default);
    Task UpdateAsync(Reserva reserva, CancellationToken ct = default);
}
