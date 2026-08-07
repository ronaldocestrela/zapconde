using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.WhatsApp.Domain.Entities;

namespace Modules.WhatsApp.Infrastructure.Persistence.Configurations;

public class WhatsAppInstanceConfigConfiguration : IEntityTypeConfiguration<WhatsAppInstanceConfig>
{
    public void Configure(EntityTypeBuilder<WhatsAppInstanceConfig> builder)
    {
        builder.ToTable("InstanceConfigs", "whatsapp");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.InstanceName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(i => i.Provider)
            .IsRequired();

        builder.Property(i => i.BaseUrl)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(i => i.ApiKey)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(i => i.WebhookSecret)
            .HasMaxLength(250);

        builder.Property(i => i.Status)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(i => new { i.TenantId, i.InstanceName })
            .IsUnique()
            .HasDatabaseName("IX_InstanceConfigs_Tenant_InstanceName");
    }
}
