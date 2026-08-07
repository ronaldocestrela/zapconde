using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.AIEngine.Domain.Entities;

namespace Modules.AIEngine.Infrastructure.Persistence.Configurations;

public class KnowledgeDocumentConfiguration : IEntityTypeConfiguration<KnowledgeDocument>
{
    public void Configure(EntityTypeBuilder<KnowledgeDocument> builder)
    {
        builder.ToTable("KnowledgeDocuments", "ai");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Title)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(d => d.OriginalFileName)
            .HasMaxLength(250);

        builder.Property(d => d.DocumentType)
            .IsRequired();

        builder.Property(d => d.Content)
            .IsRequired();

        builder.Property(d => d.ChunkCount)
            .IsRequired();

        builder.Property(d => d.IsActive)
            .IsRequired();

        builder.HasMany(d => d.Chunks)
            .WithOne(c => c.Document)
            .HasForeignKey(c => c.DocumentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(d => new { d.TenantId, d.IsActive })
            .HasDatabaseName("IX_KnowledgeDocuments_Tenant_IsActive");

        builder.HasIndex(d => new { d.TenantId, d.DocumentType })
            .HasDatabaseName("IX_KnowledgeDocuments_Tenant_Type");
    }
}
