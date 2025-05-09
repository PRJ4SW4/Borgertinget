using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class MaybeWorks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Subscriptions_PoliticianTwitterIds_PoliticianTwitterId",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "PoliticianId",
                table: "Polls");

            migrationBuilder.AlterColumn<int>(
                name: "PoliticianTwitterId",
                table: "Tweets",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "PoliticianTwitterId",
                table: "Subscriptions",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.InsertData(
                table: "Polls",
                columns: new[] { "Id", "CreatedAt", "EndedAt", "PoliticianTwitterId", "Question" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 4, 15, 10, 0, 0, 0, DateTimeKind.Utc), null, "2965907578", "Hvad synes du om den nye bro?" },
                    { 2, new DateTime(2025, 4, 28, 14, 30, 0, 0, DateTimeKind.Utc), null, "2965907578", "Skal Danmark øge investeringer i vedvarende energi?" }
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

            migrationBuilder.AddForeignKey(
                name: "FK_Subscriptions_PoliticianTwitterIds_PoliticianTwitterId",
                table: "Subscriptions",
                column: "PoliticianTwitterId",
                principalTable: "PoliticianTwitterIds",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Subscriptions_PoliticianTwitterIds_PoliticianTwitterId",
                table: "Subscriptions");

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

            migrationBuilder.AlterColumn<int>(
                name: "PoliticianTwitterId",
                table: "Tweets",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "PoliticianTwitterId",
                table: "Subscriptions",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PoliticianId",
                table: "Polls",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_Subscriptions_PoliticianTwitterIds_PoliticianTwitterId",
                table: "Subscriptions",
                column: "PoliticianTwitterId",
                principalTable: "PoliticianTwitterIds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
