using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.WhatsApp.Domain.Entities;

namespace Modules.WhatsApp.Infrastructure.Persistence.Configurations;

public class WhatsAppWebhookLogConfiguration : IEntityTypeConfiguration<WhatsAppWebhookLog>
{
    public void Configure(EntityTypeBuilder<WhatsAppWebhookLog> builder)
    {
        builder.ToTable("WebhookLogs", "whatsapp");

        builder.HasKey(w => w.Id);

        builder.Property(w => w.InstanceName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(w => w.Provider)
            .IsRequired();

        builder.Property(w => w.MessageId)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(w => w.SenderPhone)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(w => w.PushName)
            .HasMaxLength(150);

        builder.Property(w => w.MessageType)
            .IsRequired();

        builder.Property(w => w.MessageText)
            .HasMaxLength(4000);

        builder.Property(w => w.MediaUrl)
            .HasMaxLength(1000);

        builder.Property(w => w.RawPayloadJson)
            .IsRequired()
            .HasColumnType("text");

        builder.Property(w => w.Status)
            .IsRequired();

        builder.Property(w => w.ErrorMessage)
            .HasMaxLength(1000);

        builder.Property(w => w.MoradorId)
            .IsRequired(false);

        builder.HasIndex(w => new { w.TenantId, w.InstanceName })
            .HasDatabaseName("IX_WebhookLogs_Tenant_Instance");

        builder.HasIndex(w => new { w.TenantId, w.MessageId })
            .HasDatabaseName("IX_WebhookLogs_Tenant_MessageId");

        builder.HasIndex(w => new { w.TenantId, w.Status })
            .HasDatabaseName("IX_WebhookLogs_Tenant_Status");

        builder.HasIndex(w => new { w.TenantId, w.SenderPhone })
            .HasDatabaseName("IX_WebhookLogs_Tenant_SenderPhone");

        builder.HasIndex(w => new { w.TenantId, w.MoradorId })
            .HasDatabaseName("IX_WebhookLogs_Tenant_MoradorId");
    }
}
