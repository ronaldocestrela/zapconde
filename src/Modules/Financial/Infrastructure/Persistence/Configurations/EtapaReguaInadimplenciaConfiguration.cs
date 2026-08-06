using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Financial.Domain.Entities;

namespace Modules.Financial.Infrastructure.Persistence.Configurations;

public class EtapaReguaInadimplenciaConfiguration : IEntityTypeConfiguration<EtapaReguaInadimplencia>
{
    public void Configure(EntityTypeBuilder<EtapaReguaInadimplencia> builder)
    {
        builder.ToTable("EtapasReguaInadimplencia", "financial");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.NomeEtapa)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.TemplateMensagem)
            .HasMaxLength(1000);
    }
}
