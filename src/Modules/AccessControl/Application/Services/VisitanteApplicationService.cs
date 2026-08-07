using BuildingBlocks.Shared.MultiTenancy;
using BuildingBlocks.Shared;
using Microsoft.EntityFrameworkCore;
using Modules.AccessControl.Application.DTOs;
using Modules.AccessControl.Domain.Entities;
using Modules.AccessControl.Domain.Enums;
using Modules.AccessControl.Domain.Exceptions;
using Modules.AccessControl.Infrastructure.Persistence;

namespace Modules.AccessControl.Application.Services;

public class VisitanteApplicationService : IVisitanteApplicationService
{
    private readonly AccessControlDbContext _dbContext;
    private readonly ICurrentTenantService _currentTenantService;

    public VisitanteApplicationService(
        AccessControlDbContext dbContext,
        ICurrentTenantService currentTenantService)
    {
        _dbContext = dbContext;
        _currentTenantService = currentTenantService;
    }

    public async Task<Result<VisitanteDto>> AuthorizeVisitanteAsync(CreateVisitanteRequestDto request, CancellationToken ct = default)
    {
        var tenantId = _currentTenantService.TenantId ?? 1;
        var condoId = _currentTenantService.CondoId ?? 1;

        try
        {
            var visitante = Visitante.CriarAutorizacao(
                tenantId: tenantId,
                condoId: condoId,
                nomeCompleto: request.NomeCompleto,
                documento: request.Documento,
                telefone: request.Telefone,
                tipo: request.Tipo,
                unidadeId: request.UnidadeId,
                blocoUnidade: request.BlocoUnidade ?? $"Unidade {request.UnidadeId}",
                moradorId: request.MoradorId,
                dataHoraInicioAutorizacao: request.DataHoraInicioAutorizacao,
                dataHoraFimAutorizacao: request.DataHoraFimAutorizacao,
                empresa: request.Empresa,
                placaVeiculo: request.PlacaVeiculo,
                observacoes: request.Observacoes
            );

            if (request.RegistrarEntradaImediata)
            {
                visitante.RegistrarEntrada();
            }

            _dbContext.Visitantes.Add(visitante);
            await _dbContext.SaveChangesAsync(ct);

            return Result<VisitanteDto>.Success(VisitanteDto.FromDomain(visitante));
        }
        catch (VisitanteDomainException ex)
        {
            return Result<VisitanteDto>.ValidationFailure(new[] { ex.Message });
        }
        catch (Exception ex)
        {
            return Result<VisitanteDto>.Failure($"Erro ao cadastrar visitante: {ex.Message}");
        }
    }

    public async Task<Result<VisitanteDto>> RegistrarEntradaAsync(int id, int? operadorId = null, CancellationToken ct = default)
    {
        var visitante = await _dbContext.Visitantes.FirstOrDefaultAsync(v => v.Id == id, ct);
        if (visitante == null)
        {
            return Result<VisitanteDto>.Failure($"Cadastro de visitante com ID {id} não foi encontrado.");
        }

        try
        {
            visitante.RegistrarEntrada(operadorId);
            await _dbContext.SaveChangesAsync(ct);
            return Result<VisitanteDto>.Success(VisitanteDto.FromDomain(visitante));
        }
        catch (VisitanteDomainException ex)
        {
            return Result<VisitanteDto>.ValidationFailure(new[] { ex.Message });
        }
        catch (Exception ex)
        {
            return Result<VisitanteDto>.Failure($"Erro ao registrar entrada: {ex.Message}");
        }
    }

    public async Task<Result<VisitanteDto>> RegistrarSaidaAsync(int id, int? operadorId = null, CancellationToken ct = default)
    {
        var visitante = await _dbContext.Visitantes.FirstOrDefaultAsync(v => v.Id == id, ct);
        if (visitante == null)
        {
            return Result<VisitanteDto>.Failure($"Cadastro de visitante com ID {id} não foi encontrado.");
        }

        try
        {
            visitante.RegistrarSaida(operadorId);
            await _dbContext.SaveChangesAsync(ct);
            return Result<VisitanteDto>.Success(VisitanteDto.FromDomain(visitante));
        }
        catch (VisitanteDomainException ex)
        {
            return Result<VisitanteDto>.ValidationFailure(new[] { ex.Message });
        }
        catch (Exception ex)
        {
            return Result<VisitanteDto>.Failure($"Erro ao registrar saída: {ex.Message}");
        }
    }

    public async Task<Result<VisitanteDto>> CancelarAutorizacaoAsync(int id, string? motivo = null, CancellationToken ct = default)
    {
        var visitante = await _dbContext.Visitantes.FirstOrDefaultAsync(v => v.Id == id, ct);
        if (visitante == null)
        {
            return Result<VisitanteDto>.Failure($"Cadastro de visitante com ID {id} não foi encontrado.");
        }

        try
        {
            visitante.CancelarAutorizacao(motivo);
            await _dbContext.SaveChangesAsync(ct);
            return Result<VisitanteDto>.Success(VisitanteDto.FromDomain(visitante));
        }
        catch (VisitanteDomainException ex)
        {
            return Result<VisitanteDto>.ValidationFailure(new[] { ex.Message });
        }
        catch (Exception ex)
        {
            return Result<VisitanteDto>.Failure($"Erro ao cancelar autorização: {ex.Message}");
        }
    }

    public async Task<Result<VisitanteDto>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var visitante = await _dbContext.Visitantes.AsNoTracking().FirstOrDefaultAsync(v => v.Id == id, ct);
        if (visitante == null)
        {
            return Result<VisitanteDto>.Failure($"Visitante ID {id} não encontrado.");
        }

        return Result<VisitanteDto>.Success(VisitanteDto.FromDomain(visitante));
    }

    public async Task<Result<IEnumerable<VisitanteDto>>> GetVisitantesAsync(
        TipoVisitante? tipo = null,
        StatusVisitante? status = null,
        int? unidadeId = null,
        string? busca = null,
        CancellationToken ct = default)
    {
        var query = _dbContext.Visitantes.AsNoTracking().AsQueryable();

        if (tipo.HasValue)
        {
            query = query.Where(v => v.Tipo == tipo.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(v => v.Status == status.Value);
        }

        if (unidadeId.HasValue && unidadeId.Value > 0)
        {
            query = query.Where(v => v.UnidadeId == unidadeId.Value);
        }

        if (!string.IsNullOrWhiteSpace(busca))
        {
            var termo = busca.Trim().ToLower();
            query = query.Where(v => v.NomeCompleto.ToLower().Contains(termo) ||
                                     v.Documento.ToLower().Contains(termo) ||
                                     (v.Empresa != null && v.Empresa.ToLower().Contains(termo)) ||
                                     v.BlocoUnidade.ToLower().Contains(termo) ||
                                     (v.PlacaVeiculo != null && v.PlacaVeiculo.ToLower().Contains(termo)));
        }

        var lista = await query
            .OrderByDescending(v => v.CriadoEm)
            .ToListAsync(ct);

        var dtos = lista.Select(VisitanteDto.FromDomain);
        return Result<IEnumerable<VisitanteDto>>.Success(dtos);
    }

    public async Task<Result<VisitanteSummaryDto>> GetSummaryAsync(CancellationToken ct = default)
    {
        var hojeInicio = DateTimeOffset.UtcNow.Date;
        var hojeFim = hojeInicio.AddDays(1);

        var visitantes = await _dbContext.Visitantes.AsNoTracking().ToListAsync(ct);

        var totalHoje = visitantes.Count(v => v.CriadoEm >= hojeInicio && v.CriadoEm < hojeFim);
        var presentesAgora = visitantes.Count(v => v.Status == StatusVisitante.Presente);
        var agendadosPendentes = visitantes.Count(v => v.Status == StatusVisitante.Agendado);
        var entradasHoje = visitantes.Count(v => v.DataHoraEntrada.HasValue && v.DataHoraEntrada.Value >= hojeInicio && v.DataHoraEntrada.Value < hojeFim);
        var saidasHoje = visitantes.Count(v => v.DataHoraSaida.HasValue && v.DataHoraSaida.Value >= hojeInicio && v.DataHoraSaida.Value < hojeFim);

        var summary = new VisitanteSummaryDto(
            TotalHoje: totalHoje,
            PresentesAgora: presentesAgora,
            AgendadosPendentes: agendadosPendentes,
            EntradasHoje: entradasHoje,
            SaidasHoje: saidasHoje
        );

        return Result<VisitanteSummaryDto>.Success(summary);
    }
}
