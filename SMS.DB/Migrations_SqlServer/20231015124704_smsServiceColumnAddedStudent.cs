using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SMS.DB.Migrations
{
    /// <inheritdoc />
    public partial class smsServiceColumnAddedStudent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_GradingTableHists_AcademicExamGroupId",
                table: "GradingTableHists");

            migrationBuilder.AddColumn<bool>(
                name: "SMSService",
                table: "Student",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_GradingTableHists_AcademicExamGroupId",
                table: "GradingTableHists",
                column: "AcademicExamGroupId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_GradingTableHists_AcademicExamGroupId",
                table: "GradingTableHists");

            migrationBuilder.DropColumn(
                name: "SMSService",
                table: "Student");

            migrationBuilder.CreateIndex(
                name: "IX_GradingTableHists_AcademicExamGroupId",
                table: "GradingTableHists",
                column: "AcademicExamGroupId");
        }
    }
}
