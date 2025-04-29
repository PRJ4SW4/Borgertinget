using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddAktorIdRelationAndSeedPoll : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AktorId",
                table: "PoliticianTwitterIds",
                type: "integer",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "PoliticianTwitterIds",
                keyColumn: "Id",
                keyValue: 1,
                column: "AktorId",
                value: 138);

            migrationBuilder.UpdateData(
                table: "PoliticianTwitterIds",
                keyColumn: "Id",
                keyValue: 2,
                column: "AktorId",
                value: null);

            migrationBuilder.UpdateData(
                table: "PoliticianTwitterIds",
                keyColumn: "Id",
                keyValue: 3,
                column: "AktorId",
                value: null);

            migrationBuilder.CreateIndex(
                name: "IX_PoliticianTwitterIds_AktorId",
                table: "PoliticianTwitterIds",
                column: "AktorId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PoliticianTwitterIds_Aktor_AktorId",
                table: "PoliticianTwitterIds",
                column: "AktorId",
                principalTable: "Aktor",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PoliticianTwitterIds_Aktor_AktorId",
                table: "PoliticianTwitterIds");

            migrationBuilder.DropIndex(
                name: "IX_PoliticianTwitterIds_AktorId",
                table: "PoliticianTwitterIds");

            migrationBuilder.DropColumn(
                name: "AktorId",
                table: "PoliticianTwitterIds");
        }
    }
}
