using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Operations.Domain.Entities;

namespace Modules.Operations.Infrastructure.Persistence.Configurations;

public class PlanoManutencaoConfiguration : IEntityTypeConfiguration<PlanoManutencao>
{
    public void Configure(EntityTypeBuilder<PlanoManutencao> builder)
    {
        builder.ToTable("PlanosManutencao");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.TenantId)
            .IsRequired();

        builder.Property(p => p.CondoId)
            .IsRequired();

        builder.Property(p => p.Titulo)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(p => p.Descricao)
            .HasMaxLength(1000);

        builder.Property(p => p.Categoria)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(p => p.Periodicidade)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(p => p.DataProximaManutencao)
            .IsRequired();

        builder.Property(p => p.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(p => p.ResponsavelTecnico)
            .HasMaxLength(150);

        builder.Property(p => p.EmpresaContratada)
            .HasMaxLength(150);

        builder.Property(p => p.CustoEstimado)
            .HasColumnType("numeric(18,2)");

        builder.Property(p => p.CustoReal)
            .HasColumnType("numeric(18,2)");

        builder.Property(p => p.Observacoes)
            .HasMaxLength(2000);

        builder.Property(p => p.Ativo)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(p => p.DataCriacao)
            .IsRequired();

        builder.HasIndex(p => new { p.TenantId, p.CondoId, p.Status });
        builder.HasIndex(p => new { p.TenantId, p.DataProximaManutencao });
    }
}
