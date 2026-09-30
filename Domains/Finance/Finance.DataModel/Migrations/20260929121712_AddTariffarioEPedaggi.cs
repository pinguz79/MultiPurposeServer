using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Finance.DataModel.Migrations
{
    /// <inheritdoc />
    public partial class AddTariffarioEPedaggi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Caselli",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false, collation: "NOCASE")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Caselli", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Pedaggi",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    MovimentoId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CaselloEntrataId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CaselloUscitaId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pedaggi", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pedaggi_Caselli_CaselloEntrataId",
                        column: x => x.CaselloEntrataId,
                        principalTable: "Caselli",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Pedaggi_Caselli_CaselloUscitaId",
                        column: x => x.CaselloUscitaId,
                        principalTable: "Caselli",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Pedaggi_Movimenti_MovimentoId",
                        column: x => x.MovimentoId,
                        principalTable: "Movimenti",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TariffeTratte",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Formula = table.Column<string>(type: "TEXT", nullable: false),
                    ValidFrom = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    ValidTo = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    Index = table.Column<int>(type: "INTEGER", nullable: false),
                    CaselloAId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CaselloBId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TariffeTratte", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TariffeTratte_Caselli_CaselloAId",
                        column: x => x.CaselloAId,
                        principalTable: "Caselli",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TariffeTratte_Caselli_CaselloBId",
                        column: x => x.CaselloBId,
                        principalTable: "Caselli",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Caselli_Name",
                table: "Caselli",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Pedaggi_CaselloEntrataId",
                table: "Pedaggi",
                column: "CaselloEntrataId");

            migrationBuilder.CreateIndex(
                name: "IX_Pedaggi_CaselloUscitaId",
                table: "Pedaggi",
                column: "CaselloUscitaId");

            migrationBuilder.CreateIndex(
                name: "IX_Pedaggi_MovimentoId",
                table: "Pedaggi",
                column: "MovimentoId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TariffeTratte_CaselloAId_CaselloBId_Index",
                table: "TariffeTratte",
                columns: new[] { "CaselloAId", "CaselloBId", "Index" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TariffeTratte_CaselloBId",
                table: "TariffeTratte",
                column: "CaselloBId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Pedaggi");

            migrationBuilder.DropTable(
                name: "TariffeTratte");

            migrationBuilder.DropTable(
                name: "Caselli");
        }
    }
}
