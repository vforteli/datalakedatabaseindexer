using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PGDatabaseContext.Migrations
{
    /// <inheritdoc />
    public partial class testmetadatainpaths2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "should_update_metadata",
                table: "paths",
                type: "boolean",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "should_update_metadata",
                table: "paths");
        }
    }
}
