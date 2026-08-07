using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.AIEngine.Domain.Entities;

namespace Modules.AIEngine.Infrastructure.Persistence.Configurations;

public class KnowledgeChunkConfiguration : IEntityTypeConfiguration<KnowledgeChunk>
{
    public void Configure(EntityTypeBuilder<KnowledgeChunk> builder)
    {
        builder.ToTable("KnowledgeChunks", "ai");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.DocumentId)
            .IsRequired();

        builder.Property(c => c.ChunkIndex)
            .IsRequired();

        builder.Property(c => c.Content)
            .IsRequired();

        // Mapeamento do tipo vetorial pgvector com 1536 dimensões (OpenAI text-embedding-3-small)
        builder.Property(c => c.Embedding)
            .HasColumnType("vector(1536)");

        builder.HasIndex(c => new { c.TenantId, c.DocumentId })
            .HasDatabaseName("IX_KnowledgeChunks_Tenant_DocumentId");
    }
}
