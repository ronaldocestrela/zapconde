using BuildingBlocks.Infrastructure.Persistence;
using BuildingBlocks.Shared.MultiTenancy;
using Microsoft.EntityFrameworkCore;
using Modules.AIEngine.Domain.Entities;

namespace Modules.AIEngine.Infrastructure.Persistence;

public class AiDbContext : MultiTenantDbContext
{
    public AiDbContext(
        DbContextOptions<AiDbContext> options,
        ICurrentTenantService currentTenantService)
        : base(options, currentTenantService)
    {
    }

    public DbSet<AiKernelConfig> KernelConfigs => Set<AiKernelConfig>();
    public DbSet<AiExecutionLog> ExecutionLogs => Set<AiExecutionLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("ai");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AiDbContext).Assembly);
    }
}
