using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddAktor206 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "PoliticianTwitterIds",
                keyColumn: "Id",
                keyValue: 3,
                column: "AktorId",
                value: 206);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "PoliticianTwitterIds",
                keyColumn: "Id",
                keyValue: 3,
                column: "AktorId",
                value: null);
        }
    }
}
