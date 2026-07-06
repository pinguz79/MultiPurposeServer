using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MultiPurposeServer.Migrations
{
    /// <inheritdoc />
    public partial class AlbumPath_Take4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Path",
                table: "Albums",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Path",
                table: "Albums");
        }
    }
}
