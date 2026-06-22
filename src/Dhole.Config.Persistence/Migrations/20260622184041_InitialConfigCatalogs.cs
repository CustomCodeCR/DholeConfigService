using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dhole.Config.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialConfigCatalogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "config");

            migrationBuilder.CreateTable(
                name: "CatalogGroups",
                schema: "config",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    slug = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    metadata_json = table.Column<string>(type: "jsonb", nullable: true),
                    is_system = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_catalog_groups", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "inbox_messages",
                schema: "config",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_type = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    event_name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    source_service = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    consumer_service = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    correlation_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    processed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_inbox_messages", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "outbox_messages",
                schema: "config",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_type = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    event_name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    source_service = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    payload_json = table.Column<string>(type: "jsonb", nullable: false),
                    headers_json = table.Column<string>(type: "jsonb", nullable: true),
                    correlation_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    retry_count = table.Column<int>(type: "integer", nullable: false),
                    error_message = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    processed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_outbox_messages", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "CatalogItems",
                schema: "config",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    catalog_group_id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    slug = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    value = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    metadata_json = table.Column<string>(type: "jsonb", nullable: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    is_system = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_catalog_items", x => x.id);
                    table.ForeignKey(
                        name: "f_k_catalog_items_catalog_groups_catalog_group_id",
                        column: x => x.catalog_group_id,
                        principalSchema: "config",
                        principalTable: "CatalogGroups",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CatalogGroups_code",
                schema: "config",
                table: "CatalogGroups",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CatalogGroups_name",
                schema: "config",
                table: "CatalogGroups",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CatalogGroups_slug",
                schema: "config",
                table: "CatalogGroups",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CatalogItems_catalog_group_id_code",
                schema: "config",
                table: "CatalogItems",
                columns: new[] { "catalog_group_id", "code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CatalogItems_catalog_group_id_name",
                schema: "config",
                table: "CatalogItems",
                columns: new[] { "catalog_group_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CatalogItems_catalog_group_id_slug",
                schema: "config",
                table: "CatalogItems",
                columns: new[] { "catalog_group_id", "slug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CatalogItems_catalog_group_id_sort_order",
                schema: "config",
                table: "CatalogItems",
                columns: new[] { "catalog_group_id", "sort_order" });

            migrationBuilder.CreateIndex(
                name: "IX_inbox_messages_event_id_consumer_service",
                schema: "config",
                table: "inbox_messages",
                columns: new[] { "event_id", "consumer_service" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_inbox_messages_status_created_at",
                schema: "config",
                table: "inbox_messages",
                columns: new[] { "status", "created_at" });

            migrationBuilder.CreateIndex(
                name: "IX_outbox_messages_event_id",
                schema: "config",
                table: "outbox_messages",
                column: "event_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_outbox_messages_status_created_at",
                schema: "config",
                table: "outbox_messages",
                columns: new[] { "status", "created_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CatalogItems",
                schema: "config");

            migrationBuilder.DropTable(
                name: "inbox_messages",
                schema: "config");

            migrationBuilder.DropTable(
                name: "outbox_messages",
                schema: "config");

            migrationBuilder.DropTable(
                name: "CatalogGroups",
                schema: "config");
        }
    }
}
