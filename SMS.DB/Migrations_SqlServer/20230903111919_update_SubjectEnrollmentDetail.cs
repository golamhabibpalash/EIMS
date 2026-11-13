using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SMS.DB.Migrations
{
    public partial class update_SubjectEnrollmentDetail : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SubjectEnrollmentDetails_AcademicSubject_AcademicSubjectId",
                table: "SubjectEnrollmentDetails");

            migrationBuilder.DropColumn(
                name: "SubjectId",
                table: "SubjectEnrollmentDetails");

            migrationBuilder.AlterColumn<int>(
                name: "AcademicSubjectId",
                table: "SubjectEnrollmentDetails",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_SubjectEnrollmentDetails_AcademicSubject_AcademicSubjectId",
                table: "SubjectEnrollmentDetails",
                column: "AcademicSubjectId",
                principalTable: "AcademicSubject",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SubjectEnrollmentDetails_AcademicSubject_AcademicSubjectId",
                table: "SubjectEnrollmentDetails");

            migrationBuilder.AlterColumn<int>(
                name: "AcademicSubjectId",
                table: "SubjectEnrollmentDetails",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "SubjectId",
                table: "SubjectEnrollmentDetails",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_SubjectEnrollmentDetails_AcademicSubject_AcademicSubjectId",
                table: "SubjectEnrollmentDetails",
                column: "AcademicSubjectId",
                principalTable: "AcademicSubject",
                principalColumn: "Id");
        }
    }
}
