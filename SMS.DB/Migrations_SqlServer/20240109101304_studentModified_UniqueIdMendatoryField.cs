using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SMS.DB.Migrations
{
    /// <inheritdoc />
    public partial class studentModified_UniqueIdMendatoryField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Student_UniqueId",
                table: "Student");

            migrationBuilder.AlterColumn<string>(
                name: "UniqueId",
                table: "Student",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Student_UniqueId",
                table: "Student",
                column: "UniqueId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Student_UniqueId",
                table: "Student");

            migrationBuilder.AlterColumn<string>(
                name: "UniqueId",
                table: "Student",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.CreateIndex(
                name: "IX_Student_UniqueId",
                table: "Student",
                column: "UniqueId",
                unique: true,
                filter: "[UniqueId] IS NOT NULL");
        }
    }
}
