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
                    PathKey = table.Column<byte[]>(type: "binary(32)", fixedLength: true, maxLength: 32, nullable: false),
                    FilesystemName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Path = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                    Path_reversed = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true, computedColumnSql: "(reverse([Path]))", stored: false),
                    CreatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ETag = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Paths", x => x.PathKey)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateTable(
                name: "PathsMetadata",
                columns: table => new
                {
                    PathKey = table.Column<byte[]>(type: "binary(32)", fixedLength: true, maxLength: 32, nullable: false),
                    MetadataJson = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PathsMetadata", x => x.PathKey);
                });

            migrationBuilder.CreateIndex(
                name: "filesystem_path_reversed",
                table: "Paths",
                columns: new[] { "FilesystemName", "Path_reversed" });

            migrationBuilder.CreateIndex(
                name: "path_filesystem_unique",
                table: "Paths",
                columns: new[] { "FilesystemName", "Path" })
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "path_key",
                table: "Paths",
                column: "PathKey",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Paths");

            migrationBuilder.DropTable(
                name: "PathsMetadata");
        }
    }
}
