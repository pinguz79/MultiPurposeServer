using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Portfolio.DataModel.Migrations
{
    /// <inheritdoc />
    public partial class AlbumDescription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Albums",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Albums");
        }
    }
}
