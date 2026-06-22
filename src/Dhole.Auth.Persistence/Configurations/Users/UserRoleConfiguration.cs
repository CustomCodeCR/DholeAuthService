using CustomCodeFramework.Postgres.EntityFramework.Configurations;
using Dhole.Auth.Domain.Users.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dhole.Auth.Persistence.Configurations.Users;

internal sealed class UserRoleConfiguration : EntityTypeConfigurationBase<UserRole, Guid>
{
    public override void Configure(EntityTypeBuilder<UserRole> builder)
    {
        base.Configure(builder);

        builder.ToTable("UserRoles");

        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.RoleId).IsRequired();
        builder.Property(x => x.AssignedAt).IsRequired();
        builder.Property(x => x.AssignedBy);

        builder.HasIndex(x => new { x.UserId, x.RoleId }).IsUnique();
    }
}
