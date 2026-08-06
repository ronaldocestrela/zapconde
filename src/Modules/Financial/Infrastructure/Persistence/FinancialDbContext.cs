using BuildingBlocks.Infrastructure.Persistence;
using BuildingBlocks.Shared.MultiTenancy;
using Microsoft.EntityFrameworkCore;
using Modules.Financial.Domain.Entities;

namespace Modules.Financial.Infrastructure.Persistence;

public class FinancialDbContext : MultiTenantDbContext
{
    public FinancialDbContext(
        DbContextOptions<FinancialDbContext> options,
        ICurrentTenantService currentTenantService)
        : base(options, currentTenantService)
    {
    }

    public DbSet<Fatura> Faturas => Set<Fatura>();
    public DbSet<Boleto> Boletos => Set<Boleto>();
    public DbSet<ItemCobranca> ItensCobranca => Set<ItemCobranca>();
    public DbSet<Acordo> Acordos => Set<Acordo>();
    public DbSet<ParcelaAcordo> ParcelasAcordo => Set<ParcelaAcordo>();
    public DbSet<AcordoFaturaVinculada> AcordoFaturasVinculadas => Set<AcordoFaturaVinculada>();
    public DbSet<EtapaReguaInadimplencia> EtapasReguaInadimplencia => Set<EtapaReguaInadimplencia>();
    public DbSet<HistoricoCobranca> HistoricosCobranca => Set<HistoricoCobranca>();
    public DbSet<PastaDigital> PastasDigitais => Set<PastaDigital>();
    public DbSet<DocumentoPrestacaoContas> DocumentosPrestacaoContas => Set<DocumentoPrestacaoContas>();
    public DbSet<ItemBalancete> ItensBalancete => Set<ItemBalancete>();
    public DbSet<ContaBancaria> ContasBancarias => Set<ContaBancaria>();
    public DbSet<ExtratoBancarioItem> ExtratoBancarioItens => Set<ExtratoBancarioItem>();
    public DbSet<ConciliacaoBancariaRecord> ConciliacoesBancarias => Set<ConciliacaoBancariaRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FinancialDbContext).Assembly);
    }
}
