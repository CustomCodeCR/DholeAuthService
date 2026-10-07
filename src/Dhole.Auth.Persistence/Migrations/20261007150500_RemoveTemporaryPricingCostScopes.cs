using Dhole.Auth.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dhole.Auth.Persistence.Migrations;

[DbContext(typeof(ServiceDbContext))]
[Migration("20261007150500_RemoveTemporaryPricingCostScopes")]
public partial class RemoveTemporaryPricingCostScopes : Migration
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
              AND s.code IN ('pricing.cost.view', 'pricing.cost.select');
            """
        );
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Intentionally empty. These two scopes were temporary auto-grants.
    }
}
