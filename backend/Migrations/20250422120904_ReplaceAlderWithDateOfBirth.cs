using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceAlderWithDateOfBirth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Alder",
                table: "FakePolitikere");

            migrationBuilder.AddColumn<DateTime>(
                name: "date_of_birth",
                table: "FakePolitikere",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "PoliticianQuote",
                columns: table => new
                {
                    QuoteId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    QuoteText = table.Column<string>(type: "text", nullable: false),
                    PolitikerId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PoliticianQuote", x => x.QuoteId);
                    table.ForeignKey(
                        name: "FK_PoliticianQuote_FakePolitikere_PolitikerId",
                        column: x => x.PolitikerId,
                        principalTable: "FakePolitikere",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PoliticianQuote_PolitikerId",
                table: "PoliticianQuote",
                column: "PolitikerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PoliticianQuote");

            migrationBuilder.DropColumn(
                name: "date_of_birth",
                table: "FakePolitikere");

            migrationBuilder.AddColumn<int>(
                name: "Alder",
                table: "FakePolitikere",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
