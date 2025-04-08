using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePageQuestionRelationshipToOneToMany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Questions_PageId",
                table: "Questions");

            migrationBuilder.CreateIndex(
                name: "IX_Questions_PageId",
                table: "Questions",
                column: "PageId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Questions_PageId",
                table: "Questions");

            migrationBuilder.CreateIndex(
                name: "IX_Questions_PageId",
                table: "Questions",
                column: "PageId",
                unique: true);
        }
    }
}
