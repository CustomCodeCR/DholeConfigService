using Dhole.Config.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Dhole.Config.Persistence.Migrations;

[DbContext(typeof(ServiceDbContext))]
[Migration("20261118180000_AddSrmSupplierRoutes")]
public sealed class AddSrmSupplierRoutes : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "srm_supplier_routes", schema: "config",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                supplier_id = table.Column<Guid>(type: "uuid", nullable: false),
                transport_mode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                origin_ids = table.Column<Guid[]>(type: "uuid[]", nullable: false),
                destination_ids = table.Column<Guid[]>(type: "uuid[]", nullable: false),
                created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                created_by_id = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                updated_by_id = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
            }, constraints: table => table.PrimaryKey("pk_srm_supplier_routes", x => x.id));
        migrationBuilder.CreateIndex(
            name: "ix_srm_supplier_routes_supplier_mode",
            schema: "config", table: "srm_supplier_routes", columns: new[] { "supplier_id", "transport_mode" });
        migrationBuilder.Sql("CREATE INDEX ix_srm_supplier_routes_origin_gin ON config.srm_supplier_routes USING gin (origin_ids)");
        migrationBuilder.Sql("CREATE INDEX ix_srm_supplier_routes_destination_gin ON config.srm_supplier_routes USING gin (destination_ids)");

        migrationBuilder.CreateTable(
            name: "srm_supplier_route_audit", schema: "config",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                supplier_id = table.Column<Guid>(type: "uuid", nullable: false),
                route_id = table.Column<Guid>(type: "uuid", nullable: false),
                action = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                before_json = table.Column<string>(type: "text", nullable: true),
                after_json = table.Column<string>(type: "text", nullable: true),
                created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                actor_id = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
            }, constraints: table => table.PrimaryKey("pk_srm_supplier_route_audit", x => x.id));
        migrationBuilder.CreateIndex(
            name: "ix_srm_supplier_route_audit_supplier_created",
            schema: "config", table: "srm_supplier_route_audit", columns: new[] { "supplier_id", "created_at_utc" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "srm_supplier_route_audit", schema: "config");
        migrationBuilder.DropTable(name: "srm_supplier_routes", schema: "config");
    }
}
