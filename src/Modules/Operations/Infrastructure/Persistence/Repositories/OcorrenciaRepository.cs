using Microsoft.EntityFrameworkCore;
using Modules.Operations.Domain.Entities;
using Modules.Operations.Domain.Enums;
using Modules.Operations.Domain.Repositories;

namespace Modules.Operations.Infrastructure.Persistence.Repositories;

public class OcorrenciaRepository : IOcorrenciaRepository
{
    private readonly OperationsDbContext _context;

    public OcorrenciaRepository(OperationsDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Ocorrencia ocorrencia, CancellationToken cancellationToken = default)
    {
        await _context.Ocorrencias.AddAsync(ocorrencia, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Ocorrencia ocorrencia, CancellationToken cancellationToken = default)
    {
        var entry = _context.Entry(ocorrencia);
        if (entry.State == EntityState.Detached)
        {
            _context.Ocorrencias.Update(ocorrencia);
        }
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<Ocorrencia?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Ocorrencias.FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public async Task<Ocorrencia?> GetWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Ocorrencias
            .Include(o => o.Anexos)
            .Include(o => o.Historico)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Ocorrencia>> ListAsync(
        int condoId,
        StatusOcorrencia? status = null,
        CategoriaOcorrencia? categoria = null,
        PrioridadeOcorrencia? prioridade = null,
        string? moradorId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Ocorrencias
            .Include(o => o.Anexos)
            .Include(o => o.Historico)
            .Where(o => o.CondoId == condoId);

        if (status.HasValue)
        {
            query = query.Where(o => o.Status == status.Value);
        }

        if (categoria.HasValue)
        {
            query = query.Where(o => o.Categoria == categoria.Value);
        }

        if (prioridade.HasValue)
        {
            query = query.Where(o => o.Prioridade == prioridade.Value);
        }

        if (!string.IsNullOrWhiteSpace(moradorId))
        {
            query = query.Where(o => o.MoradorId == moradorId);
        }

        return await query
            .OrderByDescending(o => o.DataAbertura)
            .ToListAsync(cancellationToken);
    }

    public async Task<(int Total, int Abertas, int EmAndamento, int Resolvidas, int Urgentes)> GetSummaryMetricsAsync(
        int condoId,
        CancellationToken cancellationToken = default)
    {
        var tickets = await _context.Ocorrencias
            .Where(o => o.CondoId == condoId)
            .ToListAsync(cancellationToken);

        var total = tickets.Count;
        var abertas = tickets.Count(t => t.Status == StatusOcorrencia.Aberta);
        var emAndamento = tickets.Count(t => t.Status == StatusOcorrencia.EmAndamento || t.Status == StatusOcorrencia.AguardandoPeca);
        var resolvidas = tickets.Count(t => t.Status == StatusOcorrencia.Resolvida);
        var urgentes = tickets.Count(t => t.Prioridade == PrioridadeOcorrencia.Urgente && t.Status != StatusOcorrencia.Resolvida && t.Status != StatusOcorrencia.Cancelada);

        return (total, abertas, emAndamento, resolvidas, urgentes);
    }
}
