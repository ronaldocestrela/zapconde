using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Financial.Domain.Entities;

namespace Modules.Financial.Infrastructure.Persistence.Configurations;

public class PastaDigitalConfiguration : IEntityTypeConfiguration<PastaDigital>
{
    public void Configure(EntityTypeBuilder<PastaDigital> builder)
    {
        builder.ToTable("PastasDigitais", "financial");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.ResumoExecutivoIa)
            .HasMaxLength(2000);

        builder.Property(p => p.ObservacoesConselho)
            .HasMaxLength(1000);

        builder.Property(p => p.SaldoAnterior)
            .HasPrecision(18, 2);

        builder.Property(p => p.TotalReceitas)
            .HasPrecision(18, 2);

        builder.Property(p => p.TotalDespesas)
            .HasPrecision(18, 2);

        builder.Property(p => p.SaldoMes)
            .HasPrecision(18, 2);

        builder.Property(p => p.SaldoAcumulado)
            .HasPrecision(18, 2);

        builder.HasMany(p => p.Documentos)
            .WithOne()
            .HasForeignKey(d => d.PastaDigitalId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.ItensBalancete)
            .WithOne()
            .HasForeignKey(i => i.PastaDigitalId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
