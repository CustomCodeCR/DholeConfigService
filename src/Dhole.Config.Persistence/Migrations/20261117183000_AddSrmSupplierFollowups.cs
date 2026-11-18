using Dhole.Config.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Dhole.Config.Persistence.Migrations;

[DbContext(typeof(ServiceDbContext))]
[Migration("20261117183000_AddSrmSupplierFollowups")]
public sealed class AddSrmSupplierFollowups : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "srm_supplier_followups",
            schema: "config",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                supplier_id = table.Column<Guid>(type: "uuid", nullable: false),
                kind = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                details = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                next_action_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                created_by_id = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                created_by_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
            },
            constraints: table => table.PrimaryKey("pk_srm_supplier_followups", x => x.id));
        migrationBuilder.CreateIndex(
            name: "ix_srm_supplier_followups_supplier_created",
            schema: "config",
            table: "srm_supplier_followups",
            columns: new[] { "supplier_id", "created_at_utc" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "srm_supplier_followups", schema: "config");
    }
}
