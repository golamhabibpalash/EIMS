using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SMS.DB.Migrations
{
    public partial class AcademicSectionAddedToAcademicExam : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AcademicSectionId",
                table: "AcademicExams",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_AcademicExams_AcademicSectionId",
                table: "AcademicExams",
                column: "AcademicSectionId");

            migrationBuilder.AddForeignKey(
                name: "FK_AcademicExams_AcademicSection_AcademicSectionId",
                table: "AcademicExams",
                column: "AcademicSectionId",
                principalTable: "AcademicSection",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AcademicExams_AcademicSection_AcademicSectionId",
                table: "AcademicExams");

            migrationBuilder.DropIndex(
                name: "IX_AcademicExams_AcademicSectionId",
                table: "AcademicExams");


            migrationBuilder.DropColumn(
                name: "AcademicSectionId",
                table: "AcademicExams");
        }
    }
}
