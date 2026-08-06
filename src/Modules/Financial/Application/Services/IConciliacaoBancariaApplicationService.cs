using BuildingBlocks.Shared;
using Modules.Financial.Application.DTOs;

namespace Modules.Financial.Application.Services;

public interface IConciliacaoBancariaApplicationService
{
    Task<Result<ContaBancariaDto>> CriarContaBancariaAsync(CriarContaBancariaRequestDto request, CancellationToken ct = default);
    Task<Result<IEnumerable<ContaBancariaDto>>> ListarContasBancariasAsync(int condoId, CancellationToken ct = default);
    Task<Result<IEnumerable<ExtratoBancarioItemDto>>> ImportarExtratoAsync(ImportarExtratoRequestDto request, CancellationToken ct = default);
    Task<Result<ResultadoConciliacaoEmLoteDto>> ProcessarConciliacaoAutomaticaAsync(int contaBancariaId, CancellationToken ct = default);
    Task<Result<IEnumerable<ExtratoBancarioItemDto>>> ListarItensPendentesAsync(int contaBancariaId, CancellationToken ct = default);
    Task<Result<ExtratoBancarioItemDto>> ConciliarManualAsync(ConciliarManualRequestDto request, CancellationToken ct = default);
}
