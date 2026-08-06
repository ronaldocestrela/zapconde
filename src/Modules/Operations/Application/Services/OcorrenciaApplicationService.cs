using BuildingBlocks.Shared;
using BuildingBlocks.Shared.MultiTenancy;
using Modules.Operations.Application.DTOs;
using Modules.Operations.Domain.Entities;
using Modules.Operations.Domain.Enums;
using Modules.Operations.Domain.Exceptions;
using Modules.Operations.Domain.Repositories;

namespace Modules.Operations.Application.Services;

public class OcorrenciaApplicationService : IOcorrenciaApplicationService
{
    private readonly IOcorrenciaRepository _repository;
    private readonly ICurrentTenantService _currentTenantService;

    public OcorrenciaApplicationService(
        IOcorrenciaRepository repository,
        ICurrentTenantService currentTenantService)
    {
        _repository = repository;
        _currentTenantService = currentTenantService;
    }

    public async Task<Result<OcorrenciaDto>> CriarOcorrenciaAsync(CriarOcorrenciaRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var tenantId = _currentTenantService.TenantId;
            if (!tenantId.HasValue || tenantId.Value <= 0)
                return Result<OcorrenciaDto>.Failure("Tenant não identificado no contexto da requisição.");

            if (request.CondoId <= 0)
                return Result<OcorrenciaDto>.ValidationFailure(new[] { "CondoId é obrigatório." });

            if (string.IsNullOrWhiteSpace(request.MoradorId))
                return Result<OcorrenciaDto>.ValidationFailure(new[] { "MoradorId é obrigatório." });

            if (string.IsNullOrWhiteSpace(request.Titulo))
                return Result<OcorrenciaDto>.ValidationFailure(new[] { "Título é obrigatório." });

            if (string.IsNullOrWhiteSpace(request.Descricao))
                return Result<OcorrenciaDto>.ValidationFailure(new[] { "Descrição é obrigatória." });

            var ocorrencia = Ocorrencia.Create(
                tenantId: tenantId.Value,
                condoId: request.CondoId,
                moradorId: request.MoradorId,
                moradorNome: request.MoradorNome,
                titulo: request.Titulo,
                descricao: request.Descricao,
                categoria: request.Categoria,
                prioridade: request.Prioridade,
                localizacao: request.Localizacao
            );

            if (request.AnexosIniciais != null && request.AnexosIniciais.Count > 0)
            {
                foreach (var a in request.AnexosIniciais)
                {
                    ocorrencia.AdicionarAnexo(a.Url, a.NomeArquivo, a.ContentType, a.TamanhoBytes, request.MoradorId);
                }
            }

            await _repository.AddAsync(ocorrencia, cancellationToken);

            return Result<OcorrenciaDto>.Success(MapToDto(ocorrencia));
        }
        catch (ArgumentException ex)
        {
            return Result<OcorrenciaDto>.ValidationFailure(new[] { ex.Message });
        }
        catch (Exception ex)
        {
            return Result<OcorrenciaDto>.Failure($"Erro ao criar ocorrência: {ex.Message}");
        }
    }

    public async Task<Result<OcorrenciaDto>> AtualizarStatusAsync(Guid id, AtualizarStatusOcorrenciaRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var ocorrencia = await _repository.GetWithDetailsAsync(id, cancellationToken);
            if (ocorrencia == null)
                return Result<OcorrenciaDto>.Failure($"Ocorrência com ID '{id}' não foi encontrada.");

            ocorrencia.AtualizarStatus(
                novoStatus: request.NovoStatus,
                comentario: request.Comentario,
                usuarioId: request.UsuarioId,
                usuarioNome: request.UsuarioNome,
                observacaoResolucao: request.ObservacaoResolucao
            );

            await _repository.UpdateAsync(ocorrencia, cancellationToken);

            return Result<OcorrenciaDto>.Success(MapToDto(ocorrencia));
        }
        catch (InvalidOcorrenciaStatusTransitionException ex)
        {
            return Result<OcorrenciaDto>.Failure(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return Result<OcorrenciaDto>.ValidationFailure(new[] { ex.Message });
        }
        catch (Exception ex)
        {
            return Result<OcorrenciaDto>.Failure($"Erro ao atualizar status da ocorrência: {ex.Message}");
        }
    }

    public async Task<Result<AnexoOcorrenciaDto>> AdicionarAnexoAsync(Guid id, AdicionarAnexoOcorrenciaRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var ocorrencia = await _repository.GetWithDetailsAsync(id, cancellationToken);
            if (ocorrencia == null)
                return Result<AnexoOcorrenciaDto>.Failure($"Ocorrência com ID '{id}' não foi encontrada.");

            var anexo = ocorrencia.AdicionarAnexo(
                url: request.Url,
                nomeArquivo: request.NomeArquivo,
                contentType: request.ContentType,
                tamanhoBytes: request.TamanhoBytes,
                uploadPorUserId: request.UploadPorUserId
            );

            await _repository.UpdateAsync(ocorrencia, cancellationToken);

            return Result<AnexoOcorrenciaDto>.Success(MapToAnexoDto(anexo));
        }
        catch (Exception ex)
        {
            return Result<AnexoOcorrenciaDto>.Failure($"Erro ao adicionar anexo à ocorrência: {ex.Message}");
        }
    }

    public async Task<Result<OcorrenciaDto>> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var ocorrencia = await _repository.GetWithDetailsAsync(id, cancellationToken);
        if (ocorrencia == null)
            return Result<OcorrenciaDto>.Failure($"Ocorrência com ID '{id}' não foi encontrada.");

        return Result<OcorrenciaDto>.Success(MapToDto(ocorrencia));
    }

    public async Task<Result<IEnumerable<OcorrenciaDto>>> ListarAsync(
        int condoId,
        StatusOcorrencia? status = null,
        CategoriaOcorrencia? categoria = null,
        PrioridadeOcorrencia? prioridade = null,
        string? moradorId = null,
        CancellationToken cancellationToken = default)
    {
        if (condoId <= 0)
            return Result<IEnumerable<OcorrenciaDto>>.ValidationFailure(new[] { "CondoId é obrigatório." });

        var ocorrencias = await _repository.ListAsync(condoId, status, categoria, prioridade, moradorId, cancellationToken);
        var dtos = ocorrencias.Select(MapToDto);

        return Result<IEnumerable<OcorrenciaDto>>.Success(dtos);
    }

    public async Task<Result<OcorrenciaSummaryDto>> ObterResumoMetricasAsync(int condoId, CancellationToken cancellationToken = default)
    {
        if (condoId <= 0)
            return Result<OcorrenciaSummaryDto>.ValidationFailure(new[] { "CondoId é obrigatório." });

        var metrics = await _repository.GetSummaryMetricsAsync(condoId, cancellationToken);
        var dto = new OcorrenciaSummaryDto(metrics.Total, metrics.Abertas, metrics.EmAndamento, metrics.Resolvidas, metrics.Urgentes);

        return Result<OcorrenciaSummaryDto>.Success(dto);
    }

    private static OcorrenciaDto MapToDto(Ocorrencia ocorrencia)
    {
        return new OcorrenciaDto(
            Id: ocorrencia.Id,
            TenantId: ocorrencia.TenantId,
            CondoId: ocorrencia.CondoId,
            MoradorId: ocorrencia.MoradorId,
            MoradorNome: ocorrencia.MoradorNome,
            Titulo: ocorrencia.Titulo,
            Descricao: ocorrencia.Descricao,
            Categoria: ocorrencia.Categoria,
            Prioridade: ocorrencia.Prioridade,
            Status: ocorrencia.Status,
            Localizacao: ocorrencia.Localizacao,
            DataAbertura: ocorrencia.DataAbertura,
            DataConclusao: ocorrencia.DataConclusao,
            ResponsavelId: ocorrencia.ResponsavelId,
            ResponsavelNome: ocorrencia.ResponsavelNome,
            ObservacaoResolucao: ocorrencia.ObservacaoResolucao,
            Anexos: ocorrencia.Anexos.Select(MapToAnexoDto).ToList(),
            Historico: ocorrencia.Historico.OrderByDescending(h => h.DataAlteracao).Select(MapToHistoricoDto).ToList()
        );
    }

    private static AnexoOcorrenciaDto MapToAnexoDto(AnexoOcorrencia a)
    {
        return new AnexoOcorrenciaDto(
            Id: a.Id,
            OcorrenciaId: a.OcorrenciaId,
            Url: a.Url,
            NomeArquivo: a.NomeArquivo,
            ContentType: a.ContentType,
            TamanhoBytes: a.TamanhoBytes,
            DataUpload: a.DataUpload,
            UploadPorUserId: a.UploadPorUserId
        );
    }

    private static HistoricoOcorrenciaDto MapToHistoricoDto(HistoricoOcorrencia h)
    {
        return new HistoricoOcorrenciaDto(
            Id: h.Id,
            OcorrenciaId: h.OcorrenciaId,
            StatusAnterior: h.StatusAnterior,
            StatusNovo: h.StatusNovo,
            Comentario: h.Comentario,
            DataAlteracao: h.DataAlteracao,
            AlteradoPorUserId: h.AlteradoPorUserId,
            AlteradoPorNome: h.AlteradoPorNome
        );
    }
}
