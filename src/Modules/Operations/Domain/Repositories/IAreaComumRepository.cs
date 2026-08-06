using Modules.Operations.Domain.Entities;
using Modules.Operations.Domain.Enums;

namespace Modules.Operations.Domain.Repositories;

/// <summary>
/// Contrato de repositório para persistência da entidade AreaComum.
/// </summary>
public interface IAreaComumRepository
{
    Task<AreaComum?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<AreaComum>> GetAllAsync(int condoId, StatusAreaComum? status = null, TipoAreaComum? tipo = null, CancellationToken ct = default);
    Task<bool> ExistsByNameAsync(int condoId, string nome, int? ignoreId = null, CancellationToken ct = default);
    Task AddAsync(AreaComum areaComum, CancellationToken ct = default);
    Task UpdateAsync(AreaComum areaComum, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
