using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Selfaware.Migrations
{
    /// <inheritdoc />
    public partial class touseranswersaddedsurvesessionidandsurveysession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Answers_SurveySessionId_QuestionId",
                table: "Answers");

            migrationBuilder.CreateIndex(
                name: "IX_Answers_SurveySessionId",
                table: "Answers",
                column: "SurveySessionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Answers_SurveySessionId",
                table: "Answers");

            migrationBuilder.CreateIndex(
                name: "IX_Answers_SurveySessionId_QuestionId",
                table: "Answers",
                columns: new[] { "SurveySessionId", "QuestionId" },
                unique: true);
        }
    }
}
