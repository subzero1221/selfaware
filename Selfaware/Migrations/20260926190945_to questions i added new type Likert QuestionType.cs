using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Selfaware.Migrations
{
    /// <inheritdoc />
    public partial class toquestionsiaddednewtypeLikertQuestionType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SubscaleId",
                table: "Questions",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Subscale",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    QuizId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subscale", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Subscale_Quizzes_QuizId",
                        column: x => x.QuizId,
                        principalTable: "Quizzes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Questions_SubscaleId",
                table: "Questions",
                column: "SubscaleId");

            migrationBuilder.CreateIndex(
                name: "IX_Subscale_QuizId",
                table: "Subscale",
                column: "QuizId");

            migrationBuilder.AddForeignKey(
                name: "FK_Questions_Subscale_SubscaleId",
                table: "Questions",
                column: "SubscaleId",
                principalTable: "Subscale",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Questions_Subscale_SubscaleId",
                table: "Questions");

            migrationBuilder.DropTable(
                name: "Subscale");

            migrationBuilder.DropIndex(
                name: "IX_Questions_SubscaleId",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "SubscaleId",
                table: "Questions");
        }
    }
}
