using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Operations.Domain.Entities;

namespace Modules.Operations.Infrastructure.Persistence.Configurations;

public class PautaAssembleiaConfiguration : IEntityTypeConfiguration<PautaAssembleia>
{
    public void Configure(EntityTypeBuilder<PautaAssembleia> builder)
    {
        builder.ToTable("PautasAssembleia");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Titulo)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Descricao)
            .HasMaxLength(1000);

        builder.Property(p => p.TipoVotacao)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(p => p.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(p => p.OpcoesDisponiveis)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>())
            .HasColumnType("jsonb");

        builder.HasMany(p => p.Votos)
            .WithOne()
            .HasForeignKey(v => v.PautaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(p => p.AssembleiaId);
    }
}
