using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Selfaware.Migrations
{
    /// <inheritdoc />
    public partial class againuseransweraddedentities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Answers_OptionId",
                table: "Answers",
                column: "OptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Answers_QuestionId",
                table: "Answers",
                column: "QuestionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Answers_Options_OptionId",
                table: "Answers",
                column: "OptionId",
                principalTable: "Options",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Answers_Questions_QuestionId",
                table: "Answers",
                column: "QuestionId",
                principalTable: "Questions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Answers_SurveySessions_SurveySessionId",
                table: "Answers",
                column: "SurveySessionId",
                principalTable: "SurveySessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Answers_Options_OptionId",
                table: "Answers");

            migrationBuilder.DropForeignKey(
                name: "FK_Answers_Questions_QuestionId",
                table: "Answers");

            migrationBuilder.DropForeignKey(
                name: "FK_Answers_SurveySessions_SurveySessionId",
                table: "Answers");

            migrationBuilder.DropIndex(
                name: "IX_Answers_OptionId",
                table: "Answers");

            migrationBuilder.DropIndex(
                name: "IX_Answers_QuestionId",
                table: "Answers");
        }
    }
}
