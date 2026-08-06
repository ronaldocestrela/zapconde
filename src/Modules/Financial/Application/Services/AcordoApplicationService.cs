using BuildingBlocks.Shared.MultiTenancy;
using BuildingBlocks.Shared;
using Microsoft.EntityFrameworkCore;
using Modules.Financial.Application.Dtos;
using Modules.Financial.Domain.Entities;
using Modules.Financial.Domain.Enums;
using Modules.Financial.Domain.Services;
using Modules.Financial.Infrastructure.Persistence;

namespace Modules.Financial.Application.Services;

public class AcordoApplicationService : IAcordoApplicationService
{
    private readonly FinancialDbContext _dbContext;
    private readonly ICurrentTenantService _currentTenantService;
    private readonly CalculadoraAcordoDomainService _calculadoraAcordo;

    public AcordoApplicationService(
        FinancialDbContext dbContext,
        ICurrentTenantService currentTenantService,
        CalculadoraAcordoDomainService calculadoraAcordo)
    {
        _dbContext = dbContext;
        _currentTenantService = currentTenantService;
        _calculadoraAcordo = calculadoraAcordo;
    }

    public async Task<Result<SimulacaoAcordoResponse>> SimularAcordoAsync(SimulacaoAcordoRequest request, CancellationToken ct = default)
    {
        if (request.FaturasIds == null || !request.FaturasIds.Any())
            return Result<SimulacaoAcordoResponse>.ValidationFailure(new[] { "Pelo menos uma fatura deve ser selecionada para renegociação." });

        if (request.QuantidadeParcelas <= 0)
            return Result<SimulacaoAcordoResponse>.ValidationFailure(new[] { "Quantidade de parcelas deve ser maior que zero." });

        var faturas = await _dbContext.Faturas
            .Where(f => request.FaturasIds.Contains(f.Id) && f.UnidadeId == request.UnidadeId)
            .ToListAsync(ct);

        if (!faturas.Any())
            return Result<SimulacaoAcordoResponse>.Failure("Nenhuma fatura válida encontrada para os IDs informados.");

        var valorTotalOriginal = faturas.Sum(f => f.TotalFinal);
        var resumoCalculado = _calculadoraAcordo.SimularAcordo(
            valorTotalOriginal,
            request.ValorDescontoConcedido,
            request.QuantidadeParcelas,
            request.DataPrimeiroVencimento
        );

        var response = new SimulacaoAcordoResponse(
            ValorTotalOriginal: resumoCalculado.ValorTotalOriginal,
            ValorDesconto: resumoCalculado.ValorDesconto,
            ValorTotalAcordo: resumoCalculado.ValorTotalAcordo,
            QuantidadeParcelas: resumoCalculado.QuantidadeParcelas,
            ValorParcelaBase: resumoCalculado.ValorParcelaBase,
            Parcelas: resumoCalculado.Parcelas.Select(p => new ProjecaoParcelaDto(p.NumeroParcela, p.DataVencimento, p.ValorParcela)).ToList()
        );

        return Result<SimulacaoAcordoResponse>.Success(response);
    }

    public async Task<Result<AcordoDto>> CriarAcordoAsync(CriarAcordoRequest request, CancellationToken ct = default)
    {
        var tenantId = _currentTenantService.TenantId ?? 1;

        if (request.FaturasIds == null || !request.FaturasIds.Any())
            return Result<AcordoDto>.ValidationFailure(new[] { "Pelo menos uma fatura deve ser informada." });

        var faturas = await _dbContext.Faturas
            .Where(f => request.FaturasIds.Contains(f.Id) && f.UnidadeId == request.UnidadeId)
            .ToListAsync(ct);

        if (!faturas.Any())
            return Result<AcordoDto>.Failure("Faturas informadas não foram encontradas para esta unidade.");

        var valorTotalOriginal = faturas.Sum(f => f.TotalFinal);
        var resumoCalculado = _calculadoraAcordo.SimularAcordo(
            valorTotalOriginal,
            request.ValorDescontoConcedido,
            request.QuantidadeParcelas,
            request.DataPrimeiroVencimento
        );

        var acordo = Acordo.Create(
            tenantId,
            request.CondoId,
            request.UnidadeId,
            request.MoradorId,
            request.DataPrimeiroVencimento,
            valorTotalOriginal,
            resumoCalculado.ValorDesconto,
            request.QuantidadeParcelas,
            request.Observacoes
        );

        // Vincula faturas originais e atualiza o status delas para EmAcordo
        foreach (var fatura in faturas)
        {
            acordo.VincularFaturaOriginal(fatura.Id, fatura.TotalFinal);
            fatura.Status = StatusFatura.EmAcordo;
        }

        // Gera as parcelas do acordo
        foreach (var projecao in resumoCalculado.Parcelas)
        {
            var parcela = ParcelaAcordo.Create(
                tenantId,
                acordo.Id,
                projecao.NumeroParcela,
                projecao.DataVencimento,
                projecao.ValorParcela
            );
            acordo.AdicionarParcela(parcela);
        }

        // Efetiva o acordo
        acordo.EfetivarAcordo(DateTime.UtcNow);

        _dbContext.Acordos.Add(acordo);
        await _dbContext.SaveChangesAsync(ct);

        return Result<AcordoDto>.Success(MapearParaDto(acordo), "Acordo de renegociação efetivado com sucesso.");
    }

    public async Task<Result<IEnumerable<AcordoDto>>> ObterAcordosPorCondominioAsync(int condoId, int? unidadeId = null, StatusAcordo? status = null, CancellationToken ct = default)
    {
        var query = _dbContext.Acordos
            .Include(a => a.Parcelas)
            .Include(a => a.FaturasVinculadas)
            .Where(a => a.CondoId == condoId);

        if (unidadeId.HasValue)
            query = query.Where(a => a.UnidadeId == unidadeId.Value);

        if (status.HasValue)
            query = query.Where(a => a.Status == status.Value);

        var acordos = await query.OrderByDescending(a => a.DataCriacao).ToListAsync(ct);
        var dtos = acordos.Select(MapearParaDto);
        return Result<IEnumerable<AcordoDto>>.Success(dtos);
    }

    public async Task<Result<AcordoDto>> ObterDetalhesAcordoAsync(int acordoId, CancellationToken ct = default)
    {
        var acordo = await _dbContext.Acordos
            .Include(a => a.Parcelas)
            .Include(a => a.FaturasVinculadas)
            .FirstOrDefaultAsync(a => a.Id == acordoId, ct);

        if (acordo == null)
            return Result<AcordoDto>.Failure("Acordo não encontrado.");

        return Result<AcordoDto>.Success(MapearParaDto(acordo));
    }

    public async Task<Result> CancelarAcordoAsync(int acordoId, string motivo, CancellationToken ct = default)
    {
        var acordo = await _dbContext.Acordos
            .Include(a => a.Parcelas)
            .Include(a => a.FaturasVinculadas)
            .FirstOrDefaultAsync(a => a.Id == acordoId, ct);

        if (acordo == null)
            return Result.Failure("Acordo não encontrado.");

        acordo.Cancelar();

        // Retorna faturas originais para o status Vencido
        var faturasIds = acordo.FaturasVinculadas.Select(fv => fv.FaturaId).ToList();
        var faturas = await _dbContext.Faturas.Where(f => faturasIds.Contains(f.Id)).ToListAsync(ct);
        foreach (var fatura in faturas)
        {
            fatura.Status = StatusFatura.Vencido;
        }

        await _dbContext.SaveChangesAsync(ct);
        return Result.Success("Acordo cancelado com sucesso.");
    }

    public async Task<Result> RegistrarPagamentoParcelaAsync(int acordoId, int numeroParcela, DateTime dataPagamento, CancellationToken ct = default)
    {
        var acordo = await _dbContext.Acordos
            .Include(a => a.Parcelas)
            .Include(a => a.FaturasVinculadas)
            .FirstOrDefaultAsync(a => a.Id == acordoId, ct);

        if (acordo == null)
            return Result.Failure("Acordo não encontrado.");

        acordo.RegistrarPagamentoParcela(numeroParcela, dataPagamento);

        // Se o acordo foi quitado, marca faturas originais como Pagas
        if (acordo.Status == StatusAcordo.Quitado)
        {
            var faturasIds = acordo.FaturasVinculadas.Select(fv => fv.FaturaId).ToList();
            var faturas = await _dbContext.Faturas.Where(f => faturasIds.Contains(f.Id)).ToListAsync(ct);
            foreach (var fatura in faturas)
            {
                fatura.Status = StatusFatura.Pago;
                fatura.DataPagamento = dataPagamento;
            }
        }

        await _dbContext.SaveChangesAsync(ct);
        return Result.Success("Pagamento de parcela registrado com sucesso.");
    }

    public async Task<Result> MarcarAcordoDescumpridoAsync(int acordoId, CancellationToken ct = default)
    {
        var acordo = await _dbContext.Acordos
            .Include(a => a.Parcelas)
            .Include(a => a.FaturasVinculadas)
            .FirstOrDefaultAsync(a => a.Id == acordoId, ct);

        if (acordo == null)
            return Result.Failure("Acordo não encontrado.");

        acordo.MarcarDescumprido();

        // Retorna faturas originais consolidadas para o status Vencido
        var faturasIds = acordo.FaturasVinculadas.Select(fv => fv.FaturaId).ToList();
        var faturas = await _dbContext.Faturas.Where(f => faturasIds.Contains(f.Id)).ToListAsync(ct);
        foreach (var fatura in faturas)
        {
            fatura.Status = StatusFatura.Vencido;
        }

        await _dbContext.SaveChangesAsync(ct);
        return Result.Success("Acordo marcado como descumprido e faturas originais reativadas como vencidas.");
    }

    private static AcordoDto MapearParaDto(Acordo acordo)
    {
        return new AcordoDto(
            Id: acordo.Id,
            TenantId: acordo.TenantId,
            CondoId: acordo.CondoId,
            UnidadeId: acordo.UnidadeId,
            MoradorId: acordo.MoradorId,
            NumeroAcordo: acordo.NumeroAcordo,
            DataCriacao: acordo.DataCriacao,
            DataAceite: acordo.DataAceite,
            DataPrimeiroVencimento: acordo.DataPrimeiroVencimento,
            ValorTotalOriginal: acordo.ValorTotalOriginal,
            ValorDesconto: acordo.ValorDesconto,
            ValorTotalAcordo: acordo.ValorTotalAcordo,
            QuantidadeParcelas: acordo.QuantidadeParcelas,
            Status: acordo.Status,
            Observacoes: acordo.Observacoes,
            Parcelas: acordo.Parcelas.Select(p => new ParcelaAcordoDto(
                p.Id, p.NumeroParcela, p.DataVencimento, p.ValorParcela, p.Status, p.DataPagamento, p.FaturaGeradaId
            )).ToList(),
            FaturasVinculadas: acordo.FaturasVinculadas.Select(fv => new AcordoFaturaVinculadaDto(
                fv.FaturaId, fv.ValorFaturaOriginal
            )).ToList()
        );
    }
}
