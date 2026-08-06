using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Financial.Domain.Entities;

namespace Modules.Financial.Infrastructure.Persistence.Configurations;

public class DocumentoPrestacaoContasConfiguration : IEntityTypeConfiguration<DocumentoPrestacaoContas>
{
    public void Configure(EntityTypeBuilder<DocumentoPrestacaoContas> builder)
    {
        builder.ToTable("DocumentosPrestacaoContas", "financial");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Titulo)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(d => d.NomeArquivo)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(d => d.UrlArquivo)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(d => d.ContentType)
            .HasMaxLength(100);
    }
}
