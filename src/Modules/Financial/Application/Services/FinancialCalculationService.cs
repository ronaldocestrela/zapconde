using BuildingBlocks.Shared;
using BuildingBlocks.Shared.MultiTenancy;
using Microsoft.EntityFrameworkCore;
using Modules.Financial.Application.DTOs;
using Modules.Financial.Domain.Services;
using Modules.Financial.Domain.ValueObjects;
using Modules.Financial.Infrastructure.Persistence;

namespace Modules.Financial.Application.Services;

/// <summary>
/// Implementação do serviço de cálculo financeiro e projeções com Result Pattern e isolamento Multi-Tenant.
/// </summary>
public class FinancialCalculationService : IFinancialCalculationService
{
    private readonly FinancialDbContext _dbContext;
    private readonly ICurrentTenantService _currentTenantService;
    private readonly CalculadoraFinanceira _calculadora;

    public FinancialCalculationService(
        FinancialDbContext dbContext,
        ICurrentTenantService currentTenantService,
        CalculadoraFinanceira calculadora)
    {
        _dbContext = dbContext;
        _currentTenantService = currentTenantService;
        _calculadora = calculadora;
    }

    public Task<Result<CalculoFinanceiroDto>> CalcularSimulacaoAsync(SimularCalculoRequestDto dto, CancellationToken ct = default)
    {
        if (dto == null)
        {
            return Task.FromResult(Result<CalculoFinanceiroDto>.ValidationFailure(new[] { "Dados de simulação nulos." }));
        }

        if (dto.ValorOriginal <= 0)
        {
            return Task.FromResult(Result<CalculoFinanceiroDto>.ValidationFailure(new[] { "Valor original deve ser maior que zero." }));
        }

        if (dto.PercentualMulta < 0)
        {
            return Task.FromResult(Result<CalculoFinanceiroDto>.ValidationFailure(new[] { "Percentual de multa não pode ser negativo." }));
        }

        if (dto.PercentualJurosMensal < 0)
        {
            return Task.FromResult(Result<CalculoFinanceiroDto>.ValidationFailure(new[] { "Percentual de juros mensal não pode ser negativo." }));
        }

        try
        {
            var parametros = new ParametrosCalculoFinanceiro(
                valorOriginal: dto.ValorOriginal,
                dataVencimento: dto.DataVencimento,
                dataCalculo: dto.DataSimulacao,
                percentualMulta: dto.PercentualMulta,
                percentualJurosMensal: dto.PercentualJurosMensal,
                valorDescontoPontualidade: dto.ValorDescontoPontualidade,
                percentualDescontoPontualidade: dto.PercentualDescontoPontualidade,
                dataLimiteDesconto: dto.DataLimiteDesconto
            );

            var resultado = _calculadora.CalcularEncargos(parametros);

            var resultDto = new CalculoFinanceiroDto(
                ValorOriginal: resultado.ValorOriginal,
                DataVencimento: resultado.DataVencimento,
                DataCalculo: resultado.DataCalculo,
                DiasAtraso: resultado.DiasAtraso,
                ValorMulta: resultado.ValorMulta,
                ValorJuros: resultado.ValorJuros,
                ValorDesconto: resultado.ValorDesconto,
                ValorTotalCalculado: resultado.ValorTotalCalculado,
                MemoriaCalculoTextual: resultado.MemoriaCalculoTextual
            );

            return Task.FromResult(Result<CalculoFinanceiroDto>.Success(resultDto));
        }
        catch (Exception ex)
        {
            return Task.FromResult(Result<CalculoFinanceiroDto>.Failure($"Erro ao calcular simulação: {ex.Message}"));
        }
    }

    public async Task<Result<CalculoFinanceiroDto>> SimularFaturaExistenteAsync(
        int faturaId,
        DateTime dataSimulacao,
        int tenantId,
        CancellationToken ct = default)
    {
        if (faturaId <= 0)
        {
            return Result<CalculoFinanceiroDto>.ValidationFailure(new[] { "ID da fatura inválido." });
        }

        var fatura = await _dbContext.Faturas
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == faturaId, ct);

        if (fatura == null)
        {
            return Result<CalculoFinanceiroDto>.Failure($"Fatura ID {faturaId} não encontrada ou inacessível no tenant atual.");
        }

        var parametros = new ParametrosCalculoFinanceiro(
            valorOriginal: fatura.ValorOriginal > 0 ? fatura.ValorOriginal : 1m,
            dataVencimento: fatura.DataVencimento,
            dataCalculo: dataSimulacao,
            valorDescontoPontualidade: fatura.ValorDesconto
        );

        var resultado = _calculadora.CalcularEncargos(parametros);

        var resultDto = new CalculoFinanceiroDto(
            ValorOriginal: resultado.ValorOriginal,
            DataVencimento: resultado.DataVencimento,
            DataCalculo: resultado.DataCalculo,
            DiasAtraso: resultado.DiasAtraso,
            ValorMulta: resultado.ValorMulta,
            ValorJuros: resultado.ValorJuros,
            ValorDesconto: resultado.ValorDesconto,
            ValorTotalCalculado: resultado.ValorTotalCalculado,
            MemoriaCalculoTextual: resultado.MemoriaCalculoTextual
        );

        return Result<CalculoFinanceiroDto>.Success(resultDto);
    }

    public async Task<Result<IEnumerable<ProjecaoCalculoDto>>> ObterProjecaoFuturaAsync(
        int faturaId,
        int tenantId,
        CancellationToken ct = default)
    {
        if (faturaId <= 0)
        {
            return Result<IEnumerable<ProjecaoCalculoDto>>.ValidationFailure(new[] { "ID da fatura inválido." });
        }

        var fatura = await _dbContext.Faturas
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == faturaId, ct);

        if (fatura == null)
        {
            return Result<IEnumerable<ProjecaoCalculoDto>>.Failure($"Fatura ID {faturaId} não encontrada ou inacessível no tenant atual.");
        }

        var intervalosDias = new[] { 0, 7, 15, 30, 60 };
        var projecoes = new List<ProjecaoCalculoDto>();
        var dataBase = fatura.DataVencimento > DateTime.UtcNow ? fatura.DataVencimento : DateTime.UtcNow;

        foreach (var dias in intervalosDias)
        {
            var dataProjecao = dataBase.AddDays(dias);
            var parametros = new ParametrosCalculoFinanceiro(
                valorOriginal: fatura.ValorOriginal > 0 ? fatura.ValorOriginal : 1m,
                dataVencimento: fatura.DataVencimento,
                dataCalculo: dataProjecao,
                valorDescontoPontualidade: fatura.ValorDesconto
            );

            var res = _calculadora.CalcularEncargos(parametros);
            projecoes.Add(new ProjecaoCalculoDto(
                DiasAtrasoAdicionais: dias,
                DataProjecao: dataProjecao,
                ValorOriginal: res.ValorOriginal,
                ValorMulta: res.ValorMulta,
                ValorJuros: res.ValorJuros,
                ValorDesconto: res.ValorDesconto,
                ValorTotalProjetado: res.ValorTotalCalculado
            ));
        }

        return Result<IEnumerable<ProjecaoCalculoDto>>.Success(projecoes);
    }
}
