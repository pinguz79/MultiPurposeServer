using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MultiPurposeServer.Migrations
{
    /// <inheritdoc />
    public partial class AlbumPath_Take3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Path2",
                table: "Albums");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Path2",
                table: "Albums",
                type: "TEXT",
                nullable: true);
        }
    }
}
