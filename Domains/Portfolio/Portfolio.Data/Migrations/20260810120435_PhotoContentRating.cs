using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Portfolio.Data.Migrations
{
    /// <inheritdoc />
    public partial class PhotoContentRating : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ContentRating",
                table: "Foto",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContentRating",
                table: "Foto");
        }
    }
}
