using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Financial.Domain.Entities;

namespace Modules.Financial.Infrastructure.Persistence.Configurations;

public class FaturaConfiguration : IEntityTypeConfiguration<Fatura>
{
    public void Configure(EntityTypeBuilder<Fatura> builder)
    {
        builder.ToTable("Faturas", "financial");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.TenantId)
            .IsRequired();

        builder.Property(f => f.CondoId)
            .IsRequired();

        builder.Property(f => f.UnidadeId)
            .IsRequired();

        builder.Property(f => f.MoradorId)
            .IsRequired();

        builder.Property(f => f.Competencia)
            .IsRequired()
            .HasMaxLength(7); // "YYYY-MM"

        builder.Property(f => f.NumeroFatura)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(f => f.ValorOriginal)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(f => f.ValorDesconto)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(f => f.ValorMulta)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(f => f.ValorJuros)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(f => f.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(f => f.Observacoes)
            .HasMaxLength(500);

        builder.HasIndex(f => new { f.TenantId, f.CondoId, f.Competencia });
        builder.HasIndex(f => new { f.TenantId, f.UnidadeId });
        builder.HasIndex(f => new { f.TenantId, f.NumeroFatura }).IsUnique();

        builder.HasMany(f => f.Itens)
            .WithOne(i => i.Fatura)
            .HasForeignKey(i => i.FaturaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(f => f.Boleto)
            .WithOne(b => b.Fatura)
            .HasForeignKey<Boleto>(b => b.FaturaId)
            .OnDelete(DeleteBehavior.Cascade);

        // Mapear backing field private _itens
        builder.Navigation(f => f.Itens).Metadata.SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
