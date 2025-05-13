using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class FlashcardSeedingReturns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            /*
            migrationBuilder.InsertData(
                table: "Flashcards",
                columns: new[] { "FlashcardId", "BackContentType", "BackImagePath", "BackText", "CollectionId", "DisplayOrder", "FrontContentType", "FrontImagePath", "FrontText" },
                values: new object[,]
                {
                    { 1, 0, null, "Mette Frederiksen", 1, 1, 1, "/uploads/flashcards/mettef.png", null },
                    { 2, 0, null, "Lars Løkke Rasmussen", 1, 2, 1, "/uploads/flashcards/larsl.png", null },
                    { 3, 0, null, "Inger Støjberg", 1, 3, 0, null, "Hvem er formand for Danmarksdemokraterne?" },
                    { 4, 0, null, "Folkestyre", 2, 1, 0, null, "Hvad betyder 'Demokrati'?" },
                    { 5, 0, null, "Statens budget for det kommende år", 2, 2, 0, null, "Hvad er 'Finansloven'?" }
                });
            */
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            /*
            migrationBuilder.DeleteData(
                table: "Flashcards",
                keyColumn: "FlashcardId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Flashcards",
                keyColumn: "FlashcardId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Flashcards",
                keyColumn: "FlashcardId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Flashcards",
                keyColumn: "FlashcardId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Flashcards",
                keyColumn: "FlashcardId",
                keyValue: 5);
            */
        }
    }
}
