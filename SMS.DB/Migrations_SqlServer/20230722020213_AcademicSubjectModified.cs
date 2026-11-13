using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SMS.DB.Migrations
{
    public partial class AcademicSubjectModified : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AcademicSubject_AcademicClass_AcademicClassId",
                table: "AcademicSubject");

            migrationBuilder.DropColumn(
                name: "QuestionImage",
                table: "AcademicExamDetails");

            migrationBuilder.AlterColumn<int>(
                name: "AcademicClassId",
                table: "AcademicSubject",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_AcademicSubject_AcademicClass_AcademicClassId",
                table: "AcademicSubject",
                column: "AcademicClassId",
                principalTable: "AcademicClass",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AcademicSubject_AcademicClass_AcademicClassId",
                table: "AcademicSubject");

            migrationBuilder.AlterColumn<int>(
                name: "AcademicClassId",
                table: "AcademicSubject",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QuestionImage",
                table: "AcademicExamDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AcademicSubject_AcademicClass_AcademicClassId",
                table: "AcademicSubject",
                column: "AcademicClassId",
                principalTable: "AcademicClass",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
