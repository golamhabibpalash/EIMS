using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SMS.DB.Migrations
{
    public partial class AcademicExamGroupAdded_RelatedTableModified : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AcademicExams_AcademicExamTypes_AcademicExamTypeId",
                table: "AcademicExams");

            migrationBuilder.DropForeignKey(
                name: "FK_AcademicExams_AcademicSection_AcademicSectionId",
                table: "AcademicExams");

            migrationBuilder.DropForeignKey(
                name: "FK_AcademicExams_AcademicSession_AcademicSessionId",
                table: "AcademicExams");

            migrationBuilder.DropIndex(
                name: "IX_AcademicExams_AcademicExamTypeId",
                table: "AcademicExams");

            migrationBuilder.DropColumn(
                name: "AcademicExamTypeId",
                table: "AcademicExams");

            migrationBuilder.DropColumn(
                name: "ExamName",
                table: "AcademicExams");

            migrationBuilder.RenameColumn(
                name: "MonthId",
                table: "AcademicExams",
                newName: "AcademicExamGroupId");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "AcademicExams",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "AcademicSessionId",
                table: "AcademicExams",
                newName: "AcademicClassId");

            migrationBuilder.RenameIndex(
                name: "IX_AcademicExams_AcademicSessionId",
                table: "AcademicExams",
                newName: "IX_AcademicExams_AcademicClassId");

            migrationBuilder.AlterColumn<int>(
                name: "TotalMarks",
                table: "AcademicExams",
                type: "int",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AlterColumn<int>(
                name: "AcademicSectionId",
                table: "AcademicExams",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "AcademicExamGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExamGroupName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AcademicSessionId = table.Column<int>(type: "int", nullable: false),
                    academicExamTypeId = table.Column<int>(type: "int", nullable: false),
                    ExamMonthId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EditedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EditedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MACAddress = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AcademicExamGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AcademicExamGroups_AcademicExamTypes_academicExamTypeId",
                        column: x => x.academicExamTypeId,
                        principalTable: "AcademicExamTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AcademicExamGroups_AcademicSession_AcademicSessionId",
                        column: x => x.AcademicSessionId,
                        principalTable: "AcademicSession",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AcademicExams_AcademicExamGroupId",
                table: "AcademicExams",
                column: "AcademicExamGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_AcademicExamGroups_academicExamTypeId",
                table: "AcademicExamGroups",
                column: "academicExamTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_AcademicExamGroups_AcademicSessionId",
                table: "AcademicExamGroups",
                column: "AcademicSessionId");

            migrationBuilder.AddForeignKey(
                name: "FK_AcademicExams_AcademicClass_AcademicClassId",
                table: "AcademicExams",
                column: "AcademicClassId",
                principalTable: "AcademicClass",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_AcademicExams_AcademicExamGroups_AcademicExamGroupId",
                table: "AcademicExams",
                column: "AcademicExamGroupId",
                principalTable: "AcademicExamGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AcademicExams_AcademicSection_AcademicSectionId",
                table: "AcademicExams",
                column: "AcademicSectionId",
                principalTable: "AcademicSection",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AcademicExams_AcademicClass_AcademicClassId",
                table: "AcademicExams");

            migrationBuilder.DropForeignKey(
                name: "FK_AcademicExams_AcademicExamGroups_AcademicExamGroupId",
                table: "AcademicExams");

            migrationBuilder.DropForeignKey(
                name: "FK_AcademicExams_AcademicSection_AcademicSectionId",
                table: "AcademicExams");

            migrationBuilder.DropTable(
                name: "AcademicExamGroups");

            migrationBuilder.DropIndex(
                name: "IX_AcademicExams_AcademicExamGroupId",
                table: "AcademicExams");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "AcademicExams",
                newName: "IsActive");

            migrationBuilder.RenameColumn(
                name: "AcademicExamGroupId",
                table: "AcademicExams",
                newName: "MonthId");

            migrationBuilder.RenameColumn(
                name: "AcademicClassId",
                table: "AcademicExams",
                newName: "AcademicSessionId");

            migrationBuilder.RenameIndex(
                name: "IX_AcademicExams_AcademicClassId",
                table: "AcademicExams",
                newName: "IX_AcademicExams_AcademicSessionId");

            migrationBuilder.AlterColumn<double>(
                name: "TotalMarks",
                table: "AcademicExams",
                type: "float",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "AcademicSectionId",
                table: "AcademicExams",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "AcademicExamTypeId",
                table: "AcademicExams",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ExamName",
                table: "AcademicExams",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AcademicExams_AcademicExamTypeId",
                table: "AcademicExams",
                column: "AcademicExamTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_AcademicExams_AcademicExamTypes_AcademicExamTypeId",
                table: "AcademicExams",
                column: "AcademicExamTypeId",
                principalTable: "AcademicExamTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AcademicExams_AcademicSection_AcademicSectionId",
                table: "AcademicExams",
                column: "AcademicSectionId",
                principalTable: "AcademicSection",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AcademicExams_AcademicSession_AcademicSessionId",
                table: "AcademicExams",
                column: "AcademicSessionId",
                principalTable: "AcademicSession",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
