using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Financial.Domain.Entities;

namespace Modules.Financial.Infrastructure.Persistence.Configurations;

public class ItemCobrancaConfiguration : IEntityTypeConfiguration<ItemCobranca>
{
    public void Configure(EntityTypeBuilder<ItemCobranca> builder)
    {
        builder.ToTable("ItensCobranca", "financial");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.TenantId)
            .IsRequired();

        builder.Property(i => i.FaturaId)
            .IsRequired();

        builder.Property(i => i.Descricao)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(i => i.Tipo)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(i => i.ValorUnitario)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(i => i.Quantidade)
            .IsRequired();

        builder.Ignore(i => i.Subtotal);

        builder.HasIndex(i => new { i.TenantId, i.FaturaId });
    }
}
