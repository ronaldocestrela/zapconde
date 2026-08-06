using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Financial.Domain.Entities;

namespace Modules.Financial.Infrastructure.Persistence.Configurations;

public class AcordoFaturaVinculadaConfiguration : IEntityTypeConfiguration<AcordoFaturaVinculada>
{
    public void Configure(EntityTypeBuilder<AcordoFaturaVinculada> builder)
    {
        builder.ToTable("AcordoFaturasVinculadas", "financial");

        builder.HasKey(af => af.Id);

        builder.Property(af => af.ValorFaturaOriginal)
            .HasPrecision(18, 2);
    }
}
