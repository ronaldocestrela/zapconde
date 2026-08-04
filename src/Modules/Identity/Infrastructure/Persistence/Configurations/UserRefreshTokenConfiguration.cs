using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Identity.Domain;

namespace Modules.Identity.Infrastructure.Persistence.Configurations;

public sealed class UserRefreshTokenConfiguration : IEntityTypeConfiguration<UserRefreshToken>
{
    public void Configure(EntityTypeBuilder<UserRefreshToken> builder)
    {
        builder.ToTable("user_refresh_tokens");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Token).HasMaxLength(512).IsRequired();
        builder.HasIndex(x => x.Token).IsUnique();
    }
}
