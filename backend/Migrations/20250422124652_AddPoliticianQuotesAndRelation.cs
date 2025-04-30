using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddPoliticianQuotesAndRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PoliticianQuote_FakePolitikere_PolitikerId",
                table: "PoliticianQuote");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PoliticianQuote",
                table: "PoliticianQuote");

            migrationBuilder.RenameTable(
                name: "PoliticianQuote",
                newName: "PoliticianQuotes");

            migrationBuilder.RenameIndex(
                name: "IX_PoliticianQuote_PolitikerId",
                table: "PoliticianQuotes",
                newName: "IX_PoliticianQuotes_PolitikerId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PoliticianQuotes",
                table: "PoliticianQuotes",
                column: "QuoteId");

            migrationBuilder.AddForeignKey(
                name: "FK_PoliticianQuotes_FakePolitikere_PolitikerId",
                table: "PoliticianQuotes",
                column: "PolitikerId",
                principalTable: "FakePolitikere",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PoliticianQuotes_FakePolitikere_PolitikerId",
                table: "PoliticianQuotes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PoliticianQuotes",
                table: "PoliticianQuotes");

            migrationBuilder.RenameTable(
                name: "PoliticianQuotes",
                newName: "PoliticianQuote");

            migrationBuilder.RenameIndex(
                name: "IX_PoliticianQuotes_PolitikerId",
                table: "PoliticianQuote",
                newName: "IX_PoliticianQuote_PolitikerId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PoliticianQuote",
                table: "PoliticianQuote",
                column: "QuoteId");

            migrationBuilder.AddForeignKey(
                name: "FK_PoliticianQuote_FakePolitikere_PolitikerId",
                table: "PoliticianQuote",
                column: "PolitikerId",
                principalTable: "FakePolitikere",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
