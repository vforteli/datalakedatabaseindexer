using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PGDatabaseContext.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "paths",
                columns: table => new
                {
                    path_key = table.Column<byte[]>(type: "bytea", nullable: false),
                    filesystem_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    path = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    path_reversed = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true, computedColumnSql: "reverse((path)::text)", stored: true),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    etag = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("paths_pk", x => x.path_key);
                })
                .Annotation("Npgsql:StorageParameter:autovacuum_enabled", "true");

            migrationBuilder.CreateTable(
                name: "paths_metadata",
                columns: table => new
                {
                    path_key = table.Column<byte[]>(type: "bytea", nullable: false),
                    metadata_json = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("paths_metadata_pk", x => x.path_key);
                });

            migrationBuilder.CreateIndex(
                name: "filesystem_path_unique",
                table: "paths",
                columns: new[] { "filesystem_name", "path" })
                .Annotation("Npgsql:StorageParameter:deduplicate_items", "true");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "paths");

            migrationBuilder.DropTable(
                name: "paths_metadata");
        }
    }
}
