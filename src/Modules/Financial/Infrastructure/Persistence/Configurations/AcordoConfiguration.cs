using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Financial.Domain.Entities;

namespace Modules.Financial.Infrastructure.Persistence.Configurations;

public class AcordoConfiguration : IEntityTypeConfiguration<Acordo>
{
    public void Configure(EntityTypeBuilder<Acordo> builder)
    {
        builder.ToTable("Acordos", "financial");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.NumeroAcordo)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(a => a.ValorTotalOriginal)
            .HasPrecision(18, 2);

        builder.Property(a => a.ValorDesconto)
            .HasPrecision(18, 2);

        builder.Property(a => a.ValorTotalAcordo)
            .HasPrecision(18, 2);

        builder.Property(a => a.Observacoes)
            .HasMaxLength(500);

        builder.HasMany(a => a.Parcelas)
            .WithOne()
            .HasForeignKey(p => p.AcordoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(a => a.FaturasVinculadas)
            .WithOne()
            .HasForeignKey(f => f.AcordoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
