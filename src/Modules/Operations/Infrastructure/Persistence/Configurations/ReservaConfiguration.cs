using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Operations.Domain.Entities;
using Modules.Operations.Domain.Enums;

namespace Modules.Operations.Infrastructure.Persistence.Configurations;

public class ReservaConfiguration : IEntityTypeConfiguration<Reserva>
{
    public void Configure(EntityTypeBuilder<Reserva> builder)
    {
        builder.ToTable("Reservas", "operations");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TenantId)
            .IsRequired();

        builder.Property(x => x.CondoId)
            .IsRequired();

        builder.Property(x => x.AreaComumId)
            .IsRequired();

        builder.Property(x => x.MoradorId)
            .IsRequired();

        builder.Property(x => x.NomeMorador)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.UnidadeMorador)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.DataInicio)
            .IsRequired();

        builder.Property(x => x.DataFim)
            .IsRequired();

        builder.Property(x => x.QuantidadePessoas)
            .IsRequired();

        builder.Property(x => x.ValorTaxaReserva)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.ValorTaxaLimpeza)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.ValorTotal)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.Observacao)
            .HasMaxLength(500)
            .HasDefaultValue(string.Empty);

        builder.Property(x => x.MotivoCancelamento)
            .HasMaxLength(500)
            .HasDefaultValue(string.Empty);

        builder.Property(x => x.DataCriacao)
            .IsRequired();

        builder.HasOne(x => x.AreaComum)
            .WithMany()
            .HasForeignKey(x => x.AreaComumId)
            .OnDelete(DeleteBehavior.Restrict);

        // Índices para buscas rápidas de colisão e relatórios por tenant/morador
        builder.HasIndex(x => new { x.TenantId, x.AreaComumId, x.DataInicio, x.DataFim })
            .HasDatabaseName("IX_Reservas_Tenant_Area_Datas");

        builder.HasIndex(x => new { x.TenantId, x.MoradorId })
            .HasDatabaseName("IX_Reservas_Tenant_Morador");
    }
}
