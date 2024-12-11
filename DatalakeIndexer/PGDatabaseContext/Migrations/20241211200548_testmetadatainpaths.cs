using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PGDatabaseContext.Migrations
{
    /// <inheritdoc />
    public partial class testmetadatainpaths : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "metadata_json",
                table: "paths",
                type: "jsonb",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "metadata_json",
                table: "paths");
        }
    }
}
