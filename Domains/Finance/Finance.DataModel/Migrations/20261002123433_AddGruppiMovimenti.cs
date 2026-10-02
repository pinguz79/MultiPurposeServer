using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Finance.DataModel.Migrations
{
    /// <inheritdoc />
    public partial class AddGruppiMovimenti : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "GruppoMovimentiId",
                table: "Movimenti",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "GruppiMovimenti",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GruppiMovimenti", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Movimenti_GruppoMovimentiId",
                table: "Movimenti",
                column: "GruppoMovimentiId");

            migrationBuilder.AddForeignKey(
                name: "FK_Movimenti_GruppiMovimenti_GruppoMovimentiId",
                table: "Movimenti",
                column: "GruppoMovimentiId",
                principalTable: "GruppiMovimenti",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Movimenti_GruppiMovimenti_GruppoMovimentiId",
                table: "Movimenti");

            migrationBuilder.DropTable(
                name: "GruppiMovimenti");

            migrationBuilder.DropIndex(
                name: "IX_Movimenti_GruppoMovimentiId",
                table: "Movimenti");

            migrationBuilder.DropColumn(
                name: "GruppoMovimentiId",
                table: "Movimenti");
        }
    }
}
