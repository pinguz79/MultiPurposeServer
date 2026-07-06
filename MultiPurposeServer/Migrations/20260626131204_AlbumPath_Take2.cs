using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MultiPurposeServer.Migrations
{
    /// <inheritdoc />
    public partial class AlbumPath_Take2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Path",
                table: "Albums",
                newName: "Path2");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Path2",
                table: "Albums",
                newName: "Path");
        }
    }
}
