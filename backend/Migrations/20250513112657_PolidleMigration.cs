using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class PolidleMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ParliamentaryPositionsOfTrust",
                table: "Aktor",
                type: "text",
                nullable: true,
                oldClrType: typeof(List<string>),
                oldType: "text[]",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "daily_selections",
                columns: table => new
                {
                    selection_date = table.Column<DateOnly>(type: "date", nullable: false),
                    gamemode = table.Column<string>(type: "text", nullable: false),
                    selected_politiker_id = table.Column<int>(type: "integer", nullable: false),
                    selected_quote_text = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_daily_selections", x => new { x.selection_date, x.gamemode });
                    table.ForeignKey(
                        name: "FK_daily_selections_Aktor_selected_politiker_id",
                        column: x => x.selected_politiker_id,
                        principalTable: "Aktor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GamemodeTrackers",
                columns: table => new
                {
                    politiker_id = table.Column<int>(type: "integer", nullable: false),
                    gamemode = table.Column<string>(type: "text", nullable: false),
                    lastselecteddate = table.Column<DateOnly>(type: "date", nullable: true),
                    algovægt = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GamemodeTrackers", x => new { x.politiker_id, x.gamemode });
                    table.ForeignKey(
                        name: "FK_GamemodeTrackers_Aktor_politiker_id",
                        column: x => x.politiker_id,
                        principalTable: "Aktor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PoliticianQuotes",
                columns: table => new
                {
                    QuoteId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    QuoteText = table.Column<string>(type: "text", nullable: false),
                    AktorId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PoliticianQuotes", x => x.QuoteId);
                    table.ForeignKey(
                        name: "FK_PoliticianQuotes_Aktor_AktorId",
                        column: x => x.AktorId,
                        principalTable: "Aktor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_daily_selections_selected_politiker_id",
                table: "daily_selections",
                column: "selected_politiker_id");

            migrationBuilder.CreateIndex(
                name: "IX_PoliticianQuotes_AktorId",
                table: "PoliticianQuotes",
                column: "AktorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "daily_selections");

            migrationBuilder.DropTable(
                name: "GamemodeTrackers");

            migrationBuilder.DropTable(
                name: "PoliticianQuotes");

            migrationBuilder.AlterColumn<List<string>>(
                name: "ParliamentaryPositionsOfTrust",
                table: "Aktor",
                type: "text[]",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
