using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class fixPartyMembers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Party_Aktor_secretaryId",
                table: "Party");

            migrationBuilder.DropForeignKey(
                name: "FK_Party_Aktor_spokesmanId",
                table: "Party");

            migrationBuilder.DropForeignKey(
                name: "FK_Party_Aktor_viceChairmanId",
                table: "Party");

            migrationBuilder.AddForeignKey(
                name: "FK_Party_Aktor_secretaryId",
                table: "Party",
                column: "secretaryId",
                principalTable: "Aktor",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Party_Aktor_spokesmanId",
                table: "Party",
                column: "spokesmanId",
                principalTable: "Aktor",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Party_Aktor_viceChairmanId",
                table: "Party",
                column: "viceChairmanId",
                principalTable: "Aktor",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Party_Aktor_secretaryId",
                table: "Party");

            migrationBuilder.DropForeignKey(
                name: "FK_Party_Aktor_spokesmanId",
                table: "Party");

            migrationBuilder.DropForeignKey(
                name: "FK_Party_Aktor_viceChairmanId",
                table: "Party");

            migrationBuilder.AddForeignKey(
                name: "FK_Party_Aktor_secretaryId",
                table: "Party",
                column: "secretaryId",
                principalTable: "Aktor",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Party_Aktor_spokesmanId",
                table: "Party",
                column: "spokesmanId",
                principalTable: "Aktor",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Party_Aktor_viceChairmanId",
                table: "Party",
                column: "viceChairmanId",
                principalTable: "Aktor",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
