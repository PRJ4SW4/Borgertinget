using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable
#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace backend.Migrations
{
    public partial class OpdaterDatabasen : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Indsæt kollektionerne først
            migrationBuilder.InsertData(
                table: "FlashcardCollections",
                columns: new[] { "CollectionId", "Title", "Description", "DisplayOrder" },
                values: new object[,]
                {
                    { 1, "Statsministre", "Hvem er hvem i dansk politik?", 1 },
                    { 2, "Demokrati", "Basale begreber i det danske folkestyre", 2 },
                }
            );

            // Indsæt flashcards bagefter
            migrationBuilder.InsertData(
                table: "Flashcards",
                columns: new[]
                {
                    "FlashcardId",
                    "BackContentType",
                    "BackImagePath",
                    "BackText",
                    "CollectionId",
                    "DisplayOrder",
                    "FrontContentType",
                    "FrontImagePath",
                    "FrontText",
                },
                values: new object[,]
                {
                    {
                        1,
                        0,
                        null,
                        "Mette Frederiksen",
                        1,
                        1,
                        1,
                        "/uploads/flashcards/mettef.png",
                        null,
                    },
                    {
                        2,
                        0,
                        null,
                        "Lars Løkke Rasmussen",
                        1,
                        2,
                        1,
                        "/uploads/flashcards/larsl.png",
                        null,
                    },
                    {
                        3,
                        0,
                        null,
                        "Inger Støjberg",
                        1,
                        3,
                        0,
                        null,
                        "Hvem er formand for Danmarksdemokraterne?",
                    },
                    { 4, 0, null, "Folkestyre", 2, 1, 0, null, "Hvad betyder 'Demokrati'?" },
                    {
                        5,
                        0,
                        null,
                        "Statens budget for det kommende år",
                        2,
                        2,
                        0,
                        null,
                        "Hvad er 'Finansloven'?",
                    },
                }
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Slet kort først
            migrationBuilder.DeleteData(
                table: "Flashcards",
                keyColumn: "FlashcardId",
                keyValues: new object[] { 1, 2, 3, 4, 5 }
            );

            // Slet kollektioner bagefter
            migrationBuilder.DeleteData(
                table: "FlashcardCollections",
                keyColumn: "CollectionId",
                keyValues: new object[] { 1, 2 }
            );
        }
    }
}
