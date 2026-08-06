using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Operations.Domain.Entities;

namespace Modules.Operations.Infrastructure.Persistence.Configurations;

public class HistoricoOcorrenciaConfiguration : IEntityTypeConfiguration<HistoricoOcorrencia>
{
    public void Configure(EntityTypeBuilder<HistoricoOcorrencia> builder)
    {
        builder.ToTable("HistoricoOcorrencias");

        builder.HasKey(h => h.Id);

        builder.Property(h => h.Id)
            .ValueGeneratedOnAdd();

        builder.Property(h => h.TenantId)
            .IsRequired();

        builder.Property(h => h.CondoId)
            .IsRequired();

        builder.Property(h => h.OcorrenciaId)
            .IsRequired();

        builder.Property(h => h.StatusAnterior)
            .HasConversion<int>();

        builder.Property(h => h.StatusNovo)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(h => h.Comentario)
            .HasMaxLength(1000);

        builder.Property(h => h.AlteradoPorUserId)
            .HasMaxLength(128);

        builder.Property(h => h.AlteradoPorNome)
            .HasMaxLength(200);

        builder.HasIndex(h => new { h.TenantId, h.OcorrenciaId });
    }
}
