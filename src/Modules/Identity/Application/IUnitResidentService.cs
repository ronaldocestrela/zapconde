using BuildingBlocks.Shared;
using Modules.Identity.Application.Dtos;

namespace Modules.Identity.Application;

public interface IUnitResidentService
{
    Task<Result<IReadOnlyList<BlockDto>>> GetBlocksAsync(CancellationToken ct = default);

    Task<Result<BlockDto>> CreateBlockAsync(CreateBlockRequestDto request, CancellationToken ct = default);

    Task<Result<IReadOnlyList<UnitListItemDto>>> ListUnitsAsync(UnitListQueryDto query, CancellationToken ct = default);

    Task<Result<UnitCreatedDto>> CreateUnitAsync(CreateUnitRequestDto request, string? userId, CancellationToken ct = default);

    Task<Result<UnitListItemDto>> UpdateUnitAsync(int unitId, UpdateUnitRequestDto request, CancellationToken ct = default);

    Task<Result> TransferOwnershipAsync(int unitId, TransferOwnershipRequestDto request, string? userId, CancellationToken ct = default);

    Task<Result<IReadOnlyList<UnitHistoryItemDto>>> GetHistoryAsync(int unitId, CancellationToken ct = default);

    Task<byte[]> GetImportTemplateAsync(CancellationToken ct = default);

    Task<Result<ImportPreviewResultDto>> PreviewImportAsync(Stream fileStream, CancellationToken ct = default);

    Task<Result<ImportCommitResultDto>> CommitImportAsync(ImportCommitRequestDto request, string? userId, CancellationToken ct = default);
}
