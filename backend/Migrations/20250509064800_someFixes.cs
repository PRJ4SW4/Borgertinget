using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class someFixes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PoliticianTwitterIds_Aktor_AktorId",
                table: "PoliticianTwitterIds");

            migrationBuilder.DropForeignKey(
                name: "FK_Polls_PoliticianTwitterIds_PoliticianId",
                table: "Polls");

            migrationBuilder.DropForeignKey(
                name: "FK_Subscriptions_PoliticianTwitterIds_PoliticianId",
                table: "Subscriptions");

            migrationBuilder.DropForeignKey(
                name: "FK_Tweets_PoliticianTwitterIds_PoliticianId",
                table: "Tweets");

            migrationBuilder.DropIndex(
                name: "IX_UserVotes_UserId",
                table: "UserVotes");

            migrationBuilder.DropIndex(
                name: "IX_Tweets_PoliticianId",
                table: "Tweets");

            migrationBuilder.DropIndex(
                name: "IX_Subscriptions_PoliticianId",
                table: "Subscriptions");

            migrationBuilder.DropIndex(
                name: "IX_Polls_PoliticianId",
                table: "Polls");

            migrationBuilder.DropIndex(
                name: "IX_PoliticianTwitterIds_AktorId",
                table: "PoliticianTwitterIds");

            migrationBuilder.DropColumn(
                name: "PoliticianId",
                table: "Tweets");

            migrationBuilder.DropColumn(
                name: "PoliticianId",
                table: "Subscriptions");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_PoliticianTwitterIds_TwitterUserId",
                table: "PoliticianTwitterIds",
                column: "TwitterUserId");

            migrationBuilder.InsertData(
                table: "PoliticianTwitterIds",
                columns: new[] { "Id", "AktorId", "Name", "TwitterHandle", "TwitterUserId" },
                values: new object[,]
                {
                    { 1, null, "Statsministeriet", "Statsmin", "806068174567460864" },
                    { 2, null, "Venstre, Danmarks Liberale Parti", "venstredk", "123868861" },
                    { 3, null, "Troels Lund Poulsen", "troelslundp", "2965907578" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserVotes_UserId_PollId",
                table: "UserVotes",
                columns: new[] { "UserId", "PollId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tweets_PoliticianTwitterId_TwitterTweetId",
                table: "Tweets",
                columns: new[] { "PoliticianTwitterId", "TwitterTweetId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_PoliticianTwitterId",
                table: "Subscriptions",
                column: "PoliticianTwitterId");

            migrationBuilder.CreateIndex(
                name: "IX_Polls_PoliticianTwitterId",
                table: "Polls",
                column: "PoliticianTwitterId");

            migrationBuilder.CreateIndex(
                name: "IX_PoliticianTwitterIds_AktorId",
                table: "PoliticianTwitterIds",
                column: "AktorId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PoliticianTwitterIds_TwitterUserId",
                table: "PoliticianTwitterIds",
                column: "TwitterUserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PoliticianTwitterIds_Aktor_AktorId",
                table: "PoliticianTwitterIds",
                column: "AktorId",
                principalTable: "Aktor",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Polls_PoliticianTwitterIds_PoliticianTwitterId",
                table: "Polls",
                column: "PoliticianTwitterId",
                principalTable: "PoliticianTwitterIds",
                principalColumn: "TwitterUserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Subscriptions_PoliticianTwitterIds_PoliticianTwitterId",
                table: "Subscriptions",
                column: "PoliticianTwitterId",
                principalTable: "PoliticianTwitterIds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Tweets_PoliticianTwitterIds_PoliticianTwitterId",
                table: "Tweets",
                column: "PoliticianTwitterId",
                principalTable: "PoliticianTwitterIds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PoliticianTwitterIds_Aktor_AktorId",
                table: "PoliticianTwitterIds");

            migrationBuilder.DropForeignKey(
                name: "FK_Polls_PoliticianTwitterIds_PoliticianTwitterId",
                table: "Polls");

            migrationBuilder.DropForeignKey(
                name: "FK_Subscriptions_PoliticianTwitterIds_PoliticianTwitterId",
                table: "Subscriptions");

            migrationBuilder.DropForeignKey(
                name: "FK_Tweets_PoliticianTwitterIds_PoliticianTwitterId",
                table: "Tweets");

            migrationBuilder.DropIndex(
                name: "IX_UserVotes_UserId_PollId",
                table: "UserVotes");

            migrationBuilder.DropIndex(
                name: "IX_Tweets_PoliticianTwitterId_TwitterTweetId",
                table: "Tweets");

            migrationBuilder.DropIndex(
                name: "IX_Subscriptions_PoliticianTwitterId",
                table: "Subscriptions");

            migrationBuilder.DropIndex(
                name: "IX_Polls_PoliticianTwitterId",
                table: "Polls");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_PoliticianTwitterIds_TwitterUserId",
                table: "PoliticianTwitterIds");

            migrationBuilder.DropIndex(
                name: "IX_PoliticianTwitterIds_AktorId",
                table: "PoliticianTwitterIds");

            migrationBuilder.DropIndex(
                name: "IX_PoliticianTwitterIds_TwitterUserId",
                table: "PoliticianTwitterIds");

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

            migrationBuilder.AddColumn<int>(
                name: "PoliticianId",
                table: "Tweets",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PoliticianId",
                table: "Subscriptions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_UserVotes_UserId",
                table: "UserVotes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Tweets_PoliticianId",
                table: "Tweets",
                column: "PoliticianId");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_PoliticianId",
                table: "Subscriptions",
                column: "PoliticianId");

            migrationBuilder.CreateIndex(
                name: "IX_Polls_PoliticianId",
                table: "Polls",
                column: "PoliticianId");

            migrationBuilder.CreateIndex(
                name: "IX_PoliticianTwitterIds_AktorId",
                table: "PoliticianTwitterIds",
                column: "AktorId");

            migrationBuilder.AddForeignKey(
                name: "FK_PoliticianTwitterIds_Aktor_AktorId",
                table: "PoliticianTwitterIds",
                column: "AktorId",
                principalTable: "Aktor",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Polls_PoliticianTwitterIds_PoliticianId",
                table: "Polls",
                column: "PoliticianId",
                principalTable: "PoliticianTwitterIds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Subscriptions_PoliticianTwitterIds_PoliticianId",
                table: "Subscriptions",
                column: "PoliticianId",
                principalTable: "PoliticianTwitterIds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Tweets_PoliticianTwitterIds_PoliticianId",
                table: "Tweets",
                column: "PoliticianId",
                principalTable: "PoliticianTwitterIds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
