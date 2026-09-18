using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Selfaware.Migrations
{
    /// <inheritdoc />
    public partial class attempt16ofaddingsurvesessionidtouseranswerrs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Answers_Options_OptionId",
                table: "Answers");

            migrationBuilder.DropIndex(
                name: "IX_Answers_SurveySessionId",
                table: "Answers");

            migrationBuilder.CreateIndex(
                name: "IX_Answers_SurveySessionId_QuestionId",
                table: "Answers",
                columns: new[] { "SurveySessionId", "QuestionId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Answers_Options_OptionId",
                table: "Answers",
                column: "OptionId",
                principalTable: "Options",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Answers_Options_OptionId",
                table: "Answers");

            migrationBuilder.DropIndex(
                name: "IX_Answers_SurveySessionId_QuestionId",
                table: "Answers");

            migrationBuilder.CreateIndex(
                name: "IX_Answers_SurveySessionId",
                table: "Answers",
                column: "SurveySessionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Answers_Options_OptionId",
                table: "Answers",
                column: "OptionId",
                principalTable: "Options",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
