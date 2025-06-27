using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Clh_Project");

            migrationBuilder.CreateTable(
                name: "User",
                schema: "Clh_Project",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FullName = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Assessment",
                schema: "Clh_Project",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    TechnologyStack = table.Column<string>(type: "text", nullable: false),
                    DurationInMinutes = table.Column<int>(type: "integer", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    InstructorId = table.Column<Guid>(type: "uuid", nullable: false),
                    InstructorId1 = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PassingScore = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assessment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Assessment_User_InstructorId",
                        column: x => x.InstructorId,
                        principalSchema: "Clh_Project",
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Assessment_User_InstructorId1",
                        column: x => x.InstructorId1,
                        principalSchema: "Clh_Project",
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AssessmentAssignment",
                schema: "Clh_Project",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AssessmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EmailSent = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssessmentAssignment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssessmentAssignment_Assessment_AssessmentId",
                        column: x => x.AssessmentId,
                        principalSchema: "Clh_Project",
                        principalTable: "Assessment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AssessmentAssignment_User_StudentId",
                        column: x => x.StudentId,
                        principalSchema: "Clh_Project",
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Question",
                schema: "Clh_Project",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AssessmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionText = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: false),
                    QuestionType = table.Column<int>(type: "integer", nullable: false),
                    Marks = table.Column<short>(type: "smallint", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    TechnologyStack = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Question", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Question_Assessment_AssessmentId",
                        column: x => x.AssessmentId,
                        principalSchema: "Clh_Project",
                        principalTable: "Assessment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Submission",
                schema: "Clh_Project",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AssessmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentId1 = table.Column<Guid>(type: "uuid", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TotalScore = table.Column<short>(type: "smallint", nullable: false),
                    FeedBack = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Submission", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Submission_Assessment_AssessmentId",
                        column: x => x.AssessmentId,
                        principalSchema: "Clh_Project",
                        principalTable: "Assessment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Submission_User_StudentId",
                        column: x => x.StudentId,
                        principalSchema: "Clh_Project",
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Submission_User_StudentId1",
                        column: x => x.StudentId1,
                        principalSchema: "Clh_Project",
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Answer",
                schema: "Clh_Project",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uuid", nullable: false),
                    AnswerText = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Answer", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Answer_Question_QuestionId",
                        column: x => x.QuestionId,
                        principalSchema: "Clh_Project",
                        principalTable: "Question",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Option",
                schema: "Clh_Project",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uuid", nullable: false),
                    OptionText = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    IsCorrect = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Option", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Option_Question_QuestionId",
                        column: x => x.QuestionId,
                        principalSchema: "Clh_Project",
                        principalTable: "Question",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TestCase",
                schema: "Clh_Project",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Input = table.Column<string>(type: "text", nullable: false),
                    ExpectedOutput = table.Column<string>(type: "text", nullable: false),
                    Weight = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestCase", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TestCase_Question_QuestionId",
                        column: x => x.QuestionId,
                        principalSchema: "Clh_Project",
                        principalTable: "Question",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AnswerSubmission",
                schema: "Clh_Project",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SubmissionId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubmittedAnswer = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Score = table.Column<short>(type: "smallint", nullable: false),
                    IsCorrect = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnswerSubmission", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AnswerSubmission_Question_QuestionId",
                        column: x => x.QuestionId,
                        principalSchema: "Clh_Project",
                        principalTable: "Question",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AnswerSubmission_Submission_SubmissionId",
                        column: x => x.SubmissionId,
                        principalSchema: "Clh_Project",
                        principalTable: "Submission",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SelectedOptions",
                schema: "Clh_Project",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AnswerSubmissionId = table.Column<Guid>(type: "uuid", nullable: false),
                    OptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    OptionId1 = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SelectedOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SelectedOptions_AnswerSubmission_AnswerSubmissionId",
                        column: x => x.AnswerSubmissionId,
                        principalSchema: "Clh_Project",
                        principalTable: "AnswerSubmission",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SelectedOptions_Option_OptionId",
                        column: x => x.OptionId,
                        principalSchema: "Clh_Project",
                        principalTable: "Option",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SelectedOptions_Option_OptionId1",
                        column: x => x.OptionId1,
                        principalSchema: "Clh_Project",
                        principalTable: "Option",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TestCaseResults",
                schema: "Clh_Project",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AnswerSubmissionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Input = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: false),
                    ExpectedOutput = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: false),
                    ActualOutput = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: false),
                    Passed = table.Column<bool>(type: "boolean", nullable: false),
                    EarnedWeight = table.Column<double>(type: "double precision", precision: 5, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestCaseResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TestCaseResults_AnswerSubmission_AnswerSubmissionId",
                        column: x => x.AnswerSubmissionId,
                        principalSchema: "Clh_Project",
                        principalTable: "AnswerSubmission",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Answer_QuestionId",
                schema: "Clh_Project",
                table: "Answer",
                column: "QuestionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AnswerSubmission_QuestionId",
                schema: "Clh_Project",
                table: "AnswerSubmission",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_AnswerSubmission_SubmissionId",
                schema: "Clh_Project",
                table: "AnswerSubmission",
                column: "SubmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_Assessment_InstructorId",
                schema: "Clh_Project",
                table: "Assessment",
                column: "InstructorId");

            migrationBuilder.CreateIndex(
                name: "IX_Assessment_InstructorId1",
                schema: "Clh_Project",
                table: "Assessment",
                column: "InstructorId1");

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentAssignment_AssessmentId",
                schema: "Clh_Project",
                table: "AssessmentAssignment",
                column: "AssessmentId");

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentAssignment_StudentId",
                schema: "Clh_Project",
                table: "AssessmentAssignment",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_Option_QuestionId",
                schema: "Clh_Project",
                table: "Option",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_Question_AssessmentId",
                schema: "Clh_Project",
                table: "Question",
                column: "AssessmentId");

            migrationBuilder.CreateIndex(
                name: "IX_SelectedOptions_AnswerSubmissionId",
                schema: "Clh_Project",
                table: "SelectedOptions",
                column: "AnswerSubmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_SelectedOptions_OptionId",
                schema: "Clh_Project",
                table: "SelectedOptions",
                column: "OptionId");

            migrationBuilder.CreateIndex(
                name: "IX_SelectedOptions_OptionId1",
                schema: "Clh_Project",
                table: "SelectedOptions",
                column: "OptionId1");

            migrationBuilder.CreateIndex(
                name: "IX_Submission_AssessmentId",
                schema: "Clh_Project",
                table: "Submission",
                column: "AssessmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Submission_StudentId",
                schema: "Clh_Project",
                table: "Submission",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_Submission_StudentId1",
                schema: "Clh_Project",
                table: "Submission",
                column: "StudentId1");

            migrationBuilder.CreateIndex(
                name: "IX_TestCase_QuestionId",
                schema: "Clh_Project",
                table: "TestCase",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_TestCaseResults_AnswerSubmissionId",
                schema: "Clh_Project",
                table: "TestCaseResults",
                column: "AnswerSubmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_User_Email",
                schema: "Clh_Project",
                table: "User",
                column: "Email");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Answer",
                schema: "Clh_Project");

            migrationBuilder.DropTable(
                name: "AssessmentAssignment",
                schema: "Clh_Project");

            migrationBuilder.DropTable(
                name: "SelectedOptions",
                schema: "Clh_Project");

            migrationBuilder.DropTable(
                name: "TestCase",
                schema: "Clh_Project");

            migrationBuilder.DropTable(
                name: "TestCaseResults",
                schema: "Clh_Project");

            migrationBuilder.DropTable(
                name: "Option",
                schema: "Clh_Project");

            migrationBuilder.DropTable(
                name: "AnswerSubmission",
                schema: "Clh_Project");

            migrationBuilder.DropTable(
                name: "Question",
                schema: "Clh_Project");

            migrationBuilder.DropTable(
                name: "Submission",
                schema: "Clh_Project");

            migrationBuilder.DropTable(
                name: "Assessment",
                schema: "Clh_Project");

            migrationBuilder.DropTable(
                name: "User",
                schema: "Clh_Project");
        }
    }
}
