using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Finance.DataModel.Migrations
{
    /// <inheritdoc />
    public partial class AddCategorie : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CategoriaId",
                table: "Pianificazioni",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ModalitaCategoria",
                table: "Pianificazioni",
                type: "INTEGER",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<string>(
                name: "VoceRicorrenteCategoriaName",
                table: "Pianificazioni",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CategoriaId",
                table: "Movimenti",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Categorie",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false, collation: "NOCASE"),
                    DisplayName = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categorie", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VociRicorrenti_CategoriaId",
                table: "VociRicorrenti",
                column: "CategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_Pianificazioni_CategoriaId",
                table: "Pianificazioni",
                column: "CategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_Movimenti_CategoriaId",
                table: "Movimenti",
                column: "CategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_Categorie_Name",
                table: "Categorie",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Movimenti_Categorie_CategoriaId",
                table: "Movimenti",
                column: "CategoriaId",
                principalTable: "Categorie",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Pianificazioni_Categorie_CategoriaId",
                table: "Pianificazioni",
                column: "CategoriaId",
                principalTable: "Categorie",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_VociRicorrenti_Categorie_CategoriaId",
                table: "VociRicorrenti",
                column: "CategoriaId",
                principalTable: "Categorie",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Movimenti_Categorie_CategoriaId",
                table: "Movimenti");

            migrationBuilder.DropForeignKey(
                name: "FK_Pianificazioni_Categorie_CategoriaId",
                table: "Pianificazioni");

            migrationBuilder.DropForeignKey(
                name: "FK_VociRicorrenti_Categorie_CategoriaId",
                table: "VociRicorrenti");

            migrationBuilder.DropTable(
                name: "Categorie");

            migrationBuilder.DropIndex(
                name: "IX_VociRicorrenti_CategoriaId",
                table: "VociRicorrenti");

            migrationBuilder.DropIndex(
                name: "IX_Pianificazioni_CategoriaId",
                table: "Pianificazioni");

            migrationBuilder.DropIndex(
                name: "IX_Movimenti_CategoriaId",
                table: "Movimenti");

            migrationBuilder.DropColumn(
                name: "CategoriaId",
                table: "Pianificazioni");

            migrationBuilder.DropColumn(
                name: "ModalitaCategoria",
                table: "Pianificazioni");

            migrationBuilder.DropColumn(
                name: "VoceRicorrenteCategoriaName",
                table: "Pianificazioni");

            migrationBuilder.DropColumn(
                name: "CategoriaId",
                table: "Movimenti");
        }
    }
}
