using System;

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Finance.DataModel.Migrations
{
    /// <inheritdoc />
    public partial class AddParametriContoECorrelazioniPianificazioni : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CorrelazioniPianificazioni",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    PianificazioneAId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PianificazioneBId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CorrelazioniPianificazioni", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CorrelazioniPianificazioni_Pianificazioni_PianificazioneAId",
                        column: x => x.PianificazioneAId,
                        principalTable: "Pianificazioni",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CorrelazioniPianificazioni_Pianificazioni_PianificazioneBId",
                        column: x => x.PianificazioneBId,
                        principalTable: "Pianificazioni",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ParametriConto",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false, collation: "NOCASE"),
                    DisplayName = table.Column<string>(type: "TEXT", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Value = table.Column<decimal>(type: "TEXT", nullable: false),
                    ValidFrom = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    ValidTo = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    Index = table.Column<int>(type: "INTEGER", nullable: false),
                    ContoId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParametriConto", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ParametriConto_Conti_ContoId",
                        column: x => x.ContoId,
                        principalTable: "Conti",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CorrelazioniPianificazioni_PianificazioneAId_PianificazioneBId",
                table: "CorrelazioniPianificazioni",
                columns: new[] { "PianificazioneAId", "PianificazioneBId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CorrelazioniPianificazioni_PianificazioneBId",
                table: "CorrelazioniPianificazioni",
                column: "PianificazioneBId");

            migrationBuilder.CreateIndex(
                name: "IX_ParametriConto_ContoId_Name_Index",
                table: "ParametriConto",
                columns: new[] { "ContoId", "Name", "Index" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CorrelazioniPianificazioni");

            migrationBuilder.DropTable(
                name: "ParametriConto");
        }
    }
}
