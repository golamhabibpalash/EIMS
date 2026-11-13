using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SMS.DB.Migrations
{
    /// <inheritdoc />
    public partial class PreviouisPaymentDetailsDto_aded_for_sql : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AcademicExamGroups_AcademicExamTypes_academicExamTypeId",
                table: "AcademicExamGroups");

            migrationBuilder.RenameColumn(
                name: "academicExamTypeId",
                table: "AcademicExamGroups",
                newName: "AcademicExamTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_AcademicExamGroups_academicExamTypeId",
                table: "AcademicExamGroups",
                newName: "IX_AcademicExamGroups_AcademicExamTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_AcademicExamGroups_AcademicExamTypes_AcademicExamTypeId",
                table: "AcademicExamGroups",
                column: "AcademicExamTypeId",
                principalTable: "AcademicExamTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AcademicExamGroups_AcademicExamTypes_AcademicExamTypeId",
                table: "AcademicExamGroups");

            migrationBuilder.DropTable(
                name: "Logs");

            migrationBuilder.RenameColumn(
                name: "AcademicExamTypeId",
                table: "AcademicExamGroups",
                newName: "academicExamTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_AcademicExamGroups_AcademicExamTypeId",
                table: "AcademicExamGroups",
                newName: "IX_AcademicExamGroups_academicExamTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_AcademicExamGroups_AcademicExamTypes_academicExamTypeId",
                table: "AcademicExamGroups",
                column: "academicExamTypeId",
                principalTable: "AcademicExamTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
