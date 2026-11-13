using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SMS.DB.Migrations
{
    public partial class AcademicResultModified : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AcademicExamDetails_ExamResults_ExamResultId",
                table: "AcademicExamDetails");

            migrationBuilder.DropIndex(
                name: "IX_AcademicExamDetails_ExamResultId",
                table: "AcademicExamDetails");

            migrationBuilder.DropColumn(
                name: "MyProperty",
                table: "ExamResults");

            migrationBuilder.DropColumn(
                name: "ExamResultId",
                table: "AcademicExamDetails");

            migrationBuilder.AlterColumn<string>(
                name: "FinalGrade",
                table: "ExamResults",
                type: "nvarchar(2)",
                maxLength: 2,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "FinalGrade",
                table: "ExamResults",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(2)",
                oldMaxLength: 2,
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MyProperty",
                table: "ExamResults",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ExamResultId",
                table: "AcademicExamDetails",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AcademicExamDetails_ExamResultId",
                table: "AcademicExamDetails",
                column: "ExamResultId");

            migrationBuilder.AddForeignKey(
                name: "FK_AcademicExamDetails_ExamResults_ExamResultId",
                table: "AcademicExamDetails",
                column: "ExamResultId",
                principalTable: "ExamResults",
                principalColumn: "Id");
        }
    }
}
