using BuildingBlocks.Shared;
using Modules.Operations.Application.DTOs;

namespace Modules.AIEngine.Application.Services;

/// <summary>
/// Contrato do serviço de triagem inteligente de ocorrências via foto, áudio e relato textual.
/// </summary>
public interface IOcorrenciaTriagemService
{
    /// <summary>
    /// Realiza a triagem inteligente (prévia/análise) de uma foto, áudio ou relato sem persistir no banco.
    /// </summary>
    Task<Result<ResultadoTriagemOcorrenciaDto>> AnalisarOcorrenciaAsync(
        TriagemOcorrenciaRequestDto request,
        CancellationToken ct = default);

    /// <summary>
    /// Realiza a triagem inteligente e abre o chamado automaticamente no módulo de Operações.
    /// </summary>
    Task<Result<ResultadoTriagemOcorrenciaDto>> TriarEAbrirOcorrenciaAsync(
        TriagemOcorrenciaRequestDto request,
        CancellationToken ct = default);
}
