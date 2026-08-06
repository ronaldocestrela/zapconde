using BuildingBlocks.Infrastructure.Persistence;
using BuildingBlocks.Shared.MultiTenancy;
using Microsoft.EntityFrameworkCore;
using Modules.Operations.Domain.Entities;

namespace Modules.Operations.Infrastructure.Persistence;

public class OperationsDbContext : MultiTenantDbContext
{
    public OperationsDbContext(
        DbContextOptions<OperationsDbContext> options,
        ICurrentTenantService currentTenantService)
        : base(options, currentTenantService)
    {
    }

    public DbSet<AreaComum> AreasComuns => Set<AreaComum>();
    public DbSet<Reserva> Reservas => Set<Reserva>();
    public DbSet<Ocorrencia> Ocorrencias => Set<Ocorrencia>();
    public DbSet<AnexoOcorrencia> AnexosOcorrencia => Set<AnexoOcorrencia>();
    public DbSet<HistoricoOcorrencia> HistoricoOcorrencias => Set<HistoricoOcorrencia>();
    public DbSet<PlanoManutencao> PlanosManutencao => Set<PlanoManutencao>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("operations");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OperationsDbContext).Assembly);
    }
}
