using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Operations.Domain.Entities;

namespace Modules.Operations.Infrastructure.Persistence.Configurations;

public class VotoAssembleiaConfiguration : IEntityTypeConfiguration<VotoAssembleia>
{
    public void Configure(EntityTypeBuilder<VotoAssembleia> builder)
    {
        builder.ToTable("VotosAssembleia");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.MoradorUserId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(v => v.UnidadeId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(v => v.OpcaoEscolhida)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(v => v.PesoVoto)
            .IsRequired()
            .HasDefaultValue(1.0);

        builder.Property(v => v.DataVoto)
            .IsRequired();

        // Invariante de Banco de Dados: Voto Único por Unidade Habitacional em cada Pauta
        builder.HasIndex(v => new { v.TenantId, v.PautaId, v.UnidadeId })
            .IsUnique();

        builder.HasIndex(v => v.AssembleiaId);
    }
}
