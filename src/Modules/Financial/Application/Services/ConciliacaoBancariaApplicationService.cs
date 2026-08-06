using BuildingBlocks.Shared.MultiTenancy;
using BuildingBlocks.Shared;
using Microsoft.EntityFrameworkCore;
using Modules.Financial.Application.DTOs;
using Modules.Financial.Domain.Entities;
using Modules.Financial.Domain.Enums;
using Modules.Financial.Domain.Services;
using Modules.Financial.Infrastructure.Persistence;

namespace Modules.Financial.Application.Services;

public class ConciliacaoBancariaApplicationService : IConciliacaoBancariaApplicationService
{
    private readonly FinancialDbContext _dbContext;
    private readonly ICurrentTenantService _currentTenantService;
    private readonly ConciliacaoBancariaDomainService _domainService;

    public ConciliacaoBancariaApplicationService(
        FinancialDbContext dbContext,
        ICurrentTenantService currentTenantService,
        ConciliacaoBancariaDomainService domainService)
    {
        _dbContext = dbContext;
        _currentTenantService = currentTenantService;
        _domainService = domainService;
    }

    public async Task<Result<ContaBancariaDto>> CriarContaBancariaAsync(CriarContaBancariaRequestDto request, CancellationToken ct = default)
    {
        int tenantId = _currentTenantService.TenantId ?? 1;

        var conta = ContaBancaria.Create(
            tenantId,
            request.CondoId,
            request.NomeBanco,
            request.CodigoBanco,
            request.Agencia,
            request.NumeroConta,
            request.TipoConta,
            request.SaldoInicial);

        _dbContext.ContasBancarias.Add(conta);
        await _dbContext.SaveChangesAsync(ct);

        return Result<ContaBancariaDto>.Success(MapToContaDto(conta));
    }

    public async Task<Result<IEnumerable<ContaBancariaDto>>> ListarContasBancariasAsync(int condoId, CancellationToken ct = default)
    {
        var contas = await _dbContext.ContasBancarias
            .Where(c => c.CondoId == condoId && c.Ativa)
            .ToListAsync(ct);

        return Result<IEnumerable<ContaBancariaDto>>.Success(contas.Select(MapToContaDto));
    }

    public async Task<Result<IEnumerable<ExtratoBancarioItemDto>>> ImportarExtratoAsync(ImportarExtratoRequestDto request, CancellationToken ct = default)
    {
        int tenantId = _currentTenantService.TenantId ?? 1;

        var conta = await _dbContext.ContasBancarias.FirstOrDefaultAsync(c => c.Id == request.ContaBancariaId, ct);
        if (conta == null)
            return Result<IEnumerable<ExtratoBancarioItemDto>>.Failure($"Conta bancária ID {request.ContaBancariaId} não encontrada.");

        var criados = new List<ExtratoBancarioItem>();
        foreach (var itemDto in request.Itens)
        {
            var item = ExtratoBancarioItem.Create(
                tenantId,
                request.ContaBancariaId,
                itemDto.DataTransacao,
                itemDto.DescricaoHistorico,
                itemDto.DocumentoRef,
                itemDto.Valor,
                itemDto.TipoTransacao);

            criados.Add(item);
            _dbContext.ExtratoBancarioItens.Add(item);
        }

        await _dbContext.SaveChangesAsync(ct);
        return Result<IEnumerable<ExtratoBancarioItemDto>>.Success(criados.Select(MapToItemDto));
    }

    public async Task<Result<ResultadoConciliacaoEmLoteDto>> ProcessarConciliacaoAutomaticaAsync(int contaBancariaId, CancellationToken ct = default)
    {
        var itensPendente = await _dbContext.ExtratoBancarioItens
            .Where(i => i.ContaBancariaId == contaBancariaId && i.StatusConciliacao == StatusConciliacaoBancaria.Pendente)
            .ToListAsync(ct);

        if (!itensPendente.Any())
        {
            return Result<ResultadoConciliacaoEmLoteDto>.Success(new ResultadoConciliacaoEmLoteDto(
                0, 0, 0, new List<ExtratoBancarioItemDto>()));
        }

        var faturasLiquidadas = await _dbContext.Faturas
            .Where(f => f.Status == StatusFatura.Pago)
            .ToListAsync(ct);

        var despesasBalancete = await _dbContext.ItensBalancete
            .Where(b => b.TipoLancamento == TipoLancamentoBalancete.Despesa && !b.Conciliado)
            .ToListAsync(ct);

        var matches = _domainService.ProcessarConciliacaoAutomatica(itensPendente, faturasLiquidadas, despesasBalancete);

        int conciliados = 0;
        foreach (var match in matches)
        {
            conciliados++;
            var record = ConciliacaoBancariaRecord.Create(
                _currentTenantService.TenantId ?? 1,
                match.ExtratoItem.Id,
                match.OrigemTipo,
                match.OrigemId,
                null,
                "Conciliado automaticamente pelo sistema.");

            _dbContext.ConciliacoesBancarias.Add(record);
        }

        await _dbContext.SaveChangesAsync(ct);

        int pendentes = itensPendente.Count(i => i.StatusConciliacao == StatusConciliacaoBancaria.Pendente);
        var conciliadosDtos = itensPendente.Where(i => i.StatusConciliacao == StatusConciliacaoBancaria.ConciliadoAutomatico).Select(MapToItemDto).ToList();

        return Result<ResultadoConciliacaoEmLoteDto>.Success(new ResultadoConciliacaoEmLoteDto(
            itensPendente.Count,
            conciliados,
            pendentes,
            conciliadosDtos));
    }

    public async Task<Result<IEnumerable<ExtratoBancarioItemDto>>> ListarItensPendentesAsync(int contaBancariaId, CancellationToken ct = default)
    {
        var itens = await _dbContext.ExtratoBancarioItens
            .Where(i => i.ContaBancariaId == contaBancariaId && i.StatusConciliacao != StatusConciliacaoBancaria.ConciliadoAutomatico && i.StatusConciliacao != StatusConciliacaoBancaria.ConciliadoManual)
            .OrderByDescending(i => i.DataTransacao)
            .ToListAsync(ct);

        return Result<IEnumerable<ExtratoBancarioItemDto>>.Success(itens.Select(MapToItemDto));
    }

    public async Task<Result<ExtratoBancarioItemDto>> ConciliarManualAsync(ConciliarManualRequestDto request, CancellationToken ct = default)
    {
        var item = await _dbContext.ExtratoBancarioItens.FirstOrDefaultAsync(i => i.Id == request.ExtratoBancarioItemId, ct);
        if (item == null)
            return Result<ExtratoBancarioItemDto>.Failure($"Item de extrato ID {request.ExtratoBancarioItemId} não encontrado.");

        item.ConciliarManual(request.OrigemId);

        var record = ConciliacaoBancariaRecord.Create(
            _currentTenantService.TenantId ?? 1,
            item.Id,
            request.OrigemTipo,
            request.OrigemId,
            request.ConciliadoPorUserId,
            request.Observacoes);

        _dbContext.ConciliacoesBancarias.Add(record);
        await _dbContext.SaveChangesAsync(ct);

        return Result<ExtratoBancarioItemDto>.Success(MapToItemDto(item));
    }

    private static ContaBancariaDto MapToContaDto(ContaBancaria conta) =>
        new(conta.Id, conta.TenantId, conta.CondoId, conta.NomeBanco, conta.CodigoBanco, conta.Agencia, conta.NumeroConta, conta.TipoConta, conta.SaldoAtual, conta.Ativa);

    private static ExtratoBancarioItemDto MapToItemDto(ExtratoBancarioItem item) =>
        new(item.Id, item.TenantId, item.ContaBancariaId, item.DataTransacao, item.DescricaoHistorico, item.DocumentoRef, item.Valor, item.TipoTransacao, item.StatusConciliacao, item.TransacaoConciliadaId, item.ScoreConciliacao);
}
