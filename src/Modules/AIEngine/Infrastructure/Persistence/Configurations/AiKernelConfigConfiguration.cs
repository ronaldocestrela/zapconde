using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.AIEngine.Domain.Entities;

namespace Modules.AIEngine.Infrastructure.Persistence.Configurations;

public class AiKernelConfigConfiguration : IEntityTypeConfiguration<AiKernelConfig>
{
    public void Configure(EntityTypeBuilder<AiKernelConfig> builder)
    {
        builder.ToTable("KernelConfigs", "ai");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Provider)
            .IsRequired();

        builder.Property(c => c.ModelId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.EmbeddingModelId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.ApiKey)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(c => c.Endpoint)
            .HasMaxLength(500);

        builder.Property(c => c.OrgId)
            .HasMaxLength(200);

        builder.Property(c => c.Temperature)
            .IsRequired();

        builder.Property(c => c.MaxTokens)
            .IsRequired();

        builder.HasIndex(c => new { c.TenantId, c.IsActive })
            .HasDatabaseName("IX_KernelConfigs_Tenant_IsActive");
    }
}
