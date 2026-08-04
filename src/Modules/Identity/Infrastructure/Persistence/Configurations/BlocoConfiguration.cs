using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Identity.Domain;

namespace Modules.Identity.Infrastructure.Persistence.Configurations;

public sealed class BlocoConfiguration : IEntityTypeConfiguration<Bloco>
{
    public void Configure(EntityTypeBuilder<Bloco> builder)
    {
        builder.ToTable("blocos");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Codigo).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Nome).HasMaxLength(128).IsRequired();

        builder.HasIndex(x => new { x.TenantId, x.CondoId, x.Codigo }).IsUnique();

        builder.HasMany(x => x.Unidades)
            .WithOne(x => x.Bloco)
            .HasForeignKey(x => x.BlocoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
