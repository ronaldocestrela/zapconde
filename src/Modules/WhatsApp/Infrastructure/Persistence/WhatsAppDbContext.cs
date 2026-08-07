using BuildingBlocks.Infrastructure.Persistence;
using BuildingBlocks.Shared.MultiTenancy;
using Microsoft.EntityFrameworkCore;
using Modules.WhatsApp.Domain.Entities;

namespace Modules.WhatsApp.Infrastructure.Persistence;

public class WhatsAppDbContext : MultiTenantDbContext
{
    public WhatsAppDbContext(
        DbContextOptions<WhatsAppDbContext> options,
        ICurrentTenantService currentTenantService)
        : base(options, currentTenantService)
    {
    }

    public DbSet<WhatsAppWebhookLog> WebhookLogs => Set<WhatsAppWebhookLog>();
    public DbSet<WhatsAppInstanceConfig> InstanceConfigs => Set<WhatsAppInstanceConfig>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("whatsapp");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(WhatsAppDbContext).Assembly);
    }
}
