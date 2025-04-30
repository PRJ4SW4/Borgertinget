using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class FixedFakePolitikerPartiId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PartiId",
                table: "FakePolitikere",
                type: "integer",
                nullable: false,
                defaultValue: 0);

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

            migrationBuilder.CreateIndex(
                name: "IX_FakePolitikere_PartiId",
                table: "FakePolitikere",
                column: "PartiId");

            migrationBuilder.AddForeignKey(
                name: "FK_FakePolitikere_FakePartier_PartiId",
                table: "FakePolitikere",
                column: "PartiId",
                principalTable: "FakePartier",
                principalColumn: "PartiId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FakePolitikere_FakePartier_PartiId",
                table: "FakePolitikere");

            migrationBuilder.DropTable(
                name: "FakePartier");

            migrationBuilder.DropIndex(
                name: "IX_FakePolitikere_PartiId",
                table: "FakePolitikere");

            migrationBuilder.DropColumn(
                name: "PartiId",
                table: "FakePolitikere");
        }
    }
}
