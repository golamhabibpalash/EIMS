using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SMS.DB.Migrations
{
    /// <inheritdoc />
    public partial class SLAddedInClassFeeAndStudentFee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SL",
                table: "StudentFeeHead",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SL",
                table: "ClassFeeList",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SL",
                table: "StudentFeeHead");

            migrationBuilder.DropColumn(
                name: "SL",
                table: "ClassFeeList");
        }
    }
}
