using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Financial.Domain.Entities;

namespace Modules.Financial.Infrastructure.Persistence.Configurations;

public class ParcelaAcordoConfiguration : IEntityTypeConfiguration<ParcelaAcordo>
{
    public void Configure(EntityTypeBuilder<ParcelaAcordo> builder)
    {
        builder.ToTable("ParcelasAcordo", "financial");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.ValorParcela)
            .HasPrecision(18, 2);
    }
}
