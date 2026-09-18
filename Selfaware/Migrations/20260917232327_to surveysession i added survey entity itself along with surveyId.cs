using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Selfaware.Migrations
{
    /// <inheritdoc />
    public partial class tosurveysessioniaddedsurveyentityitselfalongwithsurveyId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_SurveySessions_SurveyId",
                table: "SurveySessions",
                column: "SurveyId");

            migrationBuilder.AddForeignKey(
                name: "FK_SurveySessions_Surveys_SurveyId",
                table: "SurveySessions",
                column: "SurveyId",
                principalTable: "Surveys",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SurveySessions_Surveys_SurveyId",
                table: "SurveySessions");

            migrationBuilder.DropIndex(
                name: "IX_SurveySessions_SurveyId",
                table: "SurveySessions");
        }
    }
}
