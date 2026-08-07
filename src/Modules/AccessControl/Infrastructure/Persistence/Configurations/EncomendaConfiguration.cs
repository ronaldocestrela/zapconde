using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.AccessControl.Domain.Entities;

namespace Modules.AccessControl.Infrastructure.Persistence.Configurations;

public class EncomendaConfiguration : IEntityTypeConfiguration<Encomenda>
{
    public void Configure(EntityTypeBuilder<Encomenda> builder)
    {
        builder.ToTable("Encomendas", "access_control");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.BlocoUnidade)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.CodigoRastreio)
            .HasMaxLength(100);

        builder.Property(e => e.Descricao)
            .HasMaxLength(300);

        builder.Property(e => e.Remetente)
            .HasMaxLength(150);

        builder.Property(e => e.Transportadora)
            .HasMaxLength(150);

        builder.Property(e => e.LocalArmazenamento)
            .HasMaxLength(100);

        builder.Property(e => e.Tipo)
            .IsRequired();

        builder.Property(e => e.Status)
            .IsRequired();

        builder.Property(e => e.RecebidoPorNome)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(e => e.RetiradoPorNome)
            .HasMaxLength(150);

        builder.Property(e => e.Observacoes)
            .HasMaxLength(1000);

        builder.HasIndex(e => new { e.TenantId, e.CondoId, e.Status })
            .HasDatabaseName("IX_Encomendas_Tenant_Condo_Status");

        builder.HasIndex(e => new { e.TenantId, e.UnidadeId })
            .HasDatabaseName("IX_Encomendas_Tenant_Unidade");

        builder.HasIndex(e => new { e.TenantId, e.CodigoRastreio })
            .HasDatabaseName("IX_Encomendas_Tenant_CodigoRastreio");
    }
}
