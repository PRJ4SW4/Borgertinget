using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddDailySelectionsTableAndTrackingLogic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "Portræt",
                table: "FakePolitikere",
                type: "bytea",
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.CreateTable(
                name: "daily_selections",
                columns: table => new
                {
                    selection_date = table.Column<DateOnly>(type: "date", nullable: false),
                    gamemode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    selected_politiker_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_daily_selections", x => new { x.selection_date, x.gamemode });
                    table.ForeignKey(
                        name: "FK_daily_selections_FakePolitikere_selected_politiker_id",
                        column: x => x.selected_politiker_id,
                        principalTable: "FakePolitikere",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_daily_selections_selected_politiker_id",
                table: "daily_selections",
                column: "selected_politiker_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "daily_selections");

            migrationBuilder.DropColumn(
                name: "Portræt",
                table: "FakePolitikere");
        }
    }
}
