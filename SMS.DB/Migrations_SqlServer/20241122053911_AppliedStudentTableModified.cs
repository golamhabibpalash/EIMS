using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SMS.DB.Migrations
{
    /// <inheritdoc />
    public partial class AppliedStudentTableModified : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppliedStudent_AcademicSession_AcademicSessionId",
                table: "AppliedStudent");

            migrationBuilder.DropForeignKey(
                name: "FK_AppliedStudent_District_PermanentDistrictId",
                table: "AppliedStudent");

            migrationBuilder.DropForeignKey(
                name: "FK_AppliedStudent_District_PresentDistrictId",
                table: "AppliedStudent");

            migrationBuilder.DropForeignKey(
                name: "FK_AppliedStudent_Gender_GenderId",
                table: "AppliedStudent");

            migrationBuilder.DropForeignKey(
                name: "FK_AppliedStudent_Nationality_NationalityId",
                table: "AppliedStudent");

            migrationBuilder.DropForeignKey(
                name: "FK_AppliedStudent_Religion_ReligionId",
                table: "AppliedStudent");

            migrationBuilder.DropForeignKey(
                name: "FK_AppliedStudent_Upazila_PermanentUpazilaId",
                table: "AppliedStudent");

            migrationBuilder.DropForeignKey(
                name: "FK_AppliedStudent_Upazila_PresentUpazilaId",
                table: "AppliedStudent");

            migrationBuilder.AlterColumn<int>(
                name: "ReligionId",
                table: "AppliedStudent",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "PreviousSchoolClassId",
                table: "AppliedStudent",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "PresentUpazilaId",
                table: "AppliedStudent",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "PresentDistrictId",
                table: "AppliedStudent",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "PermanentUpazilaId",
                table: "AppliedStudent",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "PermanentDistrictId",
                table: "AppliedStudent",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "NationalityId",
                table: "AppliedStudent",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "GenderId",
                table: "AppliedStudent",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DOB",
                table: "AppliedStudent",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<int>(
                name: "AcademicSessionId",
                table: "AppliedStudent",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_AppliedStudent_AcademicSession_AcademicSessionId",
                table: "AppliedStudent",
                column: "AcademicSessionId",
                principalTable: "AcademicSession",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AppliedStudent_District_PermanentDistrictId",
                table: "AppliedStudent",
                column: "PermanentDistrictId",
                principalTable: "District",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AppliedStudent_District_PresentDistrictId",
                table: "AppliedStudent",
                column: "PresentDistrictId",
                principalTable: "District",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AppliedStudent_Gender_GenderId",
                table: "AppliedStudent",
                column: "GenderId",
                principalTable: "Gender",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AppliedStudent_Nationality_NationalityId",
                table: "AppliedStudent",
                column: "NationalityId",
                principalTable: "Nationality",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AppliedStudent_Religion_ReligionId",
                table: "AppliedStudent",
                column: "ReligionId",
                principalTable: "Religion",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AppliedStudent_Upazila_PermanentUpazilaId",
                table: "AppliedStudent",
                column: "PermanentUpazilaId",
                principalTable: "Upazila",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AppliedStudent_Upazila_PresentUpazilaId",
                table: "AppliedStudent",
                column: "PresentUpazilaId",
                principalTable: "Upazila",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppliedStudent_AcademicSession_AcademicSessionId",
                table: "AppliedStudent");

            migrationBuilder.DropForeignKey(
                name: "FK_AppliedStudent_District_PermanentDistrictId",
                table: "AppliedStudent");

            migrationBuilder.DropForeignKey(
                name: "FK_AppliedStudent_District_PresentDistrictId",
                table: "AppliedStudent");

            migrationBuilder.DropForeignKey(
                name: "FK_AppliedStudent_Gender_GenderId",
                table: "AppliedStudent");

            migrationBuilder.DropForeignKey(
                name: "FK_AppliedStudent_Nationality_NationalityId",
                table: "AppliedStudent");

            migrationBuilder.DropForeignKey(
                name: "FK_AppliedStudent_Religion_ReligionId",
                table: "AppliedStudent");

            migrationBuilder.DropForeignKey(
                name: "FK_AppliedStudent_Upazila_PermanentUpazilaId",
                table: "AppliedStudent");

            migrationBuilder.DropForeignKey(
                name: "FK_AppliedStudent_Upazila_PresentUpazilaId",
                table: "AppliedStudent");

            migrationBuilder.AlterColumn<int>(
                name: "ReligionId",
                table: "AppliedStudent",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "PreviousSchoolClassId",
                table: "AppliedStudent",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "PresentUpazilaId",
                table: "AppliedStudent",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "PresentDistrictId",
                table: "AppliedStudent",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "PermanentUpazilaId",
                table: "AppliedStudent",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "PermanentDistrictId",
                table: "AppliedStudent",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "NationalityId",
                table: "AppliedStudent",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "GenderId",
                table: "AppliedStudent",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DOB",
                table: "AppliedStudent",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "AcademicSessionId",
                table: "AppliedStudent",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AppliedStudent_AcademicSession_AcademicSessionId",
                table: "AppliedStudent",
                column: "AcademicSessionId",
                principalTable: "AcademicSession",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AppliedStudent_District_PermanentDistrictId",
                table: "AppliedStudent",
                column: "PermanentDistrictId",
                principalTable: "District",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AppliedStudent_District_PresentDistrictId",
                table: "AppliedStudent",
                column: "PresentDistrictId",
                principalTable: "District",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AppliedStudent_Gender_GenderId",
                table: "AppliedStudent",
                column: "GenderId",
                principalTable: "Gender",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AppliedStudent_Nationality_NationalityId",
                table: "AppliedStudent",
                column: "NationalityId",
                principalTable: "Nationality",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AppliedStudent_Religion_ReligionId",
                table: "AppliedStudent",
                column: "ReligionId",
                principalTable: "Religion",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AppliedStudent_Upazila_PermanentUpazilaId",
                table: "AppliedStudent",
                column: "PermanentUpazilaId",
                principalTable: "Upazila",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AppliedStudent_Upazila_PresentUpazilaId",
                table: "AppliedStudent",
                column: "PresentUpazilaId",
                principalTable: "Upazila",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
