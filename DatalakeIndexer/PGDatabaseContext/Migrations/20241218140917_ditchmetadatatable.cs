using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PGDatabaseContext.Migrations
{
    /// <inheritdoc />
    public partial class ditchmetadatatable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "paths_metadata");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "paths_metadata",
                columns: table => new
                {
                    path_key = table.Column<byte[]>(type: "bytea", nullable: false),
                    etag = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    metadata_json = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("paths_metadata_pk", x => x.path_key);
                });
        }
    }
}
