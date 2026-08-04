using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Identity.Domain;

namespace Modules.Identity.Infrastructure.Persistence.Configurations;

public sealed class AdministradoraConfiguration : IEntityTypeConfiguration<Administradora>
{
    public void Configure(EntityTypeBuilder<Administradora> builder)
    {
        builder.ToTable("administradoras");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.RazaoSocial).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Cnpj).HasMaxLength(14).IsRequired();
        builder.Property(x => x.NomeFantasia).HasMaxLength(256).IsRequired();
        builder.Property(x => x.LicensePlan).HasConversion<string>().HasMaxLength(32);

        builder.HasIndex(x => x.Cnpj).IsUnique();

        builder.HasMany(x => x.Condominios)
            .WithOne(x => x.Administradora)
            .HasForeignKey(x => x.TenantId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
