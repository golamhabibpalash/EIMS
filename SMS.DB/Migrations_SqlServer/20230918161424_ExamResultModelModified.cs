using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SMS.DB.Migrations
{
    /// <inheritdoc />
    public partial class ExamResultModelModified : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExamResults_AcademicExams_AcademicExamId",
                table: "ExamResults");

            migrationBuilder.RenameColumn(
                name: "AcademicExamId",
                table: "ExamResults",
                newName: "AcademicExamGroupId");

            migrationBuilder.RenameIndex(
                name: "IX_ExamResults_AcademicExamId",
                table: "ExamResults",
                newName: "IX_ExamResults_AcademicExamGroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_ExamResults_AcademicExamGroups_AcademicExamGroupId",
                table: "ExamResults",
                column: "AcademicExamGroupId",
                principalTable: "AcademicExamGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExamResults_AcademicExamGroups_AcademicExamGroupId",
                table: "ExamResults");

            migrationBuilder.RenameColumn(
                name: "AcademicExamGroupId",
                table: "ExamResults",
                newName: "AcademicExamId");

            migrationBuilder.RenameIndex(
                name: "IX_ExamResults_AcademicExamGroupId",
                table: "ExamResults",
                newName: "IX_ExamResults_AcademicExamId");

            migrationBuilder.AddForeignKey(
                name: "FK_ExamResults_AcademicExams_AcademicExamId",
                table: "ExamResults",
                column: "AcademicExamId",
                principalTable: "AcademicExams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
