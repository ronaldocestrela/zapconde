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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FinancialDbContext).Assembly);
    }
}
