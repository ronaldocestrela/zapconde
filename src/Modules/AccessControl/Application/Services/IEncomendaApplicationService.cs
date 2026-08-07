using BuildingBlocks.Shared;
using Modules.AccessControl.Application.DTOs;
using Modules.AccessControl.Domain.Enums;

namespace Modules.AccessControl.Application.Services;

public interface IEncomendaApplicationService
{
    Task<Result<EncomendaDto>> RegistrarRecebimentoAsync(RegistrarRecebimentoEncomendaRequest request, CancellationToken ct = default);
    Task<Result<EncomendaDto>> RegistrarBaixaAsync(int id, RegistrarBaixaEncomendaRequest request, CancellationToken ct = default);
    Task<Result<EncomendaDto>> NotificarMoradorAsync(int id, CancellationToken ct = default);
    Task<Result<EncomendaDto>> CancelarAsync(int id, string motivo, CancellationToken ct = default);
    Task<Result<EncomendaDto>> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Result<IEnumerable<EncomendaDto>>> GetEncomendasAsync(
        StatusEncomenda? status = null,
        TipoEncomenda? tipo = null,
        int? unidadeId = null,
        string? busca = null,
        CancellationToken ct = default);
    Task<Result<EncomendaSummaryDto>> GetSummaryAsync(CancellationToken ct = default);
}
