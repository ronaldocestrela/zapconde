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
    public DbSet<KnowledgeDocument> KnowledgeDocuments => Set<KnowledgeDocument>();
    public DbSet<KnowledgeChunk> KnowledgeChunks => Set<KnowledgeChunk>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("ai");
        modelBuilder.HasPostgresExtension("vector");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AiDbContext).Assembly);

        if (Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory")
        {
            modelBuilder.Entity<KnowledgeChunk>()
                .Property(c => c.Embedding)
                .HasConversion(
                    v => v == null ? null : string.Join(',', v.ToArray()),
                    s => string.IsNullOrEmpty(s) ? null : new Pgvector.Vector(s.Split(',', StringSplitOptions.None).Select(float.Parse).ToArray()));
        }
    }
}
