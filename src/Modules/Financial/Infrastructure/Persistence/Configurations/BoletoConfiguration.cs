using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Financial.Domain.Entities;

namespace Modules.Financial.Infrastructure.Persistence.Configurations;

public class BoletoConfiguration : IEntityTypeConfiguration<Boleto>
{
    public void Configure(EntityTypeBuilder<Boleto> builder)
    {
        builder.ToTable("Boletos", "financial");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.TenantId)
            .IsRequired();

        builder.Property(b => b.FaturaId)
            .IsRequired();

        builder.Property(b => b.NossoNumero)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(b => b.LinhaDigitavel)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(b => b.CodigoBarras)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(b => b.CodigoPixCopiaECola)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(b => b.QrCodeUrl)
            .HasMaxLength(300);

        builder.Property(b => b.PdfUrl)
            .HasMaxLength(300);

        builder.Property(b => b.PixQrCodeBase64)
            .HasColumnType("text");

        builder.Property(b => b.ExternalChargeId)
            .HasMaxLength(100);

        builder.Property(b => b.GatewayProvider)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(b => b.Valor)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(b => b.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.HasIndex(b => new { b.TenantId, b.FaturaId }).IsUnique();
        builder.HasIndex(b => new { b.TenantId, b.NossoNumero }).IsUnique();
        builder.HasIndex(b => new { b.TenantId, b.ExternalChargeId });
    }
}
