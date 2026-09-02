using Dhole.Auth.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dhole.Auth.Persistence.Migrations;

[DbContext(typeof(ServiceDbContext))]
[Migration("20260902134500_ResetPricingRoleAutoGrantedScopes")]
public partial class ResetPricingRoleAutoGrantedScopes : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            DELETE FROM auth."RoleScopes" AS rs
            USING auth."Roles" AS r, auth."Scopes" AS s
            WHERE rs.role_id = r.id
              AND rs.scope_id = s.id
              AND LOWER(r.name) = 'pricing'
              AND s.code LIKE 'pricing.%'
              AND s.code <> 'pricing.workspace.access';
            """
        );
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Intentionally empty. Extra Pricing permissions must be explicitly re-assigned by scope.
    }
}
