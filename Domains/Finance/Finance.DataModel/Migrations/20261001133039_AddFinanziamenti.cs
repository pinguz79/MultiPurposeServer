using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Finance.DataModel.Migrations
{
    /// <inheritdoc />
    public partial class AddFinanziamenti : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Finanziamenti",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false, collation: "NOCASE"),
                    DisplayName = table.Column<string>(type: "TEXT", nullable: false),
                    Lender = table.Column<string>(type: "TEXT", nullable: false),
                    InitialPrincipal = table.Column<long>(type: "INTEGER", nullable: false),
                    Tan = table.Column<decimal>(type: "TEXT", nullable: false),
                    Installment = table.Column<long>(type: "INTEGER", nullable: false),
                    Insurance = table.Column<long>(type: "INTEGER", nullable: false),
                    Fees = table.Column<long>(type: "INTEGER", nullable: false),
                    FirstDueDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    DueDay = table.Column<int>(type: "INTEGER", nullable: false),
                    InstallmentCount = table.Column<int>(type: "INTEGER", nullable: false),
                    IsClosed = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Finanziamenti", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RiallineamentiFinanziamenti",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    InstallmentNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    Principal = table.Column<long>(type: "INTEGER", nullable: false),
                    FinanziamentoId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RiallineamentiFinanziamenti", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RiallineamentiFinanziamenti_Finanziamenti_FinanziamentoId",
                        column: x => x.FinanziamentoId,
                        principalTable: "Finanziamenti",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Finanziamenti_Name",
                table: "Finanziamenti",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RiallineamentiFinanziamenti_FinanziamentoId_InstallmentNumber",
                table: "RiallineamentiFinanziamenti",
                columns: new[] { "FinanziamentoId", "InstallmentNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RiallineamentiFinanziamenti");

            migrationBuilder.DropTable(
                name: "Finanziamenti");
        }
    }
}
