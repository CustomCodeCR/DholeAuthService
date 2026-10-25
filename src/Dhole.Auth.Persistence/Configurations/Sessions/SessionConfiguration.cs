using CustomCodeFramework.Postgres.EntityFramework.Configurations;
using Dhole.Auth.Domain.Sessions.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dhole.Auth.Persistence.Configurations.Sessions;

internal sealed class SessionConfiguration : EntityTypeConfigurationBase<Session, Guid>
{
    public override void Configure(EntityTypeBuilder<Session> builder)
    {
        base.Configure(builder);

        builder.ToTable("Sessions");

        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.UserId).IsRequired();

        builder.Property(x => x.RefreshTokenHash).HasMaxLength(500).IsRequired();

        builder.HasIndex(x => x.RefreshTokenHash).IsUnique();

        builder.Property(x => x.IpAddress).HasMaxLength(100);

        builder.Property(x => x.UserAgent).HasMaxLength(1000);

        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.LastUsedAt).IsRequired();
        builder.Property(x => x.ExpiresAt).IsRequired();

        builder.Property(x => x.IsRevoked).IsRequired().HasDefaultValue(false);

        builder.Property(x => x.RevokedAt);
        builder.Property(x => x.RevokedBy);

        builder.Property(x => x.RevocationReason).HasMaxLength(500);

        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => new { x.UserId, x.IsRevoked });
    }
}
