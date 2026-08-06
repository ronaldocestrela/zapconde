using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Financial.Domain.Entities;

namespace Modules.Financial.Infrastructure.Persistence.Configurations;

public class ExtratoBancarioItemConfiguration : IEntityTypeConfiguration<ExtratoBancarioItem>
{
    public void Configure(EntityTypeBuilder<ExtratoBancarioItem> builder)
    {
        builder.ToTable("ExtratoBancarioItens", "financial");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.DescricaoHistorico)
            .HasMaxLength(300);

        builder.Property(e => e.DocumentoRef)
            .HasMaxLength(100);

        builder.Property(e => e.Valor)
            .HasPrecision(18, 2);

        builder.Property(e => e.ScoreConciliacao)
            .HasPrecision(5, 2);
    }
}
