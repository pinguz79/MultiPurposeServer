using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MultiPurposeServer.Migrations
{
    /// <inheritdoc />
    public partial class AlbumHierarchy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ParentId",
                table: "Albums",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Albums_ParentId",
                table: "Albums",
                column: "ParentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Albums_Albums_ParentId",
                table: "Albums",
                column: "ParentId",
                principalTable: "Albums",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Albums_Albums_ParentId",
                table: "Albums");

            migrationBuilder.DropIndex(
                name: "IX_Albums_ParentId",
                table: "Albums");

            migrationBuilder.DropColumn(
                name: "ParentId",
                table: "Albums");
        }
    }
}
