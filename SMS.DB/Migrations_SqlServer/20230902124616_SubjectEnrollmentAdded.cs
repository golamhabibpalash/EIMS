using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SMS.DB.Migrations
{
    public partial class SubjectEnrollmentAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExamResultDetails_Student_StudentId",
                table: "ExamResultDetails");

            migrationBuilder.RenameColumn(
                name: "StudentId",
                table: "ExamResultDetails",
                newName: "AcademicSubjectId");

            migrationBuilder.RenameColumn(
                name: "ObtainPoint",
                table: "ExamResultDetails",
                newName: "TotalMark");

            migrationBuilder.RenameColumn(
                name: "ObtainMarks",
                table: "ExamResultDetails",
                newName: "ObtainMark");

            migrationBuilder.RenameColumn(
                name: "ObtainGrade",
                table: "ExamResultDetails",
                newName: "Grade");

            migrationBuilder.RenameColumn(
                name: "Marks",
                table: "ExamResultDetails",
                newName: "GPA");

            migrationBuilder.RenameIndex(
                name: "IX_ExamResultDetails_StudentId",
                table: "ExamResultDetails",
                newName: "IX_ExamResultDetails_AcademicSubjectId");

            migrationBuilder.AddColumn<double>(
                name: "AttendancePercentage",
                table: "ExamResults",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "CGPA",
                table: "ExamResults",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "FinalGrade",
                table: "ExamResults",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GradeComments",
                table: "ExamResults",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MyProperty",
                table: "ExamResults",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "StudentId",
                table: "ExamResults",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalFails",
                table: "ExamResults",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "TotalObtainMarks",
                table: "ExamResults",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.CreateIndex(
                name: "IX_ExamResults_StudentId",
                table: "ExamResults",
                column: "StudentId");

            migrationBuilder.AddForeignKey(
                name: "FK_ExamResultDetails_AcademicSubject_AcademicSubjectId",
                table: "ExamResultDetails",
                column: "AcademicSubjectId",
                principalTable: "AcademicSubject",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_ExamResults_Student_StudentId",
                table: "ExamResults",
                column: "StudentId",
                principalTable: "Student",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExamResultDetails_AcademicSubject_AcademicSubjectId",
                table: "ExamResultDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_ExamResults_Student_StudentId",
                table: "ExamResults");

            migrationBuilder.DropIndex(
                name: "IX_ExamResults_StudentId",
                table: "ExamResults");

            migrationBuilder.DropColumn(
                name: "AttendancePercentage",
                table: "ExamResults");

            migrationBuilder.DropColumn(
                name: "CGPA",
                table: "ExamResults");

            migrationBuilder.DropColumn(
                name: "FinalGrade",
                table: "ExamResults");

            migrationBuilder.DropColumn(
                name: "GradeComments",
                table: "ExamResults");

            migrationBuilder.DropColumn(
                name: "MyProperty",
                table: "ExamResults");

            migrationBuilder.DropColumn(
                name: "StudentId",
                table: "ExamResults");

            migrationBuilder.DropColumn(
                name: "TotalFails",
                table: "ExamResults");

            migrationBuilder.DropColumn(
                name: "TotalObtainMarks",
                table: "ExamResults");

            migrationBuilder.RenameColumn(
                name: "TotalMark",
                table: "ExamResultDetails",
                newName: "ObtainPoint");

            migrationBuilder.RenameColumn(
                name: "ObtainMark",
                table: "ExamResultDetails",
                newName: "ObtainMarks");

            migrationBuilder.RenameColumn(
                name: "Grade",
                table: "ExamResultDetails",
                newName: "ObtainGrade");

            migrationBuilder.RenameColumn(
                name: "GPA",
                table: "ExamResultDetails",
                newName: "Marks");

            migrationBuilder.RenameColumn(
                name: "AcademicSubjectId",
                table: "ExamResultDetails",
                newName: "StudentId");

            migrationBuilder.RenameIndex(
                name: "IX_ExamResultDetails_AcademicSubjectId",
                table: "ExamResultDetails",
                newName: "IX_ExamResultDetails_StudentId");

            migrationBuilder.AddForeignKey(
                name: "FK_ExamResultDetails_Student_StudentId",
                table: "ExamResultDetails",
                column: "StudentId",
                principalTable: "Student",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
