using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Operations.Domain.Entities;

namespace Modules.Operations.Infrastructure.Persistence.Configurations;

public class OcorrenciaConfiguration : IEntityTypeConfiguration<Ocorrencia>
{
    public void Configure(EntityTypeBuilder<Ocorrencia> builder)
    {
        builder.ToTable("Ocorrencias");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Id)
            .ValueGeneratedOnAdd();

        builder.Property(o => o.TenantId)
            .IsRequired();

        builder.Property(o => o.CondoId)
            .IsRequired();

        builder.Property(o => o.MoradorId)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(o => o.MoradorNome)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(o => o.Titulo)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(o => o.Descricao)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(o => o.Categoria)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(o => o.Prioridade)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(o => o.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(o => o.Localizacao)
            .HasMaxLength(300);

        builder.Property(o => o.DataAbertura)
            .IsRequired();

        builder.Property(o => o.ResponsavelId)
            .HasMaxLength(128);

        builder.Property(o => o.ResponsavelNome)
            .HasMaxLength(200);

        builder.Property(o => o.ObservacaoResolucao)
            .HasMaxLength(2000);

        builder.Property(o => o.OrigemTriagemIa)
            .HasMaxLength(50);

        builder.Property(o => o.ResumoTriagemIa)
            .HasMaxLength(1000);

        builder.Property(o => o.ConfiancaTriagemIa);

        builder.Property(o => o.AudioUrl)
            .HasMaxLength(1000);

        builder.Property(o => o.TranscricaoAudio)
            .HasMaxLength(4000);

        builder.Property(o => o.SetorResponsavelSugerido)
            .HasMaxLength(200);

        builder.HasMany(o => o.Anexos)
            .WithOne()
            .HasForeignKey(a => a.OcorrenciaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(o => o.Historico)
            .WithOne()
            .HasForeignKey(h => h.OcorrenciaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(o => new { o.TenantId, o.CondoId, o.Status });
        builder.HasIndex(o => new { o.TenantId, o.MoradorId });
    }
}
