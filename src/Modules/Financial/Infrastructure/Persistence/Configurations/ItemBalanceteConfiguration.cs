using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Financial.Domain.Entities;

namespace Modules.Financial.Infrastructure.Persistence.Configurations;

public class ItemBalanceteConfiguration : IEntityTypeConfiguration<ItemBalancete>
{
    public void Configure(EntityTypeBuilder<ItemBalancete> builder)
    {
        builder.ToTable("ItensBalancete", "financial");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Descricao)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(i => i.ValorOrcado)
            .HasPrecision(18, 2);

        builder.Property(i => i.ValorRealizado)
            .HasPrecision(18, 2);
    }
}
