using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SMS.DB.Migrations
{
    public partial class AcademicExamClassModified : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AcademicExams_AcademicSubject_AcademicSubjectId",
                table: "AcademicExams");

            migrationBuilder.AlterColumn<int>(
                name: "AcademicSubjectId",
                table: "AcademicExams",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_AcademicExams_AcademicSubject_AcademicSubjectId",
                table: "AcademicExams",
                column: "AcademicSubjectId",
                principalTable: "AcademicSubject",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AcademicExams_AcademicSubject_AcademicSubjectId",
                table: "AcademicExams");

            migrationBuilder.AlterColumn<int>(
                name: "AcademicSubjectId",
                table: "AcademicExams",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AcademicExams_AcademicSubject_AcademicSubjectId",
                table: "AcademicExams",
                column: "AcademicSubjectId",
                principalTable: "AcademicSubject",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
