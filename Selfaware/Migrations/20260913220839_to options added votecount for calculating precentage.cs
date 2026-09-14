using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Selfaware.Migrations
{
    /// <inheritdoc />
    public partial class tooptionsaddedvotecountforcalculatingprecentage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "VoteCount",
                table: "Option",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Surveys_QuizId",
                table: "Surveys",
                column: "QuizId");

            migrationBuilder.AddForeignKey(
                name: "FK_Surveys_Quizzes_QuizId",
                table: "Surveys",
                column: "QuizId",
                principalTable: "Quizzes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Surveys_Quizzes_QuizId",
                table: "Surveys");

            migrationBuilder.DropIndex(
                name: "IX_Surveys_QuizId",
                table: "Surveys");

            migrationBuilder.DropColumn(
                name: "VoteCount",
                table: "Option");
        }
    }
}
