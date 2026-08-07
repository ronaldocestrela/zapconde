using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.AIEngine.Domain.Entities;

namespace Modules.AIEngine.Infrastructure.Persistence.Configurations;

public class AiExecutionLogConfiguration : IEntityTypeConfiguration<AiExecutionLog>
{
    public void Configure(EntityTypeBuilder<AiExecutionLog> builder)
    {
        builder.ToTable("ExecutionLogs", "ai");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Prompt)
            .IsRequired();

        builder.Property(l => l.Response);

        builder.Property(l => l.ModelUsed)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(l => l.ErrorMessage)
            .HasMaxLength(2000);

        builder.HasIndex(l => new { l.TenantId, l.ExecutedAt })
            .HasDatabaseName("IX_ExecutionLogs_Tenant_ExecutedAt");

        builder.HasIndex(l => new { l.TenantId, l.Success })
            .HasDatabaseName("IX_ExecutionLogs_Tenant_Success");
    }
}
