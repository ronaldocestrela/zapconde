using BuildingBlocks.Infrastructure.Persistence;
using BuildingBlocks.Shared.MultiTenancy;
using Microsoft.EntityFrameworkCore;
using Modules.AccessControl.Domain.Entities;

namespace Modules.AccessControl.Infrastructure.Persistence;

public class AccessControlDbContext : MultiTenantDbContext
{
    public AccessControlDbContext(
        DbContextOptions<AccessControlDbContext> options,
        ICurrentTenantService currentTenantService)
        : base(options, currentTenantService)
    {
    }

    public DbSet<Visitante> Visitantes => Set<Visitante>();
    public DbSet<Encomenda> Encomendas => Set<Encomenda>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("access_control");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AccessControlDbContext).Assembly);
    }
}
