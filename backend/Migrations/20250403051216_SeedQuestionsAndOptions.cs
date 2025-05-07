using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class SeedQuestionsAndOptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 1,
                column: "Content",
                value: "Indhold for Politik 101...");

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 2,
                column: "Content",
                value: "Indhold for Den Politiske Akse...");

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 3,
                column: "Content",
                value: "Indhold for Venstre vs Højre...");

            migrationBuilder.InsertData(
                table: "Questions",
                columns: new[] { "QuestionId", "PageId", "QuestionText" },
                values: new object[,]
                {
                    { 1, 1, "Hvad beskæftiger politologi sig primært med?" },
                    { 2, 1, "Hvilket begreb dækker over fordelingen af autoritet i et samfund?" },
                    { 3, 4, "Hvilket økonomisk princip forbindes ofte med højreorienteret politik?" },
                    { 4, 5, "Hvilken værdi vægtes typisk højt i venstreorienteret ideologi?" }
                });

            migrationBuilder.InsertData(
                table: "AnswerOptions",
                columns: new[] { "AnswerOptionId", "DisplayOrder", "IsCorrect", "OptionText", "QuestionId" },
                values: new object[,]
                {
                    { 1, 1, true, "Studiet af magtstrukturer og beslutningsprocesser", 1 },
                    { 2, 2, false, "Analyse af internationale handelsaftaler", 1 },
                    { 3, 3, false, "Udforskning af historiske monarkier", 1 },
                    { 4, 1, false, "Social mobilitet", 2 },
                    { 5, 2, true, "Magtdeling", 2 },
                    { 6, 3, false, "Kulturel assimilation", 2 },
                    { 7, 1, false, "Planøkonomi", 3 },
                    { 8, 2, false, "Høj grad af omfordeling", 3 },
                    { 9, 3, true, "Frit marked og privat ejendomsret", 3 },
                    { 10, 1, false, "Individuel konkurrence", 4 },
                    { 11, 2, true, "Social lighed og fællesskabets velfærd", 4 },
                    { 12, 3, false, "Traditionelle hierarkier", 4 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AnswerOptions",
                keyColumn: "AnswerOptionId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "AnswerOptions",
                keyColumn: "AnswerOptionId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "AnswerOptions",
                keyColumn: "AnswerOptionId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "AnswerOptions",
                keyColumn: "AnswerOptionId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "AnswerOptions",
                keyColumn: "AnswerOptionId",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "AnswerOptions",
                keyColumn: "AnswerOptionId",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "AnswerOptions",
                keyColumn: "AnswerOptionId",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "AnswerOptions",
                keyColumn: "AnswerOptionId",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "AnswerOptions",
                keyColumn: "AnswerOptionId",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "AnswerOptions",
                keyColumn: "AnswerOptionId",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "AnswerOptions",
                keyColumn: "AnswerOptionId",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "AnswerOptions",
                keyColumn: "AnswerOptionId",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Questions",
                keyColumn: "QuestionId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Questions",
                keyColumn: "QuestionId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Questions",
                keyColumn: "QuestionId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Questions",
                keyColumn: "QuestionId",
                keyValue: 4);

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 1,
                column: "Content",
                value: "Lorem Ipsum lorem ipsum...\n\n her er en Heading skirt");

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 2,
                column: "Content",
                value: "Lorem Ipsum lorem ipsum...");

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 3,
                column: "Content",
                value: "Lorem Ipsum lorem ipsum...");
        }
    }
}
