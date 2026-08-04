using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Identity.Domain;

namespace Modules.Identity.Infrastructure.Persistence.Configurations;

public sealed class UserCondoMembershipConfiguration : IEntityTypeConfiguration<UserCondoMembership>
{
    public void Configure(EntityTypeBuilder<UserCondoMembership> builder)
    {
        builder.ToTable("user_condo_memberships");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Role)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(x => x.DisplayLabel)
            .HasMaxLength(256);

        builder.HasIndex(x => new { x.UserId, x.TenantId, x.CondoId, x.Role })
            .IsUnique();

        builder.HasOne(x => x.User)
            .WithMany(x => x.Memberships)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
