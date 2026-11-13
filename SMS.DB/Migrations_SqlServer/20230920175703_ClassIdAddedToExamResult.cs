using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SMS.DB.Migrations
{
    /// <inheritdoc />
    public partial class ClassIdAddedToExamResult : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AcademicClassId",
                table: "ExamResults",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ExamResults_AcademicClassId",
                table: "ExamResults",
                column: "AcademicClassId");

            migrationBuilder.AddForeignKey(
                name: "FK_ExamResults_AcademicClass_AcademicClassId",
                table: "ExamResults",
                column: "AcademicClassId",
                principalTable: "AcademicClass",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExamResults_AcademicClass_AcademicClassId",
                table: "ExamResults");

            migrationBuilder.DropIndex(
                name: "IX_ExamResults_AcademicClassId",
                table: "ExamResults");

            migrationBuilder.DropColumn(
                name: "AcademicClassId",
                table: "ExamResults");
        }
    }
}
