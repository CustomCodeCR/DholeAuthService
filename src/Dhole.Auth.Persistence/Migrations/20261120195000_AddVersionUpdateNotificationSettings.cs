using Dhole.Auth.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dhole.Auth.Persistence.Migrations;

[DbContext(typeof(ServiceDbContext))]
[Migration("20261120195000_AddVersionUpdateNotificationSettings")]
public partial class AddVersionUpdateNotificationSettings : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "VersionUpdateNotificationSettings",
            schema: "auth",
            columns: table => new
            {
                Key = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                Enabled = table.Column<bool>(type: "boolean", nullable: false),
                UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_VersionUpdateNotificationSettings", x => x.Key));
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "VersionUpdateNotificationSettings", schema: "auth");
    }
}
