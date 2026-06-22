using CustomCodeFramework.Postgres.EntityFramework.Configurations;
using Dhole.Auth.Domain.Roles.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dhole.Auth.Persistence.Configurations.Roles;

internal sealed class RoleScopeConfiguration : EntityTypeConfigurationBase<RoleScope, Guid>
{
    public override void Configure(EntityTypeBuilder<RoleScope> builder)
    {
        base.Configure(builder);

        builder.ToTable("RoleScopes");

        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.RoleId).IsRequired();
        builder.Property(x => x.ScopeId).IsRequired();
        builder.Property(x => x.AssignedAt).IsRequired();
        builder.Property(x => x.AssignedBy);

        builder.HasIndex(x => new { x.RoleId, x.ScopeId }).IsUnique();
    }
}
