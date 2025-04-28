using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddAktorModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
    }
}
