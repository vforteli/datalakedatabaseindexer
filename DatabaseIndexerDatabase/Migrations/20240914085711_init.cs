using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DatabaseIndexerDatabase.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Paths",
                columns: table => new
                {
                    PathId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FilesystemName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Path = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                    Path_reversed = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true, computedColumnSql: "(reverse([Path]))", stored: false),
                    CreatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Paths", x => x.PathId);
                });

            migrationBuilder.CreateIndex(
                name: "filesystem_path_reversed",
                table: "Paths",
                columns: new[] { "FilesystemName", "Path_reversed" });

            migrationBuilder.CreateIndex(
                name: "path_filesystem_unique",
                table: "Paths",
                columns: new[] { "FilesystemName", "Path" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Paths");
        }
    }
}
