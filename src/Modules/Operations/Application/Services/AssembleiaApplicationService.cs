using BuildingBlocks.Shared;
using BuildingBlocks.Shared.MultiTenancy;
using Microsoft.EntityFrameworkCore;
using Modules.Operations.Application.DTOs;
using Modules.Operations.Domain.Entities;
using Modules.Operations.Domain.Enums;
using Modules.Operations.Domain.Exceptions;
using Modules.Operations.Infrastructure.Persistence;

namespace Modules.Operations.Application.Services;

public class AssembleiaApplicationService : IAssembleiaApplicationService
{
    private readonly OperationsDbContext _dbContext;
    private readonly ICurrentTenantService _tenantService;

    public AssembleiaApplicationService(
        OperationsDbContext dbContext,
        ICurrentTenantService tenantService)
    {
        _dbContext = dbContext;
        _tenantService = tenantService;
    }

    public async Task<Result<AssembleiaDto>> CriarAssembleiaAsync(CreateAssembleiaRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            int tenantId = GetTenantIdOrThrow();

            var assembleia = AssembleiaVirtual.Create(
                tenantId,
                request.CondoId,
                request.Titulo,
                request.Tipo,
                request.DataInicio,
                request.DataFim,
                request.CriadoPorUserId,
                request.Descricao);

            if (request.PautasInicial != null && request.PautasInicial.Count > 0)
            {
                foreach (var pautaInput in request.PautasInicial)
                {
                    assembleia.AdicionarPauta(pautaInput.Titulo, pautaInput.TipoVotacao, pautaInput.Descricao, pautaInput.OpcoesDisponiveis);
                }
            }

            _dbContext.AssembleiasVirtuais.Add(assembleia);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return Result<AssembleiaDto>.Success(MapToDto(assembleia));
        }
        catch (AssembleiaDomainException ex)
        {
            return Result<AssembleiaDto>.ValidationFailure(new[] { ex.Message });
        }
        catch (Exception ex)
        {
            return Result<AssembleiaDto>.Failure($"Erro ao criar assembleia: {ex.Message}");
        }
    }

    public async Task<Result<AssembleiaDto>> AdicionarPautaAsync(Guid assembleiaId, CreatePautaInput request, CancellationToken cancellationToken = default)
    {
        try
        {
            var assembleia = await GetAssembleiaWithPautasAsync(assembleiaId, cancellationToken);
            if (assembleia == null)
                return Result<AssembleiaDto>.Failure("Assembleia virtual não encontrada.");

            assembleia.AdicionarPauta(request.Titulo, request.TipoVotacao, request.Descricao, request.OpcoesDisponiveis);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return Result<AssembleiaDto>.Success(MapToDto(assembleia));
        }
        catch (AssembleiaDomainException ex)
        {
            return Result<AssembleiaDto>.ValidationFailure(new[] { ex.Message });
        }
        catch (Exception ex)
        {
            return Result<AssembleiaDto>.Failure($"Erro ao adicionar pauta: {ex.Message}");
        }
    }

    public async Task<Result<AssembleiaDto>> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var assembleia = await GetAssembleiaWithPautasAsync(id, cancellationToken);
        if (assembleia == null)
            return Result<AssembleiaDto>.Failure("Assembleia virtual não encontrada.");

        return Result<AssembleiaDto>.Success(MapToDto(assembleia));
    }

    public async Task<Result<IEnumerable<AssembleiaDto>>> ListarAsync(
        int condoId,
        StatusAssembleia? status = null,
        TipoAssembleia? tipo = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.AssembleiasVirtuais
            .Include(a => a.Pautas)
                .ThenInclude(p => p.Votos)
            .Where(a => a.CondoId == condoId);

        if (status.HasValue)
            query = query.Where(a => a.Status == status.Value);

        if (tipo.HasValue)
            query = query.Where(a => a.Tipo == tipo.Value);

        var list = await query
            .OrderByDescending(a => a.DataInicio)
            .ToListAsync(cancellationToken);

        return Result<IEnumerable<AssembleiaDto>>.Success(list.Select(MapToDto));
    }

    public async Task<Result<AssembleiaDto>> AtualizarStatusAsync(Guid id, StatusAssembleia novoStatus, CancellationToken cancellationToken = default)
    {
        try
        {
            var assembleia = await GetAssembleiaWithPautasAsync(id, cancellationToken);
            if (assembleia == null)
                return Result<AssembleiaDto>.Failure("Assembleia virtual não encontrada.");

            if (novoStatus == StatusAssembleia.EmAndamento)
            {
                assembleia.IniciarAssembleia();
            }
            else if (novoStatus == StatusAssembleia.Encerrada)
            {
                assembleia.EncerrarEGerarAta();
            }
            else if (novoStatus == StatusAssembleia.Cancelada)
            {
                assembleia.CancelarAssembleia();
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            return Result<AssembleiaDto>.Success(MapToDto(assembleia));
        }
        catch (AssembleiaDomainException ex)
        {
            return Result<AssembleiaDto>.ValidationFailure(new[] { ex.Message });
        }
        catch (Exception ex)
        {
            return Result<AssembleiaDto>.Failure($"Erro ao atualizar status da assembleia: {ex.Message}");
        }
    }

    public async Task<Result<AssembleiaDto>> RegistrarVotoAsync(Guid assembleiaId, Guid pautaId, RegistrarVotoRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var assembleia = await GetAssembleiaWithPautasAsync(assembleiaId, cancellationToken);
            if (assembleia == null)
                return Result<AssembleiaDto>.Failure("Assembleia virtual não encontrada.");

            assembleia.RegistrarVoto(pautaId, request.MoradorUserId, request.UnidadeId, request.OpcaoEscolhida, request.PesoVoto);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return Result<AssembleiaDto>.Success(MapToDto(assembleia));
        }
        catch (VotoDuplicadoException ex)
        {
            return Result<AssembleiaDto>.Failure(ex.Message);
        }
        catch (AssembleiaDomainException ex)
        {
            return Result<AssembleiaDto>.ValidationFailure(new[] { ex.Message });
        }
        catch (Exception ex)
        {
            return Result<AssembleiaDto>.Failure($"Erro ao registrar voto: {ex.Message}");
        }
    }

    public async Task<Result<AssembleiaDto>> EncerrarEGerarAtaAsync(Guid assembleiaId, CancellationToken cancellationToken = default)
    {
        try
        {
            var assembleia = await GetAssembleiaWithPautasAsync(assembleiaId, cancellationToken);
            if (assembleia == null)
                return Result<AssembleiaDto>.Failure("Assembleia virtual não encontrada.");

            assembleia.EncerrarEGerarAta();
            await _dbContext.SaveChangesAsync(cancellationToken);

            return Result<AssembleiaDto>.Success(MapToDto(assembleia));
        }
        catch (AssembleiaDomainException ex)
        {
            return Result<AssembleiaDto>.ValidationFailure(new[] { ex.Message });
        }
        catch (Exception ex)
        {
            return Result<AssembleiaDto>.Failure($"Erro ao encerrar assembleia e gerar ata: {ex.Message}");
        }
    }

    public async Task<Result<AssembleiaSummaryDto>> ObterResumoKpiAsync(int condoId, CancellationToken cancellationToken = default)
    {
        var assembleias = await _dbContext.AssembleiasVirtuais
            .Include(a => a.Pautas)
                .ThenInclude(p => p.Votos)
            .Where(a => a.CondoId == condoId)
            .ToListAsync(cancellationToken);

        int total = assembleias.Count;
        int agendadas = assembleias.Count(a => a.Status == StatusAssembleia.Agendada);
        int emAndamento = assembleias.Count(a => a.Status == StatusAssembleia.EmAndamento);
        int encerradas = assembleias.Count(a => a.Status == StatusAssembleia.Encerrada);
        int canceladas = assembleias.Count(a => a.Status == StatusAssembleia.Cancelada);

        int totalVotos = assembleias
            .SelectMany(a => a.Pautas)
            .SelectMany(p => p.Votos)
            .Count();

        var summary = new AssembleiaSummaryDto(total, agendadas, emAndamento, encerradas, canceladas, totalVotos);
        return Result<AssembleiaSummaryDto>.Success(summary);
    }

    private async Task<AssembleiaVirtual?> GetAssembleiaWithPautasAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _dbContext.AssembleiasVirtuais
            .Include(a => a.Pautas)
                .ThenInclude(p => p.Votos)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    private int GetTenantIdOrThrow()
    {
        if (!_tenantService.TenantId.HasValue || _tenantService.TenantId.Value <= 0)
            throw new InvalidOperationException("Tenant Context não configurado.");

        return _tenantService.TenantId.Value;
    }

    private static AssembleiaDto MapToDto(AssembleiaVirtual a)
    {
        var pautasDto = a.Pautas
            .OrderBy(p => p.Ordem)
            .Select(p => new PautaDto(
                p.Id,
                p.AssembleiaId,
                p.Titulo,
                p.Descricao,
                p.Ordem,
                p.TipoVotacao,
                p.Status,
                p.OpcoesDisponiveis,
                p.Votos.Count,
                p.ApurarContagemVotos(),
                p.Votos.Select(v => new VotoDto(v.Id, v.PautaId, v.MoradorUserId, v.UnidadeId, v.OpcaoEscolhida, v.PesoVoto, v.DataVoto)).ToList()
            )).ToList();

        int quorumUnidades = a.Pautas
            .SelectMany(p => p.Votos)
            .Select(v => v.UnidadeId)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Count();

        return new AssembleiaDto(
            a.Id,
            a.TenantId,
            a.CondoId,
            a.Titulo,
            a.Descricao,
            a.Tipo,
            a.Status,
            a.DataInicio,
            a.DataFim,
            a.DataEncerramento,
            a.AtaTexto,
            a.AtaGeradaEm,
            a.CriadoPorUserId,
            a.Pautas.Count,
            quorumUnidades,
            pautasDto,
            a.DataCriacao,
            a.DataAtualizacao
        );
    }
}
