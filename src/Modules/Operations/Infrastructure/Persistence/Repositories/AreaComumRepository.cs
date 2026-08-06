using Microsoft.EntityFrameworkCore;
using Modules.Operations.Domain.Entities;
using Modules.Operations.Domain.Enums;
using Modules.Operations.Domain.Repositories;

namespace Modules.Operations.Infrastructure.Persistence.Repositories;

public class AreaComumRepository : IAreaComumRepository
{
    private readonly OperationsDbContext _context;

    public AreaComumRepository(OperationsDbContext context)
    {
        _context = context;
    }

    public async Task<AreaComum?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _context.AreasComuns.FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task<IEnumerable<AreaComum>> GetAllAsync(
        int condoId,
        StatusAreaComum? status = null,
        TipoAreaComum? tipo = null,
        CancellationToken ct = default)
    {
        var query = _context.AreasComuns.AsQueryable();

        if (condoId > 0)
            query = query.Where(x => x.CondoId == condoId);

        if (status.HasValue)
            query = query.Where(x => x.Status == status.Value);

        if (tipo.HasValue)
            query = query.Where(x => x.Tipo == tipo.Value);

        return await query.OrderBy(x => x.Nome).ToListAsync(ct);
    }

    public async Task<bool> ExistsByNameAsync(int condoId, string nome, int? ignoreId = null, CancellationToken ct = default)
    {
        var query = _context.AreasComuns.Where(x => x.CondoId == condoId && x.Nome.ToLower() == nome.Trim().ToLower());

        if (ignoreId.HasValue)
            query = query.Where(x => x.Id != ignoreId.Value);

        return await query.AnyAsync(ct);
    }

    public async Task AddAsync(AreaComum areaComum, CancellationToken ct = default)
    {
        await _context.AreasComuns.AddAsync(areaComum, ct);
    }

    public Task UpdateAsync(AreaComum areaComum, CancellationToken ct = default)
    {
        _context.AreasComuns.Update(areaComum);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await _context.SaveChangesAsync(ct);
    }
}
