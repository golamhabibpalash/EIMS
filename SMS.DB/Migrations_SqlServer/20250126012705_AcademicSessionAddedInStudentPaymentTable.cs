using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SMS.DB.Migrations
{
    /// <inheritdoc />
    public partial class AcademicSessionAddedInStudentPaymentTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ClassFeeId",
                table: "StudentPaymentDetails",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "AcademicSessionId",
                table: "StudentPayment",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudentPayment_AcademicSessionId",
                table: "StudentPayment",
                column: "AcademicSessionId");

            migrationBuilder.AddForeignKey(
                name: "FK_StudentPayment_AcademicSession_AcademicSessionId",
                table: "StudentPayment",
                column: "AcademicSessionId",
                principalTable: "AcademicSession",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StudentPayment_AcademicSession_AcademicSessionId",
                table: "StudentPayment");

            migrationBuilder.DropIndex(
                name: "IX_StudentPayment_AcademicSessionId",
                table: "StudentPayment");

            migrationBuilder.DropColumn(
                name: "ClassFeeId",
                table: "StudentPaymentDetails");

            migrationBuilder.DropColumn(
                name: "AcademicSessionId",
                table: "StudentPayment");
        }
    }
}
