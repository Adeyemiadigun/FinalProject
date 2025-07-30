using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixSubmissionMapping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Submission_User_StudentId1",
                schema: "Clh_Project",
                table: "Submission");

            migrationBuilder.DropIndex(
                name: "IX_Submission_StudentId1",
                schema: "Clh_Project",
                table: "Submission");

            migrationBuilder.DropColumn(
                name: "StudentId1",
                schema: "Clh_Project",
                table: "Submission");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "StudentId1",
                schema: "Clh_Project",
                table: "Submission",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Submission_StudentId1",
                schema: "Clh_Project",
                table: "Submission",
                column: "StudentId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Submission_User_StudentId1",
                schema: "Clh_Project",
                table: "Submission",
                column: "StudentId1",
                principalSchema: "Clh_Project",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
