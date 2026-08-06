using BuildingBlocks.Shared;
using Modules.Operations.Application.DTOs;
using Modules.Operations.Domain.Enums;

namespace Modules.Operations.Application.Services;

/// <summary>
/// Interface do serviço de aplicação para gerenciamento de áreas comuns.
/// </summary>
public interface IAreaComumApplicationService
{
    Task<Result<AreaComumDto>> CreateAsync(CreateAreaComumRequest request, CancellationToken ct = default);
    Task<Result<AreaComumDto>> UpdateAsync(int id, UpdateAreaComumRequest request, CancellationToken ct = default);
    Task<Result<AreaComumDto>> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Result<IEnumerable<AreaComumDto>>> GetAllAsync(int condoId, StatusAreaComum? status = null, TipoAreaComum? tipo = null, CancellationToken ct = default);
    Task<Result<AreaComumDto>> ChangeStatusAsync(int id, ChangeAreaComumStatusRequest request, CancellationToken ct = default);
    Task<Result<AreaComumSummaryDto>> GetSummaryAsync(int condoId, CancellationToken ct = default);
}
