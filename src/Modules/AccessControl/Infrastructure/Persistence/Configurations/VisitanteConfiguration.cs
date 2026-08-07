using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.AccessControl.Domain.Entities;

namespace Modules.AccessControl.Infrastructure.Persistence.Configurations;

public class VisitanteConfiguration : IEntityTypeConfiguration<Visitante>
{
    public void Configure(EntityTypeBuilder<Visitante> builder)
    {
        builder.ToTable("Visitantes", "access_control");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.NomeCompleto)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(v => v.Documento)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(v => v.Telefone)
            .HasMaxLength(30);

        builder.Property(v => v.Tipo)
            .IsRequired();

        builder.Property(v => v.Status)
            .IsRequired();

        builder.Property(v => v.Empresa)
            .HasMaxLength(150);

        builder.Property(v => v.PlacaVeiculo)
            .HasMaxLength(20);

        builder.Property(v => v.BlocoUnidade)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(v => v.Observacoes)
            .HasMaxLength(1000);

        builder.HasIndex(v => new { v.TenantId, v.CondoId, v.Status })
            .HasDatabaseName("IX_Visitantes_Tenant_Condo_Status");

        builder.HasIndex(v => new { v.TenantId, v.UnidadeId })
            .HasDatabaseName("IX_Visitantes_Tenant_Unidade");

        builder.HasIndex(v => new { v.TenantId, v.Documento })
            .HasDatabaseName("IX_Visitantes_Tenant_Documento");
    }
}
