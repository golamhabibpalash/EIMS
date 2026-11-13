using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SMS.DB.Migrations
{
    public partial class SubjectEnrollmentWithDettailsAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SubjectEnrollments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EditedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EditedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MACAddress = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubjectEnrollments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubjectEnrollments_Student_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Student",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SubjectEnrollmentDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubjectEnrollmentId = table.Column<int>(type: "int", nullable: false),
                    SubjectId = table.Column<int>(type: "int", nullable: false),
                    AcademicSubjectId = table.Column<int>(type: "int", nullable: true),
                    AcademicSubjectTypeId = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EditedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EditedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MACAddress = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubjectEnrollmentDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubjectEnrollmentDetails_AcademicSubject_AcademicSubjectId",
                        column: x => x.AcademicSubjectId,
                        principalTable: "AcademicSubject",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SubjectEnrollmentDetails_AcademicSubjectType_AcademicSubjectTypeId",
                        column: x => x.AcademicSubjectTypeId,
                        principalTable: "AcademicSubjectType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SubjectEnrollmentDetails_SubjectEnrollments_SubjectEnrollmentId",
                        column: x => x.SubjectEnrollmentId,
                        principalTable: "SubjectEnrollments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SubjectEnrollmentDetails_AcademicSubjectId",
                table: "SubjectEnrollmentDetails",
                column: "AcademicSubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_SubjectEnrollmentDetails_AcademicSubjectTypeId",
                table: "SubjectEnrollmentDetails",
                column: "AcademicSubjectTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_SubjectEnrollmentDetails_SubjectEnrollmentId",
                table: "SubjectEnrollmentDetails",
                column: "SubjectEnrollmentId");

            migrationBuilder.CreateIndex(
                name: "IX_SubjectEnrollments_StudentId",
                table: "SubjectEnrollments",
                column: "StudentId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SubjectEnrollmentDetails");

            migrationBuilder.DropTable(
                name: "SubjectEnrollments");
        }
    }
}
