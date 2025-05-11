using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class initialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Aktor",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    biografi = table.Column<string>(type: "text", nullable: true),
                    fornavn = table.Column<string>(type: "text", nullable: true),
                    efternavn = table.Column<string>(type: "text", nullable: true),
                    typeid = table.Column<int>(type: "integer", nullable: true),
                    gruppeNavnKort = table.Column<string>(type: "text", nullable: true),
                    navn = table.Column<string>(type: "text", nullable: true),
                    opdateringsdato = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    periodeid = table.Column<int>(type: "integer", nullable: false),
                    slutdato = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    startdato = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Party = table.Column<string>(type: "text", nullable: true),
                    PartyShortname = table.Column<string>(type: "text", nullable: true),
                    Sex = table.Column<string>(type: "text", nullable: true),
                    Born = table.Column<string>(type: "text", nullable: true),
                    EducationStatistic = table.Column<string>(type: "text", nullable: true),
                    PictureMiRes = table.Column<string>(type: "text", nullable: true),
                    FunctionFormattedTitle = table.Column<string>(type: "text", nullable: true),
                    FunctionStartDate = table.Column<string>(type: "text", nullable: true),
                    PositionsOfTrust = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    MinisterTitel = table.Column<string>(type: "text", nullable: true),
                    Ministers = table.Column<string>(type: "text", nullable: true),
                    Spokesmen = table.Column<string>(type: "text", nullable: true),
                    ParliamentaryPositionsOfTrust = table.Column<List<string>>(type: "text[]", nullable: true),
                    Constituencies = table.Column<string>(type: "text", nullable: true),
                    Nominations = table.Column<string>(type: "text", nullable: true),
                    Educations = table.Column<string>(type: "text", nullable: true),
                    Occupations = table.Column<string>(type: "text", nullable: true),
                    PublicationTitles = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Aktor", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CalendarEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "character varying(350)", maxLength: 350, nullable: false),
                    StartDateTimeUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Location = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    SourceUrl = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    LastScrapedUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalendarEvents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FlashcardCollections",
                columns: table => new
                {
                    CollectionId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlashcardCollections", x => x.CollectionId);
                });

            migrationBuilder.CreateTable(
                name: "Pages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    ParentPageId = table.Column<int>(type: "integer", nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pages_Pages_ParentPageId",
                        column: x => x.ParentPageId,
                        principalTable: "Pages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Email = table.Column<string>(type: "text", nullable: false),
                    UserName = table.Column<string>(type: "text", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    IsVerified = table.Column<bool>(type: "boolean", nullable: false),
                    VerificationToken = table.Column<string>(type: "text", nullable: true),
                    Roles = table.Column<List<string>>(type: "text[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Party",
                columns: table => new
                {
                    partyId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    partyName = table.Column<string>(type: "text", nullable: true),
                    partyShortName = table.Column<string>(type: "text", nullable: true),
                    partyProgram = table.Column<string>(type: "text", nullable: true),
                    politics = table.Column<string>(type: "text", nullable: true),
                    history = table.Column<string>(type: "text", nullable: true),
                    stats = table.Column<string>(type: "text", nullable: true),
                    chairmanId = table.Column<int>(type: "integer", nullable: true),
                    viceChairmanId = table.Column<int>(type: "integer", nullable: true),
                    secretaryId = table.Column<int>(type: "integer", nullable: true),
                    spokesmanId = table.Column<int>(type: "integer", nullable: true),
                    memberIds = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Party", x => x.partyId);
                    table.ForeignKey(
                        name: "FK_Party_Aktor_chairmanId",
                        column: x => x.chairmanId,
                        principalTable: "Aktor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Party_Aktor_secretaryId",
                        column: x => x.secretaryId,
                        principalTable: "Aktor",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Party_Aktor_spokesmanId",
                        column: x => x.spokesmanId,
                        principalTable: "Aktor",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Party_Aktor_viceChairmanId",
                        column: x => x.viceChairmanId,
                        principalTable: "Aktor",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Flashcards",
                columns: table => new
                {
                    FlashcardId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CollectionId = table.Column<int>(type: "integer", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    FrontContentType = table.Column<int>(type: "integer", nullable: false),
                    FrontText = table.Column<string>(type: "text", nullable: true),
                    FrontImagePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    BackContentType = table.Column<int>(type: "integer", nullable: false),
                    BackText = table.Column<string>(type: "text", nullable: true),
                    BackImagePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Flashcards", x => x.FlashcardId);
                    table.ForeignKey(
                        name: "FK_Flashcards_FlashcardCollections_CollectionId",
                        column: x => x.CollectionId,
                        principalTable: "FlashcardCollections",
                        principalColumn: "CollectionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Questions",
                columns: table => new
                {
                    QuestionId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    QuestionText = table.Column<string>(type: "text", nullable: false),
                    PageId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Questions", x => x.QuestionId);
                    table.ForeignKey(
                        name: "FK_Questions_Pages_PageId",
                        column: x => x.PageId,
                        principalTable: "Pages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AnswerOptions",
                columns: table => new
                {
                    AnswerOptionId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OptionText = table.Column<string>(type: "text", nullable: false),
                    IsCorrect = table.Column<bool>(type: "boolean", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    QuestionId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnswerOptions", x => x.AnswerOptionId);
                    table.ForeignKey(
                        name: "FK_AnswerOptions_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "QuestionId",
                        onDelete: ReferentialAction.Cascade);
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_AnswerOptions_QuestionId",
                table: "AnswerOptions",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_CalendarEvents_SourceUrl",
                table: "CalendarEvents",
                column: "SourceUrl",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Flashcards_CollectionId",
                table: "Flashcards",
                column: "CollectionId");

            migrationBuilder.CreateIndex(
                name: "IX_Pages_ParentPageId",
                table: "Pages",
                column: "ParentPageId");

            migrationBuilder.CreateIndex(
                name: "IX_Party_chairmanId",
                table: "Party",
                column: "chairmanId");

            migrationBuilder.CreateIndex(
                name: "IX_Party_secretaryId",
                table: "Party",
                column: "secretaryId");

            migrationBuilder.CreateIndex(
                name: "IX_Party_spokesmanId",
                table: "Party",
                column: "spokesmanId");

            migrationBuilder.CreateIndex(
                name: "IX_Party_viceChairmanId",
                table: "Party",
                column: "viceChairmanId");

            migrationBuilder.CreateIndex(
                name: "IX_Questions_PageId",
                table: "Questions",
                column: "PageId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnswerOptions");

            migrationBuilder.DropTable(
                name: "CalendarEvents");

            migrationBuilder.DropTable(
                name: "Flashcards");

            migrationBuilder.DropTable(
                name: "Party");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Questions");

            migrationBuilder.DropTable(
                name: "FlashcardCollections");

            migrationBuilder.DropTable(
                name: "Aktor");

            migrationBuilder.DropTable(
                name: "Pages");
        }
    }
}
