using CustomCodeFramework.Postgres.EntityFramework.Configurations;
using Dhole.Auth.Domain.Users.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dhole.Auth.Persistence.Configurations.Users;

internal sealed class UserConfiguration : EntityTypeConfigurationBase<User, Guid>
{
    public override void Configure(EntityTypeBuilder<User> builder)
    {
        base.Configure(builder);

        builder.ToTable("Users");

        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.UserName).HasMaxLength(150).IsRequired();

        builder.HasIndex(x => x.UserName).IsUnique();

        builder.Property(x => x.Email).HasMaxLength(250).IsRequired();

        builder.HasIndex(x => x.Email).IsUnique();

        builder.Property(x => x.DisplayName).HasMaxLength(250).IsRequired();

        builder.Property(x => x.UserType).HasConversion<string>().HasMaxLength(50).IsRequired();

        builder.Property(x => x.PasswordHash).HasMaxLength(500).IsRequired();

        builder.Property(x => x.IsActive).IsRequired().HasDefaultValue(true);

        builder.Property(x => x.IsLocked).IsRequired().HasDefaultValue(false);

        builder.Property(x => x.LockedAt);

        builder.Property(x => x.LockedReason).HasMaxLength(500);

        builder.Property(x => x.FailedLoginAttempts).IsRequired().HasDefaultValue(0);

        builder.Property(x => x.LastLoginAt);
        builder.Property(x => x.LastFailedLoginAt);

        builder.Property(x => x.TokenVersion).IsRequired().HasDefaultValue(0);

        builder
            .HasMany(x => x.Roles)
            .WithOne()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasMany(x => x.Scopes)
            .WithOne()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Roles).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(x => x.Scopes).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
