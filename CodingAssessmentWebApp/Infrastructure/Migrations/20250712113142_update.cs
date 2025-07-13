using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class update : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "BatchId",
                schema: "Clh_Project",
                table: "User",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Batch",
                schema: "Clh_Project",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    BatchNumber = table.Column<short>(type: "smallint", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Batch", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StudentAssessmentProgresses",
                schema: "Clh_Project",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssessmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastSavedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ElapsedTime = table.Column<TimeSpan>(type: "interval", nullable: false),
                    CurrentSessionStart = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentAssessmentProgresses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BatchAssessment",
                schema: "Clh_Project",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AssessmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    BatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BatchAssessment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BatchAssessment_Assessment_AssessmentId",
                        column: x => x.AssessmentId,
                        principalSchema: "Clh_Project",
                        principalTable: "Assessment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BatchAssessment_Batch_BatchId",
                        column: x => x.BatchId,
                        principalSchema: "Clh_Project",
                        principalTable: "Batch",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InProgressAnswers",
                schema: "Clh_Project",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uuid", nullable: false),
                    AnswerText = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: true),
                    StudentAssessmentProgressId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InProgressAnswers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InProgressAnswers_StudentAssessmentProgresses_StudentAssess~",
                        column: x => x.StudentAssessmentProgressId,
                        principalSchema: "Clh_Project",
                        principalTable: "StudentAssessmentProgresses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InProgressSelectedOptions",
                schema: "Clh_Project",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    InProgressAnswerId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InProgressSelectedOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InProgressSelectedOptions_InProgressAnswers_InProgressAnswe~",
                        column: x => x.InProgressAnswerId,
                        principalSchema: "Clh_Project",
                        principalTable: "InProgressAnswers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_User_BatchId",
                schema: "Clh_Project",
                table: "User",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_BatchAssessment_AssessmentId",
                schema: "Clh_Project",
                table: "BatchAssessment",
                column: "AssessmentId");

            migrationBuilder.CreateIndex(
                name: "IX_BatchAssessment_BatchId",
                schema: "Clh_Project",
                table: "BatchAssessment",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_InProgressAnswers_StudentAssessmentProgressId",
                schema: "Clh_Project",
                table: "InProgressAnswers",
                column: "StudentAssessmentProgressId");

            migrationBuilder.CreateIndex(
                name: "IX_InProgressSelectedOptions_InProgressAnswerId",
                schema: "Clh_Project",
                table: "InProgressSelectedOptions",
                column: "InProgressAnswerId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentAssessmentProgresses_StudentId_AssessmentId",
                schema: "Clh_Project",
                table: "StudentAssessmentProgresses",
                columns: new[] { "StudentId", "AssessmentId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_User_Batch_BatchId",
                schema: "Clh_Project",
                table: "User",
                column: "BatchId",
                principalSchema: "Clh_Project",
                principalTable: "Batch",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_User_Batch_BatchId",
                schema: "Clh_Project",
                table: "User");

            migrationBuilder.DropTable(
                name: "BatchAssessment",
                schema: "Clh_Project");

            migrationBuilder.DropTable(
                name: "InProgressSelectedOptions",
                schema: "Clh_Project");

            migrationBuilder.DropTable(
                name: "Batch",
                schema: "Clh_Project");

            migrationBuilder.DropTable(
                name: "InProgressAnswers",
                schema: "Clh_Project");

            migrationBuilder.DropTable(
                name: "StudentAssessmentProgresses",
                schema: "Clh_Project");

            migrationBuilder.DropIndex(
                name: "IX_User_BatchId",
                schema: "Clh_Project",
                table: "User");

            migrationBuilder.DropColumn(
                name: "BatchId",
                schema: "Clh_Project",
                table: "User");
        }
    }
}
