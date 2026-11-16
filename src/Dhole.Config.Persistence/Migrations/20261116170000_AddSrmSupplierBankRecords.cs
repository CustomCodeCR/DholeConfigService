using Dhole.Config.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Dhole.Config.Persistence.Migrations;

[DbContext(typeof(ServiceDbContext))]
[Migration("20261116170000_AddSrmSupplierBankRecords")]
public sealed class AddSrmSupplierBankRecords : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "srm_supplier_bank_records",
            schema: "config",
            columns: table => new
            {
                supplier_id = table.Column<Guid>(type: "uuid", nullable: false),
                ciphertext = table.Column<byte[]>(type: "bytea", nullable: false),
                nonce = table.Column<byte[]>(type: "bytea", nullable: false),
                tag = table.Column<byte[]>(type: "bytea", nullable: false),
                updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table => table.PrimaryKey("pk_srm_supplier_bank_records", x => x.supplier_id));
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "srm_supplier_bank_records", schema: "config");
    }
}
