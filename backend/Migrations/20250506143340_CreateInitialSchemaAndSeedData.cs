using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class CreateInitialSchemaAndSeedData : Migration
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
                name: "FakePartier",
                columns: table => new
                {
                    PartiId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PartiNavn = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FakePartier", x => x.PartiId);
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
                name: "PoliticianTwitterIds",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TwitterUserId = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    TwitterHandle = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PoliticianTwitterIds", x => x.Id);
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
                    Roles = table.Column<string>(type: "text", nullable: false)
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
                name: "FakePolitikere",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PolitikerNavn = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    date_of_birth = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Køn = table.Column<string>(type: "text", nullable: false),
                    Uddannelse = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Region = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Portræt = table.Column<byte[]>(type: "bytea", nullable: false),
                    PartiId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FakePolitikere", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FakePolitikere_FakePartier_PartiId",
                        column: x => x.PartiId,
                        principalTable: "FakePartier",
                        principalColumn: "PartiId",
                        onDelete: ReferentialAction.Restrict);
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
                name: "Polls",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Question = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PoliticianTwitterId = table.Column<string>(type: "text", nullable: false),
                    PoliticianId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Polls", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Polls_PoliticianTwitterIds_PoliticianId",
                        column: x => x.PoliticianId,
                        principalTable: "PoliticianTwitterIds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tweets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Text = table.Column<string>(type: "text", nullable: false),
                    ImageUrl = table.Column<string>(type: "text", nullable: true),
                    Likes = table.Column<int>(type: "integer", nullable: false),
                    Retweets = table.Column<int>(type: "integer", nullable: false),
                    Replies = table.Column<int>(type: "integer", nullable: false),
                    TwitterTweetId = table.Column<string>(type: "text", nullable: false),
                    PoliticianTwitterId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tweets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tweets_PoliticianTwitterIds_PoliticianTwitterId",
                        column: x => x.PoliticianTwitterId,
                        principalTable: "PoliticianTwitterIds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Subscriptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    PoliticianTwitterId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Subscriptions_PoliticianTwitterIds_PoliticianTwitterId",
                        column: x => x.PoliticianTwitterId,
                        principalTable: "PoliticianTwitterIds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Subscriptions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "daily_selections",
                columns: table => new
                {
                    selection_date = table.Column<DateOnly>(type: "date", nullable: false),
                    gamemode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    selected_politiker_id = table.Column<int>(type: "integer", nullable: false),
                    selected_quote_text = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_daily_selections", x => new { x.selection_date, x.gamemode });
                    table.ForeignKey(
                        name: "FK_daily_selections_FakePolitikere_selected_politiker_id",
                        column: x => x.selected_politiker_id,
                        principalTable: "FakePolitikere",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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

            migrationBuilder.CreateTable(
                name: "PoliticianQuotes",
                columns: table => new
                {
                    QuoteId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    QuoteText = table.Column<string>(type: "text", nullable: false),
                    PolitikerId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PoliticianQuotes", x => x.QuoteId);
                    table.ForeignKey(
                        name: "FK_PoliticianQuotes_FakePolitikere_PolitikerId",
                        column: x => x.PolitikerId,
                        principalTable: "FakePolitikere",
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

            migrationBuilder.CreateTable(
                name: "PollOptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OptionText = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Votes = table.Column<int>(type: "integer", nullable: false),
                    PollId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PollOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PollOptions_Polls_PollId",
                        column: x => x.PollId,
                        principalTable: "Polls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserVotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    PollId = table.Column<int>(type: "integer", nullable: false),
                    ChosenOptionId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserVotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserVotes_PollOptions_ChosenOptionId",
                        column: x => x.ChosenOptionId,
                        principalTable: "PollOptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserVotes_Polls_PollId",
                        column: x => x.PollId,
                        principalTable: "Polls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserVotes_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "FlashcardCollections",
                columns: new[] { "CollectionId", "Description", "DisplayOrder", "Title" },
                values: new object[,]
                {
                    { 1, "Kendte danske politikere", 1, "Politikere" },
                    { 2, "Grundlæggende politiske termer", 2, "Politiske Begreber" }
                });

            migrationBuilder.InsertData(
                table: "PoliticianTwitterIds",
                columns: new[] { "Id", "Name", "TwitterHandle", "TwitterUserId" },
                values: new object[,]
                {
                    { 1, "Statsministeriet", "Statsmin", "806068174567460864" },
                    { 2, "Venstre, Danmarks Liberale Parti", "venstredk", "123868861" },
                    { 3, "Troels Lund Poulsen", "troelslundp", "2965907578" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Email", "IsVerified", "PasswordHash", "Roles", "UserName", "VerificationToken" },
                values: new object[] { 1, "testuser@example.com", true, "hashed_password_placeholder", "[\"User\"]", "TestUser", null });

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
                table: "Subscriptions",
                columns: new[] { "Id", "PoliticianTwitterId", "UserId" },
                values: new object[,]
                {
                    { 1, 1, 1 },
                    { 2, 2, 1 },
                    { 3, 3, 1 }
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
                name: "IX_daily_selections_selected_politiker_id",
                table: "daily_selections",
                column: "selected_politiker_id");

            migrationBuilder.CreateIndex(
                name: "IX_FakePolitikere_PartiId",
                table: "FakePolitikere",
                column: "PartiId");

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
                name: "IX_PoliticianQuotes_PolitikerId",
                table: "PoliticianQuotes",
                column: "PolitikerId");

            migrationBuilder.CreateIndex(
                name: "IX_PoliticianTwitterIds_TwitterUserId",
                table: "PoliticianTwitterIds",
                column: "TwitterUserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PollOptions_PollId",
                table: "PollOptions",
                column: "PollId");

            migrationBuilder.CreateIndex(
                name: "IX_Polls_PoliticianId",
                table: "Polls",
                column: "PoliticianId");

            migrationBuilder.CreateIndex(
                name: "IX_Questions_PageId",
                table: "Questions",
                column: "PageId");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_PoliticianTwitterId",
                table: "Subscriptions",
                column: "PoliticianTwitterId");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_UserId",
                table: "Subscriptions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Tweets_PoliticianTwitterId_TwitterTweetId",
                table: "Tweets",
                columns: new[] { "PoliticianTwitterId", "TwitterTweetId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserVotes_ChosenOptionId",
                table: "UserVotes",
                column: "ChosenOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_UserVotes_PollId",
                table: "UserVotes",
                column: "PollId");

            migrationBuilder.CreateIndex(
                name: "IX_UserVotes_UserId_PollId",
                table: "UserVotes",
                columns: new[] { "UserId", "PollId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnswerOptions");

            migrationBuilder.DropTable(
                name: "CalendarEvents");

            migrationBuilder.DropTable(
                name: "daily_selections");

            migrationBuilder.DropTable(
                name: "Flashcards");

            migrationBuilder.DropTable(
                name: "GameTrackings");

            migrationBuilder.DropTable(
                name: "Party");

            migrationBuilder.DropTable(
                name: "PoliticianQuotes");

            migrationBuilder.DropTable(
                name: "Subscriptions");

            migrationBuilder.DropTable(
                name: "Tweets");

            migrationBuilder.DropTable(
                name: "UserVotes");

            migrationBuilder.DropTable(
                name: "Questions");

            migrationBuilder.DropTable(
                name: "FlashcardCollections");

            migrationBuilder.DropTable(
                name: "Aktor");

            migrationBuilder.DropTable(
                name: "FakePolitikere");

            migrationBuilder.DropTable(
                name: "PollOptions");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Pages");

            migrationBuilder.DropTable(
                name: "FakePartier");

            migrationBuilder.DropTable(
                name: "Polls");

            migrationBuilder.DropTable(
                name: "PoliticianTwitterIds");
        }
    }
}
