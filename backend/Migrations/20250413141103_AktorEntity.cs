using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AktorEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Aktor",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
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
                    Ministers = table.Column<List<string>>(type: "text[]", nullable: true),
                    Spokesmen = table.Column<List<string>>(type: "text[]", nullable: true),
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Aktor");
        }
    }
}
