using BuildingBlocks.Shared.MultiTenancy;
using BuildingBlocks.Shared;
using Microsoft.EntityFrameworkCore;
using Modules.Financial.Application.DTOs;
using Modules.Financial.Domain.Enums;
using Modules.Financial.Infrastructure.Persistence;

namespace Modules.Financial.Application.Services;

public class RelatorioConsolidadoApplicationService : IRelatorioConsolidadoApplicationService
{
    private readonly FinancialDbContext _dbContext;
    private readonly ICurrentTenantService _currentTenantService;

    public RelatorioConsolidadoApplicationService(
        FinancialDbContext dbContext,
        ICurrentTenantService currentTenantService)
    {
        _dbContext = dbContext;
        _currentTenantService = currentTenantService;
    }

    public async Task<Result<RelatorioConsolidadoMulticondominioDto>> ObterRelatorioConsolidadoAsync(CancellationToken ct = default)
    {
        int tenantId = _currentTenantService.TenantId ?? 1;

        var pastas = await _dbContext.PastasDigitais
            .Include(p => p.ItensBalancete)
            .ToListAsync(ct);

        var faturas = await _dbContext.Faturas.ToListAsync(ct);

        var condoIds = pastas.Select(p => p.CondoId)
            .Union(faturas.Select(f => f.CondoId))
            .DefaultIfEmpty(1)
            .Distinct()
            .ToList();

        var resumosCondominios = new List<ResumoCondominioDto>();

        foreach (var condoId in condoIds)
        {
            var pastasCondo = pastas.Where(p => p.CondoId == condoId).ToList();
            var faturasCondo = faturas.Where(f => f.CondoId == condoId).ToList();

            decimal receitas = pastasCondo.Sum(p => p.TotalReceitas);
            decimal despesas = pastasCondo.Sum(p => p.TotalDespesas);
            decimal saldo = receitas - despesas;

            int totalFaturas = faturasCondo.Count;
            int vencidas = faturasCondo.Count(f => f.Status == StatusFatura.Vencido);
            decimal taxaInadimplencia = totalFaturas > 0 ? ((decimal)vencidas / totalFaturas) * 100m : 0m;
            int pendentes = pastasCondo.Count(p => p.Status == StatusPastaDigital.EmAnaliseConselho || p.Status == StatusPastaDigital.Rascunho);

            resumosCondominios.Add(new ResumoCondominioDto(
                condoId,
                $"Condomínio #{condoId}",
                receitas,
                despesas,
                saldo,
                Math.Round(taxaInadimplencia, 2),
                vencidas,
                pendentes));
        }

        decimal receitaTotal = resumosCondominios.Sum(r => r.TotalReceitas);
        decimal despesaTotal = resumosCondominios.Sum(r => r.TotalDespesas);
        decimal saldoTotal = receitaTotal - despesaTotal;
        decimal taxaMedia = resumosCondominios.Any() ? resumosCondominios.Average(r => r.TaxaInadimplenciaPercentual) : 0m;
        int pendentesTotal = resumosCondominios.Sum(r => r.PastasPendentesAprovacao);

        var relatorio = new RelatorioConsolidadoMulticondominioDto(
            tenantId,
            DateTime.UtcNow,
            resumosCondominios.Count,
            receitaTotal,
            despesaTotal,
            saldoTotal,
            Math.Round(taxaMedia, 2),
            pendentesTotal,
            resumosCondominios);

        return Result<RelatorioConsolidadoMulticondominioDto>.Success(relatorio);
    }
}
