using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Identity.Domain;

namespace Modules.Identity.Infrastructure.Persistence.Configurations;

public sealed class VinculoUnidadeConfiguration : IEntityTypeConfiguration<VinculoUnidade>
{
    public void Configure(EntityTypeBuilder<VinculoUnidade> builder)
    {
        builder.ToTable("vinculos_unidade");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Papel).HasConversion<string>().HasMaxLength(32);
        builder.Property(x => x.MotivoEncerramento).HasMaxLength(512);
        builder.Property(x => x.CreatedByUserId).HasMaxLength(64);

        builder.Property(x => x.Dependencias)
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<string>())
            .HasColumnType("jsonb")
            .Metadata.SetValueComparer(new Microsoft.EntityFrameworkCore.ChangeTracking.ValueComparer<List<string>>(
                (a, b) => (a == null && b == null) || (a != null && b != null && a.SequenceEqual(b)),
                v => v.Aggregate(0, (hash, s) => HashCode.Combine(hash, s.GetHashCode())),
                v => v.ToList()));

        builder.HasOne(x => x.Morador)
            .WithMany(x => x.Vinculos)
            .HasForeignKey(x => x.MoradorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.UnidadeId, x.Papel, x.IsActive });
    }
}
