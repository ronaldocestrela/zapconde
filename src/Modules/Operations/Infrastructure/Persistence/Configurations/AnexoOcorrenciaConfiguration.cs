using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Operations.Domain.Entities;

namespace Modules.Operations.Infrastructure.Persistence.Configurations;

public class AnexoOcorrenciaConfiguration : IEntityTypeConfiguration<AnexoOcorrencia>
{
    public void Configure(EntityTypeBuilder<AnexoOcorrencia> builder)
    {
        builder.ToTable("AnexosOcorrencia");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .ValueGeneratedOnAdd();

        builder.Property(a => a.TenantId)
            .IsRequired();

        builder.Property(a => a.CondoId)
            .IsRequired();

        builder.Property(a => a.OcorrenciaId)
            .IsRequired();

        builder.Property(a => a.Url)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(a => a.NomeArquivo)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(a => a.ContentType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.UploadPorUserId)
            .HasMaxLength(128);

        builder.HasIndex(a => new { a.TenantId, a.OcorrenciaId });
    }
}
