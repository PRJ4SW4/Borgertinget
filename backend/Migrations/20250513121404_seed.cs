using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class seed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "PoliticianTwitterIds",
                keyColumn: "Id",
                keyValue: 1,
                column: "AktorId",
                value: 138);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "PoliticianTwitterIds",
                keyColumn: "Id",
                keyValue: 1,
                column: "AktorId",
                value: null);
        }
    }
}
