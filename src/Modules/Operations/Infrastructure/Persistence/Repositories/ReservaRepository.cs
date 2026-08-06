using Microsoft.EntityFrameworkCore;
using Modules.Operations.Domain.Entities;
using Modules.Operations.Domain.Enums;
using Modules.Operations.Domain.Repositories;

namespace Modules.Operations.Infrastructure.Persistence.Repositories;

public class ReservaRepository : IReservaRepository
{
    private readonly OperationsDbContext _context;

    public ReservaRepository(OperationsDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Reserva?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var query = _context.Reservas
            .Include(x => x.AreaComum)
            .Where(x => x.Id == id);

        if (!_context.CurrentTenantId.HasValue)
        {
            query = query.IgnoreQueryFilters();
        }

        return await query.FirstOrDefaultAsync(ct);
    }

    public async Task<IEnumerable<Reserva>> GetAllAsync(
        int condoId,
        int? areaComumId = null,
        int? moradorId = null,
        StatusReserva? status = null,
        DateTime? dataInicio = null,
        DateTime? dataFim = null,
        CancellationToken ct = default)
    {
        var query = _context.Reservas
            .Include(x => x.AreaComum)
            .Where(x => x.CondoId == condoId);

        if (!_context.CurrentTenantId.HasValue)
        {
            query = query.IgnoreQueryFilters();
        }

        if (areaComumId.HasValue && areaComumId.Value > 0)
        {
            query = query.Where(x => x.AreaComumId == areaComumId.Value);
        }

        if (moradorId.HasValue && moradorId.Value > 0)
        {
            query = query.Where(x => x.MoradorId == moradorId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(x => x.Status == status.Value);
        }

        if (dataInicio.HasValue)
        {
            var inicioUtc = dataInicio.Value.Kind == DateTimeKind.Utc ? dataInicio.Value : DateTime.SpecifyKind(dataInicio.Value, DateTimeKind.Utc);
            query = query.Where(x => x.DataInicio >= inicioUtc);
        }

        if (dataFim.HasValue)
        {
            var fimUtc = dataFim.Value.Kind == DateTimeKind.Utc ? dataFim.Value : DateTime.SpecifyKind(dataFim.Value, DateTimeKind.Utc);
            query = query.Where(x => x.DataFim <= fimUtc);
        }

        return await query
            .OrderByDescending(x => x.DataInicio)
            .ToListAsync(ct);
    }

    public async Task<bool> HasOverlappingReservationAsync(
        int condoId,
        int areaComumId,
        DateTime dataInicio,
        DateTime dataFim,
        int? ignoreReservaId = null,
        CancellationToken ct = default)
    {
        var inicioUtc = dataInicio.Kind == DateTimeKind.Utc ? dataInicio : DateTime.SpecifyKind(dataInicio, DateTimeKind.Utc);
        var fimUtc = dataFim.Kind == DateTimeKind.Utc ? dataFim : DateTime.SpecifyKind(dataFim, DateTimeKind.Utc);

        var query = _context.Reservas
            .Where(x => x.CondoId == condoId && x.AreaComumId == areaComumId)
            .Where(x => x.Status == StatusReserva.Confirmada || x.Status == StatusReserva.PendenteAprovacao)
            .Where(x => x.DataInicio < fimUtc && x.DataFim > inicioUtc);

        if (!_context.CurrentTenantId.HasValue)
        {
            query = query.IgnoreQueryFilters();
        }

        if (ignoreReservaId.HasValue && ignoreReservaId.Value > 0)
        {
            query = query.Where(x => x.Id != ignoreReservaId.Value);
        }

        return await query.AnyAsync(ct);
    }

    public async Task AddAsync(Reserva reserva, CancellationToken ct = default)
    {
        await _context.Reservas.AddAsync(reserva, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Reserva reserva, CancellationToken ct = default)
    {
        _context.Reservas.Update(reserva);
        await _context.SaveChangesAsync(ct);
    }
}
