using BuildingBlocks.Shared.MultiTenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Modules.AIEngine.Infrastructure.Persistence;

public class AiDbContextFactory : IDesignTimeDbContextFactory<AiDbContext>
{
    public AiDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AiDbContext>();
        var connectionString = "Host=localhost;Port=5432;Database=smartcondo_dev;Username=postgres;Password=postgres";

        optionsBuilder.UseNpgsql(
            connectionString,
            npgsqlOptions => npgsqlOptions.UseVector());

        return new AiDbContext(optionsBuilder.Options, new DesignTimeCurrentTenantService());
    }

    private class DesignTimeCurrentTenantService : ICurrentTenantService
    {
        public int? TenantId { get; private set; } = 1;
        public int? CondoId { get; private set; } = 1;
        public void SetTenantId(int tenantId) => TenantId = tenantId;
        public void SetCondoId(int condoId) => CondoId = condoId;
        public void Clear() { TenantId = null; CondoId = null; }
    }
}
