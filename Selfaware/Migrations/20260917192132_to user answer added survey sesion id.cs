using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Selfaware.Migrations
{
    /// <inheritdoc />
    public partial class touseransweraddedsurveysesionid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SurveySessionId",
                table: "Answers",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Answers_SurveySessionId_QuestionId",
                table: "Answers",
                columns: new[] { "SurveySessionId", "QuestionId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Answers_SurveySessionId_QuestionId",
                table: "Answers");

            migrationBuilder.DropColumn(
                name: "SurveySessionId",
                table: "Answers");
        }
    }
}
