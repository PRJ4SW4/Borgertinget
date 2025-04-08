﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class PolidleDbSetup : Migration
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

            migrationBuilder.DeleteData(
                table: "FlashcardCollections",
                keyColumn: "CollectionId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "FlashcardCollections",
                keyColumn: "CollectionId",
                keyValue: 2);

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

            migrationBuilder.DeleteData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.CreateTable(
                name: "FakePolitikere",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PolitikerNavn = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Alder = table.Column<int>(type: "integer", nullable: false),
                    Køn = table.Column<string>(type: "text", nullable: false),
                    Uddannelse = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Region = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FakePolitikere", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GameTrackings",
                columns: table => new
                {
                    politiker_id = table.Column<int>(type: "integer", nullable: false),
                    gamemode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    lastselecteddate = table.Column<DateOnly>(type: "date", nullable: true),
                    algovægt = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameTrackings", x => new { x.politiker_id, x.gamemode });
                    table.ForeignKey(
                        name: "FK_GameTrackings_FakePolitikere_politiker_id",
                        column: x => x.politiker_id,
                        principalTable: "FakePolitikere",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GameTrackings");

            migrationBuilder.DropTable(
                name: "FakePolitikere");

            migrationBuilder.InsertData(
                table: "FlashcardCollections",
                columns: new[] { "CollectionId", "Description", "DisplayOrder", "Title" },
                values: new object[,]
                {
                    { 1, null, 1, "Politikerne og deres navne" },
                    { 2, null, 2, "Politiske begreber" }
                });

            migrationBuilder.InsertData(
                table: "Pages",
                columns: new[] { "Id", "Content", "DisplayOrder", "ParentPageId", "Title" },
                values: new object[] { 1, "Indhold for Politik 101...", 1, null, "Politik 101" });

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

            migrationBuilder.InsertData(
                table: "Pages",
                columns: new[] { "Id", "Content", "DisplayOrder", "ParentPageId", "Title" },
                values: new object[] { 2, "Indhold for Den Politiske Akse...", 1, 1, "Den Politiske Akse" });

            migrationBuilder.InsertData(
                table: "Questions",
                columns: new[] { "QuestionId", "PageId", "QuestionText" },
                values: new object[,]
                {
                    { 1, 1, "Hvad beskæftiger politologi sig primært med?" },
                    { 2, 1, "Hvilket begreb dækker over fordelingen af autoritet i et samfund?" }
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
                    { 6, 3, false, "Kulturel assimilation", 2 }
                });

            migrationBuilder.InsertData(
                table: "Pages",
                columns: new[] { "Id", "Content", "DisplayOrder", "ParentPageId", "Title" },
                values: new object[,]
                {
                    { 3, "Indhold for Venstre vs Højre...", 1, 2, "Venstre vs Højre" },
                    { 4, "Højre er at være højre...", 1, 3, "Højre" },
                    { 5, "Venstre er at være venstre...", 2, 3, "Venstre" }
                });

            migrationBuilder.InsertData(
                table: "Questions",
                columns: new[] { "QuestionId", "PageId", "QuestionText" },
                values: new object[,]
                {
                    { 3, 4, "Hvilket økonomisk princip forbindes ofte med højreorienteret politik?" },
                    { 4, 5, "Hvilken værdi vægtes typisk højt i venstreorienteret ideologi?" }
                });

            migrationBuilder.InsertData(
                table: "AnswerOptions",
                columns: new[] { "AnswerOptionId", "DisplayOrder", "IsCorrect", "OptionText", "QuestionId" },
                values: new object[,]
                {
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
