using BuildingBlocks.Shared;
using Modules.Financial.Application.DTOs;
using Modules.Financial.Domain.Enums;

namespace Modules.Financial.Application.Services;

public interface IInvoiceService
{
    Task<Result<IEnumerable<FaturaSummaryDto>>> GetInvoicesAsync(
        int? condoId = null,
        int? unidadeId = null,
        string? competencia = null,
        StatusFatura? status = null,
        CancellationToken ct = default);

    Task<Result<FaturaDetailDto>> GetInvoiceByIdAsync(int id, CancellationToken ct = default);

    Task<Result<FaturaDetailDto>> CreateInvoiceAsync(CreateFaturaRequest request, CancellationToken ct = default);

    Task<Result> CancelInvoiceAsync(int id, CancellationToken ct = default);
}
