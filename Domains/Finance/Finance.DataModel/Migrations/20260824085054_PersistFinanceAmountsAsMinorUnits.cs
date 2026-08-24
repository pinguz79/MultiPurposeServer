using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Finance.DataModel.Migrations
{
    /// <inheritdoc />
    public partial class PersistFinanceAmountsAsMinorUnits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "InitialBalance",
                table: "Conti",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "TEXT");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE Conti SET InitialBalance = InitialBalance / 100.0;");

            migrationBuilder.AlterColumn<decimal>(
                name: "InitialBalance",
                table: "Conti",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "INTEGER");
        }
    }
}
