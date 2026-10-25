using Dhole.Auth.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dhole.Auth.Persistence.Migrations;

[DbContext(typeof(ServiceDbContext))]
[Migration("20260918043000_AddMustChangePasswordToUsers")]
public partial class AddMustChangePasswordToUsers : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "must_change_password",
            schema: "auth",
            table: "Users",
            type: "boolean",
            nullable: false,
            defaultValue: false
        );
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "must_change_password",
            schema: "auth",
            table: "Users"
        );
    }
}
