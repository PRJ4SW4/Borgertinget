using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddPartyEnt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Party",
                columns: table => new
                {
                    partyId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    partyName = table.Column<string>(type: "text", nullable: true),
                    partyProgram = table.Column<string>(type: "text", nullable: true),
                    poilitics = table.Column<string>(type: "text", nullable: true),
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
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Party_Aktor_spokesmanId",
                        column: x => x.spokesmanId,
                        principalTable: "Aktor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Party_Aktor_viceChairmanId",
                        column: x => x.viceChairmanId,
                        principalTable: "Aktor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Party");
        }
    }
}
