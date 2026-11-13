using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SMS.DB.Migrations
{
    /// <inheritdoc />
    public partial class _classFeeListIdAddedInStudentFeeAllocation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StudentPayment_AcademicSession_AcademicSessionId",
                table: "StudentPayment");

            migrationBuilder.DropIndex(
                name: "IX_StudentPayment_AcademicSessionId",
                table: "StudentPayment");

            migrationBuilder.AddColumn<int>(
                name: "ClassFeeListId",
                table: "StudentFeeAllocations",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudentFeeAllocations_ClassFeeListId",
                table: "StudentFeeAllocations",
                column: "ClassFeeListId");

            migrationBuilder.AddForeignKey(
                name: "FK_StudentFeeAllocations_ClassFeeList_ClassFeeListId",
                table: "StudentFeeAllocations",
                column: "ClassFeeListId",
                principalTable: "ClassFeeList",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StudentFeeAllocations_ClassFeeList_ClassFeeListId",
                table: "StudentFeeAllocations");

            migrationBuilder.DropIndex(
                name: "IX_StudentFeeAllocations_ClassFeeListId",
                table: "StudentFeeAllocations");

            migrationBuilder.DropColumn(
                name: "ClassFeeListId",
                table: "StudentFeeAllocations");

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
    }
}
