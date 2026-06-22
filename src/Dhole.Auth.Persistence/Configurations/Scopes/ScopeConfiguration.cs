using CustomCodeFramework.Postgres.EntityFramework.Configurations;
using Dhole.Auth.Domain.Scopes.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dhole.Auth.Persistence.Configurations.Scopes;

internal sealed class ScopeConfiguration : EntityTypeConfigurationBase<Scope, Guid>
{
    public override void Configure(EntityTypeBuilder<Scope> builder)
    {
        base.Configure(builder);

        builder.ToTable("Scopes");

        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.Code).HasMaxLength(200).IsRequired();

        builder.HasIndex(x => x.Code).IsUnique();

        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();

        builder.Property(x => x.Description).HasMaxLength(500);

        builder.Property(x => x.IsActive).IsRequired().HasDefaultValue(true);
    }
}
