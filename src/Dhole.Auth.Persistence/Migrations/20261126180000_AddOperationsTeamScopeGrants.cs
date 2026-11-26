using Dhole.Auth.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dhole.Auth.Persistence.Migrations;

[DbContext(typeof(ServiceDbContext))]
[Migration("20261126180000_AddOperationsTeamScopeGrants")]
public sealed class AddOperationsTeamScopeGrants : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            CREATE TABLE auth."OperationsTeamScopeGrants" (
                "UserId" uuid NOT NULL,
                "ScopeCode" varchar(150) NOT NULL,
                PRIMARY KEY ("UserId", "ScopeCode")
            );
            CREATE TABLE auth."OperationsTeamScopeCheckpoint" (
                "Id" integer PRIMARY KEY CHECK ("Id" = 1),
                "Revision" bigint NOT NULL
            );
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            DROP TABLE auth."OperationsTeamScopeGrants";
            DROP TABLE auth."OperationsTeamScopeCheckpoint";
            """);
    }
}
