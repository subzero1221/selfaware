using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Selfaware.Migrations
{
    /// <inheritdoc />
    public partial class actuallyaddedsubscales : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Questions_Subscale_SubscaleId",
                table: "Questions");

            migrationBuilder.DropForeignKey(
                name: "FK_Subscale_Quizzes_QuizId",
                table: "Subscale");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Subscale",
                table: "Subscale");

            migrationBuilder.RenameTable(
                name: "Subscale",
                newName: "Subscales");

            migrationBuilder.RenameIndex(
                name: "IX_Subscale_QuizId",
                table: "Subscales",
                newName: "IX_Subscales_QuizId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Subscales",
                table: "Subscales",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Questions_Subscales_SubscaleId",
                table: "Questions",
                column: "SubscaleId",
                principalTable: "Subscales",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Subscales_Quizzes_QuizId",
                table: "Subscales",
                column: "QuizId",
                principalTable: "Quizzes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Questions_Subscales_SubscaleId",
                table: "Questions");

            migrationBuilder.DropForeignKey(
                name: "FK_Subscales_Quizzes_QuizId",
                table: "Subscales");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Subscales",
                table: "Subscales");

            migrationBuilder.RenameTable(
                name: "Subscales",
                newName: "Subscale");

            migrationBuilder.RenameIndex(
                name: "IX_Subscales_QuizId",
                table: "Subscale",
                newName: "IX_Subscale_QuizId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Subscale",
                table: "Subscale",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Questions_Subscale_SubscaleId",
                table: "Questions",
                column: "SubscaleId",
                principalTable: "Subscale",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Subscale_Quizzes_QuizId",
                table: "Subscale",
                column: "QuizId",
                principalTable: "Quizzes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
