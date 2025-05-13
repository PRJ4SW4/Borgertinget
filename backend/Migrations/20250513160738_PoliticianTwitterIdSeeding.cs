using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class PoliticianTwitterIdSeeding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "PoliticianTwitterIds",
                columns: new[] { "Id", "AktorId", "Name", "TwitterHandle", "TwitterUserId" },
                values: new object[,]
                {
                    { 1, null, "Statsministeriet", "Statsmin", "806068174567460864" },
                    { 2, null, "Venstre, Danmarks Liberale Parti", "venstredk", "123868861" },
                    { 3, null, "Troels Lund Poulsen", "troelslundp", "2965907578" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "PoliticianTwitterIds",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "PoliticianTwitterIds",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "PoliticianTwitterIds",
                keyColumn: "Id",
                keyValue: 3);
        }
    }
}
