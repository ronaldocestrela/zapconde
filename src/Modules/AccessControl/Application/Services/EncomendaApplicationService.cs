using BuildingBlocks.Shared;
using BuildingBlocks.Shared.MultiTenancy;
using Microsoft.EntityFrameworkCore;
using Modules.AccessControl.Application.DTOs;
using Modules.AccessControl.Domain.Entities;
using Modules.AccessControl.Domain.Enums;
using Modules.AccessControl.Domain.Exceptions;
using Modules.AccessControl.Infrastructure.Persistence;

namespace Modules.AccessControl.Application.Services;

public class EncomendaApplicationService : IEncomendaApplicationService
{
    private readonly AccessControlDbContext _dbContext;
    private readonly ICurrentTenantService _currentTenantService;

    public EncomendaApplicationService(
        AccessControlDbContext dbContext,
        ICurrentTenantService currentTenantService)
    {
        _dbContext = dbContext;
        _currentTenantService = currentTenantService;
    }

    public async Task<Result<EncomendaDto>> RegistrarRecebimentoAsync(RegistrarRecebimentoEncomendaRequest request, CancellationToken ct = default)
    {
        var tenantId = _currentTenantService.TenantId ?? 1;
        var condoId = request.CondoId > 0 ? request.CondoId : (_currentTenantService.CondoId ?? 1);

        try
        {
            var dataRecebimento = request.DataRecebimento ?? DateTimeOffset.UtcNow;

            var encomenda = Encomenda.Criar(
                tenantId: tenantId,
                condoId: condoId,
                unidadeId: request.UnidadeId,
                blocoUnidade: request.BlocoUnidade,
                codigoRastreio: request.CodigoRastreio,
                descricao: request.Descricao,
                remetente: request.Remetente,
                transportadora: request.Transportadora,
                localArmazenamento: request.LocalArmazenamento,
                tipo: request.Tipo,
                recebidoPorNome: request.RecebidoPorNome,
                dataRecebimento: dataRecebimento,
                observacoes: request.Observacoes);

            if (!string.IsNullOrWhiteSpace(request.FotoEtiquetaUrl) || request.ConfiancaOcr.HasValue || !string.IsNullOrWhiteSpace(request.DadosOcrJson))
            {
                encomenda.AssociarMetadadosVision(request.FotoEtiquetaUrl, request.ConfiancaOcr, request.DadosOcrJson);
            }

            _dbContext.Encomendas.Add(encomenda);
            await _dbContext.SaveChangesAsync(ct);

            return Result<EncomendaDto>.Success(MapToDto(encomenda));
        }
        catch (EncomendaDomainException ex)
        {
            return Result<EncomendaDto>.ValidationFailure(new[] { ex.Message });
        }
        catch (Exception ex)
        {
            return Result<EncomendaDto>.Failure($"Erro ao registrar recebimento da encomenda: {ex.Message}");
        }
    }

    public async Task<Result<EncomendaDto>> RegistrarBaixaAsync(int id, RegistrarBaixaEncomendaRequest request, CancellationToken ct = default)
    {
        try
        {
            var encomenda = await _dbContext.Encomendas.FirstOrDefaultAsync(e => e.Id == id, ct);
            if (encomenda is null)
            {
                return Result<EncomendaDto>.Failure($"Encomenda com ID {id} não foi encontrada.");
            }

            var dataRetirada = request.DataRetirada ?? DateTimeOffset.UtcNow;
            encomenda.MarcarComoEntregue(request.RetiradoPorNome, dataRetirada);

            await _dbContext.SaveChangesAsync(ct);

            return Result<EncomendaDto>.Success(MapToDto(encomenda));
        }
        catch (EncomendaDomainException ex)
        {
            return Result<EncomendaDto>.ValidationFailure(new[] { ex.Message });
        }
        catch (Exception ex)
        {
            return Result<EncomendaDto>.Failure($"Erro ao registrar baixa da encomenda: {ex.Message}");
        }
    }

    public async Task<Result<EncomendaDto>> NotificarMoradorAsync(int id, CancellationToken ct = default)
    {
        try
        {
            var encomenda = await _dbContext.Encomendas.FirstOrDefaultAsync(e => e.Id == id, ct);
            if (encomenda is null)
            {
                return Result<EncomendaDto>.Failure($"Encomenda com ID {id} não foi encontrada.");
            }

            encomenda.NotificarMorador();
            await _dbContext.SaveChangesAsync(ct);

            return Result<EncomendaDto>.Success(MapToDto(encomenda));
        }
        catch (EncomendaDomainException ex)
        {
            return Result<EncomendaDto>.ValidationFailure(new[] { ex.Message });
        }
        catch (Exception ex)
        {
            return Result<EncomendaDto>.Failure($"Erro ao notificar morador: {ex.Message}");
        }
    }

    public async Task<Result<EncomendaDto>> CancelarAsync(int id, string motivo, CancellationToken ct = default)
    {
        try
        {
            var encomenda = await _dbContext.Encomendas.FirstOrDefaultAsync(e => e.Id == id, ct);
            if (encomenda is null)
            {
                return Result<EncomendaDto>.Failure($"Encomenda com ID {id} não foi encontrada.");
            }

            encomenda.Cancelar(motivo);
            await _dbContext.SaveChangesAsync(ct);

            return Result<EncomendaDto>.Success(MapToDto(encomenda));
        }
        catch (EncomendaDomainException ex)
        {
            return Result<EncomendaDto>.ValidationFailure(new[] { ex.Message });
        }
        catch (Exception ex)
        {
            return Result<EncomendaDto>.Failure($"Erro ao cancelar encomenda: {ex.Message}");
        }
    }

    public async Task<Result<EncomendaDto>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var encomenda = await _dbContext.Encomendas.FirstOrDefaultAsync(e => e.Id == id, ct);
        if (encomenda is null)
        {
            return Result<EncomendaDto>.Failure($"Encomenda com ID {id} não foi encontrada.");
        }

        return Result<EncomendaDto>.Success(MapToDto(encomenda));
    }

    public async Task<Result<IEnumerable<EncomendaDto>>> GetEncomendasAsync(
        StatusEncomenda? status = null,
        TipoEncomenda? tipo = null,
        int? unidadeId = null,
        string? busca = null,
        CancellationToken ct = default)
    {
        var query = _dbContext.Encomendas.AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(e => e.Status == status.Value);
        }

        if (tipo.HasValue)
        {
            query = query.Where(e => e.Tipo == tipo.Value);
        }

        if (unidadeId.HasValue && unidadeId.Value > 0)
        {
            query = query.Where(e => e.UnidadeId == unidadeId.Value);
        }

        if (!string.IsNullOrWhiteSpace(busca))
        {
            var term = busca.Trim().ToLower();
            query = query.Where(e =>
                e.CodigoRastreio.ToLower().Contains(term) ||
                e.Descricao.ToLower().Contains(term) ||
                (e.Remetente != null && e.Remetente.ToLower().Contains(term)) ||
                (e.Transportadora != null && e.Transportadora.ToLower().Contains(term)) ||
                e.BlocoUnidade.ToLower().Contains(term) ||
                (e.RetiradoPorNome != null && e.RetiradoPorNome.ToLower().Contains(term)));
        }

        var list = await query.OrderByDescending(e => e.DataRecebimento)
                             .ToListAsync(ct);

        var dtos = list.Select(MapToDto);
        return Result<IEnumerable<EncomendaDto>>.Success(dtos);
    }

    public async Task<Result<EncomendaSummaryDto>> GetSummaryAsync(CancellationToken ct = default)
    {
        var total = await _dbContext.Encomendas.CountAsync(ct);
        var aguardando = await _dbContext.Encomendas.CountAsync(e => e.Status == StatusEncomenda.AguardandoRetirada, ct);
        
        var hojeInicio = new DateTimeOffset(DateTime.UtcNow.Date, TimeSpan.Zero);
        var entreguesHoje = await _dbContext.Encomendas.CountAsync(e => 
            e.Status == StatusEncomenda.Entregue && 
            e.DataRetirada.HasValue && 
            e.DataRetirada.Value >= hojeInicio, ct);

        var pereciveis = await _dbContext.Encomendas.CountAsync(e => 
            e.Status == StatusEncomenda.AguardandoRetirada && 
            e.Tipo == TipoEncomenda.Perecivel, ct);

        var summary = new EncomendaSummaryDto(total, aguardando, entreguesHoje, pereciveis);
        return Result<EncomendaSummaryDto>.Success(summary);
    }

    private static EncomendaDto MapToDto(Encomenda e)
    {
        return new EncomendaDto(
            e.Id,
            e.TenantId,
            e.CondoId,
            e.UnidadeId,
            e.BlocoUnidade,
            e.CodigoRastreio,
            e.Descricao,
            e.Remetente,
            e.Transportadora,
            e.LocalArmazenamento,
            e.Tipo,
            e.Tipo.ToString(),
            e.Status,
            e.Status.ToString(),
            e.DataRecebimento,
            e.RecebidoPorNome,
            e.DataRetirada,
            e.RetiradoPorNome,
            e.NotificadoEm,
            e.Observacoes,
            e.FotoEtiquetaUrl,
            e.ConfiancaOcr,
            e.DadosOcrJson,
            e.CriadoEm,
            e.AtualizadoEm);
    }
}
