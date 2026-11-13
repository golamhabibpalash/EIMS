using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SMS.DB.Migrations
{
    /// <inheritdoc />
    public partial class GradingTableHistModified : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_GradingTableHists_AcademicExamGroupId",
                table: "GradingTableHists",
                column: "AcademicExamGroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_GradingTableHists_AcademicExamGroups_AcademicExamGroupId",
                table: "GradingTableHists",
                column: "AcademicExamGroupId",
                principalTable: "AcademicExamGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GradingTableHists_AcademicExamGroups_AcademicExamGroupId",
                table: "GradingTableHists");

            migrationBuilder.DropIndex(
                name: "IX_GradingTableHists_AcademicExamGroupId",
                table: "GradingTableHists");
        }
    }
}
