using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SMS.DB.Migrations
{
    /// <inheritdoc />
    public partial class addCategoryToAcademicExamAndRemoveFromExamDetalis : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExamCategory",
                table: "AcademicExamDetails");

            migrationBuilder.AddColumn<string>(
                name: "ExamCategory",
                table: "AcademicExams",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExamCategory",
                table: "AcademicExams");

            migrationBuilder.AddColumn<string>(
                name: "ExamCategory",
                table: "AcademicExamDetails",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
