using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SMS.DB.Migrations
{
    public partial class AcademicExamModified : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AcademicExams_AcademicSection_AcademicSectionId",
                table: "AcademicExams");

            migrationBuilder.AlterColumn<int>(
                name: "AcademicSectionId",
                table: "AcademicExams",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_AcademicExams_AcademicSection_AcademicSectionId",
                table: "AcademicExams",
                column: "AcademicSectionId",
                principalTable: "AcademicSection",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AcademicExams_AcademicSection_AcademicSectionId",
                table: "AcademicExams");

            migrationBuilder.AlterColumn<int>(
                name: "AcademicSectionId",
                table: "AcademicExams",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AcademicExams_AcademicSection_AcademicSectionId",
                table: "AcademicExams",
                column: "AcademicSectionId",
                principalTable: "AcademicSection",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
