using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Selfaware.Migrations
{
    /// <inheritdoc />
    public partial class ToSurveySessioniattacheduserAnswers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SurveySessionId1",
                table: "Answers",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Answers_SurveySessionId1",
                table: "Answers",
                column: "SurveySessionId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Answers_SurveySessions_SurveySessionId1",
                table: "Answers",
                column: "SurveySessionId1",
                principalTable: "SurveySessions",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Answers_SurveySessions_SurveySessionId1",
                table: "Answers");

            migrationBuilder.DropIndex(
                name: "IX_Answers_SurveySessionId1",
                table: "Answers");

            migrationBuilder.DropColumn(
                name: "SurveySessionId1",
                table: "Answers");
        }
    }
}
