using BuildingBlocks.Shared;
using BuildingBlocks.Shared.MultiTenancy;
using Microsoft.EntityFrameworkCore;
using Modules.Operations.Application.DTOs;
using Modules.Operations.Domain.Entities;
using Modules.Operations.Domain.Enums;
using Modules.Operations.Domain.Exceptions;
using Modules.Operations.Infrastructure.Persistence;

namespace Modules.Operations.Application.Services;

public class PlanoManutencaoApplicationService : IPlanoManutencaoApplicationService
{
    private readonly OperationsDbContext _dbContext;
    private readonly ICurrentTenantService _currentTenantService;

    public PlanoManutencaoApplicationService(
        OperationsDbContext dbContext,
        ICurrentTenantService currentTenantService)
    {
        _dbContext = dbContext;
        _currentTenantService = currentTenantService;
    }

    public async Task<Result<PlanoManutencaoDto>> CriarPlanoAsync(
        CreatePlanoManutencaoRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var tenantId = _currentTenantService.TenantId;
            if (!tenantId.HasValue || tenantId.Value <= 0)
            {
                return Result<PlanoManutencaoDto>.Failure("TenantId não especificado no contexto de execução.");
            }

            var plano = PlanoManutencao.Create(
                tenantId.Value,
                request.CondoId,
                request.Titulo,
                request.Categoria,
                request.Periodicidade,
                request.DataProximaManutencao,
                request.Descricao,
                request.ResponsavelTecnico,
                request.EmpresaContratada,
                request.CustoEstimado,
                request.DataUltimaManutencao,
                request.Observacoes);

            _dbContext.PlanosManutencao.Add(plano);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return Result<PlanoManutencaoDto>.Success(MapToDto(plano));
        }
        catch (PlanoManutencaoDomainException ex)
        {
            return Result<PlanoManutencaoDto>.ValidationFailure(new[] { ex.Message });
        }
        catch (Exception ex)
        {
            return Result<PlanoManutencaoDto>.Failure($"Erro ao criar plano de manutenção: {ex.Message}");
        }
    }

    public async Task<Result<PlanoManutencaoDto>> AtualizarPlanoAsync(
        Guid id,
        UpdatePlanoManutencaoRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var plano = await _dbContext.PlanosManutencao
                .FirstOrDefaultAsync(p => p.Id == id && p.Ativo, cancellationToken);

            if (plano is null)
            {
                return Result<PlanoManutencaoDto>.Failure("Plano de manutenção não encontrado.");
            }

            plano.Atualizar(
                request.Titulo,
                request.Descricao,
                request.Categoria,
                request.Periodicidade,
                request.DataProximaManutencao,
                request.ResponsavelTecnico,
                request.EmpresaContratada,
                request.CustoEstimado,
                request.Observacoes);

            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result<PlanoManutencaoDto>.Success(MapToDto(plano));
        }
        catch (PlanoManutencaoDomainException ex)
        {
            return Result<PlanoManutencaoDto>.ValidationFailure(new[] { ex.Message });
        }
        catch (Exception ex)
        {
            return Result<PlanoManutencaoDto>.Failure($"Erro ao atualizar plano de manutenção: {ex.Message}");
        }
    }

    public async Task<Result<PlanoManutencaoDto>> ConcluirManutencaoAsync(
        Guid id,
        ConcluirManutencaoRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var plano = await _dbContext.PlanosManutencao
                .FirstOrDefaultAsync(p => p.Id == id && p.Ativo, cancellationToken);

            if (plano is null)
            {
                return Result<PlanoManutencaoDto>.Failure("Plano de manutenção não encontrado.");
            }

            plano.ConcluirManutencao(
                request.DataRealizacao,
                request.CustoReal,
                request.Observacoes,
                request.AgendarProxima);

            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result<PlanoManutencaoDto>.Success(MapToDto(plano));
        }
        catch (PlanoManutencaoDomainException ex)
        {
            return Result<PlanoManutencaoDto>.ValidationFailure(new[] { ex.Message });
        }
        catch (Exception ex)
        {
            return Result<PlanoManutencaoDto>.Failure($"Erro ao registrar conclusão de manutenção: {ex.Message}");
        }
    }

    public async Task<Result<PlanoManutencaoDto>> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var plano = await _dbContext.PlanosManutencao
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

            if (plano is null)
            {
                return Result<PlanoManutencaoDto>.Failure("Plano de manutenção não encontrado.");
            }

            plano.CalcularStatus(DateTime.Today);
            return Result<PlanoManutencaoDto>.Success(MapToDto(plano));
        }
        catch (Exception ex)
        {
            return Result<PlanoManutencaoDto>.Failure($"Erro ao buscar plano de manutenção: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<PlanoManutencaoDto>>> ListarAsync(
        int condoId,
        CategoriaManutencao? categoria = null,
        StatusManutencao? status = null,
        PeriodicidadeManutencao? periodicidade = null,
        DateTime? inicio = null,
        DateTime? fim = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _dbContext.PlanosManutencao
                .AsNoTracking()
                .Where(p => p.CondoId == condoId && p.Ativo);

            if (categoria.HasValue)
                query = query.Where(p => p.Categoria == categoria.Value);

            if (periodicidade.HasValue)
                query = query.Where(p => p.Periodicidade == periodicidade.Value);

            if (inicio.HasValue)
                query = query.Where(p => p.DataProximaManutencao >= inicio.Value.Date);

            if (fim.HasValue)
                query = query.Where(p => p.DataProximaManutencao <= fim.Value.Date);

            var planos = await query.ToListAsync(cancellationToken);

            // Recalcula status dinâmico para amostragem atualizada
            foreach (var plano in planos)
            {
                plano.CalcularStatus(DateTime.Today);
            }

            if (status.HasValue)
            {
                planos = planos.Where(p => p.Status == status.Value).ToList();
            }

            var dtos = planos.OrderBy(p => p.DataProximaManutencao).Select(MapToDto);
            return Result<IEnumerable<PlanoManutencaoDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<PlanoManutencaoDto>>.Failure($"Erro ao listar planos de manutenção: {ex.Message}");
        }
    }

    public async Task<Result<PlanoManutencaoSummaryDto>> ObterResumoMetricasAsync(int condoId, CancellationToken cancellationToken = default)
    {
        try
        {
            var planos = await _dbContext.PlanosManutencao
                .AsNoTracking()
                .Where(p => p.CondoId == condoId && p.Ativo)
                .ToListAsync(cancellationToken);

            foreach (var plano in planos)
            {
                plano.CalcularStatus(DateTime.Today);
            }

            var total = planos.Count;
            var emDia = planos.Count(p => p.Status == StatusManutencao.EmDia);
            var proximas = planos.Count(p => p.Status == StatusManutencao.Proxima);
            var atrasadas = planos.Count(p => p.Status == StatusManutencao.Atrasada);
            var emExecucao = planos.Count(p => p.Status == StatusManutencao.EmExecucao);
            var custoEstimado = planos.Sum(p => p.CustoEstimado ?? 0);
            var custoReal = planos.Sum(p => p.CustoReal ?? 0);

            var summary = new PlanoManutencaoSummaryDto(total, emDia, proximas, atrasadas, emExecucao, custoEstimado, custoReal);
            return Result<PlanoManutencaoSummaryDto>.Success(summary);
        }
        catch (Exception ex)
        {
            return Result<PlanoManutencaoSummaryDto>.Failure($"Erro ao gerar resumo de manutenção: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<ManutencaoCalendarEventDto>>> ObterEventosCalendarioAsync(
        int condoId,
        DateTime? inicio = null,
        DateTime? fim = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _dbContext.PlanosManutencao
                .AsNoTracking()
                .Where(p => p.CondoId == condoId && p.Ativo);

            if (inicio.HasValue)
                query = query.Where(p => p.DataProximaManutencao >= inicio.Value.Date);

            if (fim.HasValue)
                query = query.Where(p => p.DataProximaManutencao <= fim.Value.Date);

            var planos = await query.ToListAsync(cancellationToken);

            var eventos = planos.Select(p =>
            {
                p.CalcularStatus(DateTime.Today);
                return new ManutencaoCalendarEventDto(
                    p.Id,
                    p.Titulo,
                    p.Categoria,
                    p.Status,
                    p.DataProximaManutencao,
                    p.EmpresaContratada);
            }).OrderBy(e => e.Data);

            return Result<IEnumerable<ManutencaoCalendarEventDto>>.Success(eventos);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<ManutencaoCalendarEventDto>>.Failure($"Erro ao carregar calendário de manutenções: {ex.Message}");
        }
    }

    private static PlanoManutencaoDto MapToDto(PlanoManutencao plano)
    {
        return new PlanoManutencaoDto(
            plano.Id,
            plano.TenantId,
            plano.CondoId,
            plano.Titulo,
            plano.Descricao,
            plano.Categoria,
            plano.Periodicidade,
            plano.DataUltimaManutencao,
            plano.DataProximaManutencao,
            plano.Status,
            plano.ResponsavelTecnico,
            plano.EmpresaContratada,
            plano.CustoEstimado,
            plano.CustoReal,
            plano.Observacoes,
            plano.Ativo,
            plano.DataCriacao,
            plano.DataAtualizacao
        );
    }
}
