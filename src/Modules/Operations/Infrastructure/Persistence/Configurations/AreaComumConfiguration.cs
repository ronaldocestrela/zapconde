using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Operations.Domain.Entities;

namespace Modules.Operations.Infrastructure.Persistence.Configurations;

public class AreaComumConfiguration : IEntityTypeConfiguration<AreaComum>
{
    public void Configure(EntityTypeBuilder<AreaComum> builder)
    {
        builder.ToTable("AreasComuns", "operations");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TenantId)
            .IsRequired();

        builder.Property(x => x.CondoId)
            .IsRequired();

        builder.Property(x => x.Nome)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.Descricao)
            .HasMaxLength(500);

        builder.Property(x => x.Tipo)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.CapacidadeMaxima)
            .IsRequired();

        builder.Property(x => x.TaxaReserva)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.TaxaLimpeza)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.HorarioInicioFuncionamento)
            .IsRequired();

        builder.Property(x => x.HorarioFimFuncionamento)
            .IsRequired();

        builder.Property(x => x.TempoAntecedenciaMinimaDias)
            .IsRequired();

        builder.Property(x => x.TempoAntecedenciaMaximaDias)
            .IsRequired();

        builder.Property(x => x.RequerAprovacaoSindico)
            .IsRequired();

        builder.Property(x => x.RegrasUso)
            .HasMaxLength(2000);

        builder.Property(x => x.DataCriacao)
            .IsRequired();

        builder.HasIndex(x => new { x.TenantId, x.CondoId, x.Status });
        builder.HasIndex(x => new { x.TenantId, x.CondoId, x.Nome }).IsUnique();
    }
}
