using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Finance.DataModel.Migrations
{
    /// <inheritdoc />
    public partial class AddPianificazioni : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE Movimenti SET Formula = REPLACE(REPLACE(Formula, '.', ''), ',', '.') WHERE Formula NOT LIKE '%[%';");

            migrationBuilder.AddColumn<Guid>(
                name: "PianificazioneId",
                table: "Movimenti",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Periodicita",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Frequenza = table.Column<int>(type: "INTEGER", nullable: false),
                    Intervallo = table.Column<int>(type: "INTEGER", nullable: false),
                    GiornoSettimana = table.Column<int>(type: "INTEGER", nullable: true),
                    SettimanaMese = table.Column<int>(type: "INTEGER", nullable: true),
                    GiornoMese = table.Column<int>(type: "INTEGER", nullable: true),
                    MeseAnno = table.Column<int>(type: "INTEGER", nullable: true),
                    FineMese = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Periodicita", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Pianificazioni",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    MovimentoFormula = table.Column<string>(type: "TEXT", nullable: false),
                    MovimentoDescription = table.Column<string>(type: "TEXT", nullable: false),
                    ValidFrom = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    ValidTo = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    PeriodicitaId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ContoId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pianificazioni", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pianificazioni_Conti_ContoId",
                        column: x => x.ContoId,
                        principalTable: "Conti",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Pianificazioni_Periodicita_PeriodicitaId",
                        column: x => x.PeriodicitaId,
                        principalTable: "Periodicita",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Movimenti_PianificazioneId",
                table: "Movimenti",
                column: "PianificazioneId");

            migrationBuilder.CreateIndex(
                name: "IX_Periodicita_Frequenza_Intervallo_GiornoSettimana_SettimanaMese_GiornoMese_MeseAnno_FineMese",
                table: "Periodicita",
                columns: new[] { "Frequenza", "Intervallo", "GiornoSettimana", "SettimanaMese", "GiornoMese", "MeseAnno", "FineMese" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Pianificazioni_ContoId",
                table: "Pianificazioni",
                column: "ContoId");

            migrationBuilder.CreateIndex(
                name: "IX_Pianificazioni_PeriodicitaId",
                table: "Pianificazioni",
                column: "PeriodicitaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Movimenti_Pianificazioni_PianificazioneId",
                table: "Movimenti",
                column: "PianificazioneId",
                principalTable: "Pianificazioni",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE Movimenti SET Formula = REPLACE(Formula, '.', ',') WHERE Formula NOT LIKE '%[%';");

            migrationBuilder.DropForeignKey(
                name: "FK_Movimenti_Pianificazioni_PianificazioneId",
                table: "Movimenti");

            migrationBuilder.DropTable(
                name: "Pianificazioni");

            migrationBuilder.DropTable(
                name: "Periodicita");

            migrationBuilder.DropIndex(
                name: "IX_Movimenti_PianificazioneId",
                table: "Movimenti");

            migrationBuilder.DropColumn(
                name: "PianificazioneId",
                table: "Movimenti");
        }
    }
}
