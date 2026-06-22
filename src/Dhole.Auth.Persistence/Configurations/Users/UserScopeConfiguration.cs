using CustomCodeFramework.Postgres.EntityFramework.Configurations;
using Dhole.Auth.Domain.Users.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dhole.Auth.Persistence.Configurations.Users;

internal sealed class UserScopeConfiguration : EntityTypeConfigurationBase<UserScope, Guid>
{
    public override void Configure(EntityTypeBuilder<UserScope> builder)
    {
        base.Configure(builder);

        builder.ToTable("UserScopes");

        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.ScopeId).IsRequired();
        builder.Property(x => x.AssignedAt).IsRequired();
        builder.Property(x => x.AssignedBy);

        builder.HasIndex(x => new { x.UserId, x.ScopeId }).IsUnique();
    }
}
