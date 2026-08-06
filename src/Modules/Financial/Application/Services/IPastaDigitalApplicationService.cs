using BuildingBlocks.Shared;
using Modules.Financial.Application.DTOs;

namespace Modules.Financial.Application.Services;

public interface IPastaDigitalApplicationService
{
    Task<Result<PastaDigitalDto>> CriarPastaDigitalAsync(CriarPastaDigitalRequestDto request, CancellationToken ct = default);
    Task<Result<PastaDigitalDto>> ObterPorIdAsync(int id, CancellationToken ct = default);
    Task<Result<IEnumerable<PastaDigitalDto>>> ListarPorCondominioAsync(int condoId, int? ano = null, CancellationToken ct = default);
    Task<Result<PastaDigitalDto>> AdicionarItemBalanceteAsync(int pastaDigitalId, AdicionarItemBalanceteRequestDto request, CancellationToken ct = default);
    Task<Result<PastaDigitalDto>> AnexarDocumentoAsync(int pastaDigitalId, AnexarDocumentoRequestDto request, CancellationToken ct = default);
    Task<Result<PastaDigitalDto>> SubmeterParaConselhoAsync(int pastaDigitalId, CancellationToken ct = default);
    Task<Result<PastaDigitalDto>> AprovarPastaDigitalAsync(int pastaDigitalId, AprovarPastaDigitalRequestDto request, CancellationToken ct = default);
    Task<Result<PastaDigitalDto>> RejeitarPastaDigitalAsync(int pastaDigitalId, RejeitarPastaDigitalRequestDto request, CancellationToken ct = default);
}
