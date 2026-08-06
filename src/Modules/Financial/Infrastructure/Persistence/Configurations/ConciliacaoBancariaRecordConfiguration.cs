using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Financial.Domain.Entities;

namespace Modules.Financial.Infrastructure.Persistence.Configurations;

public class ConciliacaoBancariaRecordConfiguration : IEntityTypeConfiguration<ConciliacaoBancariaRecord>
{
    public void Configure(EntityTypeBuilder<ConciliacaoBancariaRecord> builder)
    {
        builder.ToTable("ConciliacoesBancarias", "financial");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Observacoes)
            .HasMaxLength(500);
    }
}
