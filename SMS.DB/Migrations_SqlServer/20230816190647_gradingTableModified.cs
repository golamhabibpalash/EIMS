using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SMS.DB.Migrations
{
    public partial class gradingTableModified : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "gradeComments",
                table: "GradingTables",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ObtainGrade",
                table: "ExamResultDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ObtainMarks",
                table: "ExamResultDetails",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "ObtainPoint",
                table: "ExamResultDetails",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "gradeComments",
                table: "GradingTables");

            migrationBuilder.DropColumn(
                name: "ObtainGrade",
                table: "ExamResultDetails");

            migrationBuilder.DropColumn(
                name: "ObtainMarks",
                table: "ExamResultDetails");

            migrationBuilder.DropColumn(
                name: "ObtainPoint",
                table: "ExamResultDetails");
        }
    }
}
