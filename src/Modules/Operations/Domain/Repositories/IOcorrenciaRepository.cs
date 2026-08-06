using Modules.Operations.Domain.Entities;
using Modules.Operations.Domain.Enums;

namespace Modules.Operations.Domain.Repositories;

public interface IOcorrenciaRepository
{
    Task AddAsync(Ocorrencia ocorrencia, CancellationToken cancellationToken = default);
    Task UpdateAsync(Ocorrencia ocorrencia, CancellationToken cancellationToken = default);
    Task<Ocorrencia?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Ocorrencia?> GetWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Ocorrencia>> ListAsync(
        int condoId,
        StatusOcorrencia? status = null,
        CategoriaOcorrencia? categoria = null,
        PrioridadeOcorrencia? prioridade = null,
        string? moradorId = null,
        CancellationToken cancellationToken = default);

    Task<(int Total, int Abertas, int EmAndamento, int Resolvidas, int Urgentes)> GetSummaryMetricsAsync(
        int condoId,
        CancellationToken cancellationToken = default);
}
