using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class Addmette : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "PoliticianTwitterIds",
                columns: new[] { "Id", "AktorId", "Name", "TwitterHandle", "TwitterUserId" },
                values: new object[,]
                {
                    { 1, 138, "Statsministeriet", "Statsmin", "806068174567460864" },
                    { 2, null, "Venstre, Danmarks Liberale Parti", "venstredk", "123868861" },
                    { 3, null, "Troels Lund Poulsen", "troelslundp", "2965907578" }
                });

            migrationBuilder.InsertData(
                table: "Polls",
                columns: new[] { "Id", "CreatedAt", "EndedAt", "PoliticianTwitterId", "Question" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 4, 15, 10, 0, 0, 0, DateTimeKind.Utc), null, 1, "Hvad synes du om den nye bro?" },
                    { 2, new DateTime(2025, 4, 28, 14, 30, 0, 0, DateTimeKind.Utc), null, 1, "Skal Danmark øge investeringer i vedvarende energi?" }
                });

            migrationBuilder.InsertData(
                table: "PollOptions",
                columns: new[] { "Id", "OptionText", "PollId", "Votes" },
                values: new object[,]
                {
                    { 1, "Den er fantastisk!", 1, 5 },
                    { 2, "Den er ok, men dyr.", 1, 12 },
                    { 3, "Den er unødvendig.", 1, 3 },
                    { 4, "Ja, meget mere end nu", 2, 42 },
                    { 5, "Ja, lidt mere", 2, 28 },
                    { 6, "Nej, det nuværende niveau er passende", 2, 15 },
                    { 7, "Nej, vi bør investere mindre", 2, 8 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "PoliticianTwitterIds",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "PoliticianTwitterIds",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "PollOptions",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "PollOptions",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "PollOptions",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "PollOptions",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "PollOptions",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "PollOptions",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "PollOptions",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Polls",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Polls",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "PoliticianTwitterIds",
                keyColumn: "Id",
                keyValue: 1);
        }
    }
}
