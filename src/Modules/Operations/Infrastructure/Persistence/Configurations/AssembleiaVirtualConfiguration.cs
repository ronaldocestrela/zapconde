using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Operations.Domain.Entities;

namespace Modules.Operations.Infrastructure.Persistence.Configurations;

public class AssembleiaVirtualConfiguration : IEntityTypeConfiguration<AssembleiaVirtual>
{
    public void Configure(EntityTypeBuilder<AssembleiaVirtual> builder)
    {
        builder.ToTable("AssembleiasVirtuais");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Titulo)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(a => a.Descricao)
            .HasMaxLength(2000);

        builder.Property(a => a.Tipo)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(a => a.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(a => a.DataInicio)
            .IsRequired();

        builder.Property(a => a.DataFim)
            .IsRequired();

        builder.Property(a => a.CriadoPorUserId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.AtaTexto)
            .HasMaxLength(8000);

        builder.HasMany(a => a.Pautas)
            .WithOne()
            .HasForeignKey(p => p.AssembleiaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(a => new { a.TenantId, a.CondoId, a.Status });
        builder.HasIndex(a => new { a.TenantId, a.DataInicio, a.DataFim });
    }
}
