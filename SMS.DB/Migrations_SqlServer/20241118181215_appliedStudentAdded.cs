using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace SMS.DB.Migrations
{
    /// <inheritdoc />
    public partial class appliedStudentAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppliedStudent",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NameBangla = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DOB = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FatherName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FatherNameBangla = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FatherNID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FatherOccupation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FatherMonthlyIncome = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FatherPhoneNo = table.Column<string>(type: "nvarchar(11)", maxLength: 11, nullable: true),
                    MotherName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MotherNameBangla = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MotherNID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MotherOccupation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MotherMonthlyIncome = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MotherPhoneNo = table.Column<string>(type: "nvarchar(11)", maxLength: 11, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Photo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BirthCertificateNo = table.Column<string>(type: "nvarchar(17)", maxLength: 17, nullable: true),
                    ReligionId = table.Column<int>(type: "int", nullable: false),
                    GenderId = table.Column<int>(type: "int", nullable: false),
                    BloodGroupId = table.Column<int>(type: "int", nullable: true),
                    NationalityId = table.Column<int>(type: "int", nullable: false),
                    PresentAddressArea = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AddressInfo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PresentAddressPO = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PresentUpazilaId = table.Column<int>(type: "int", nullable: false),
                    PresentDistrictId = table.Column<int>(type: "int", nullable: false),
                    PresentDivisionId = table.Column<int>(type: "int", nullable: true),
                    PermanentAddressArea = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PermanentAddressPO = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PermanentUpazilaId = table.Column<int>(type: "int", nullable: false),
                    PermanentDistrictId = table.Column<int>(type: "int", nullable: false),
                    PermanentDivisionId = table.Column<int>(type: "int", nullable: true),
                    AcademicSessionId = table.Column<int>(type: "int", nullable: false),
                    PreviousSchool = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PreviousSchoolClassId = table.Column<int>(type: "int", nullable: false),
                    InterestedAppliedClassId = table.Column<int>(type: "int", nullable: false),
                    AimInLife = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<bool>(type: "bit", nullable: false),
                    AcademicClassId = table.Column<int>(type: "int", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EditedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EditedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MACAddress = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppliedStudent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppliedStudent_AcademicClass_AcademicClassId",
                        column: x => x.AcademicClassId,
                        principalTable: "AcademicClass",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AppliedStudent_AcademicSession_AcademicSessionId",
                        column: x => x.AcademicSessionId,
                        principalTable: "AcademicSession",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AppliedStudent_BloodGroup_BloodGroupId",
                        column: x => x.BloodGroupId,
                        principalTable: "BloodGroup",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AppliedStudent_District_PermanentDistrictId",
                        column: x => x.PermanentDistrictId,
                        principalTable: "District",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_AppliedStudent_District_PresentDistrictId",
                        column: x => x.PresentDistrictId,
                        principalTable: "District",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_AppliedStudent_Division_PermanentDivisionId",
                        column: x => x.PermanentDivisionId,
                        principalTable: "Division",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AppliedStudent_Division_PresentDivisionId",
                        column: x => x.PresentDivisionId,
                        principalTable: "Division",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AppliedStudent_Gender_GenderId",
                        column: x => x.GenderId,
                        principalTable: "Gender",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AppliedStudent_Nationality_NationalityId",
                        column: x => x.NationalityId,
                        principalTable: "Nationality",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_AppliedStudent_Religion_ReligionId",
                        column: x => x.ReligionId,
                        principalTable: "Religion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_AppliedStudent_Upazila_PermanentUpazilaId",
                        column: x => x.PermanentUpazilaId,
                        principalTable: "Upazila",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_AppliedStudent_Upazila_PresentUpazilaId",
                        column: x => x.PresentUpazilaId,
                        principalTable: "Upazila",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppliedStudent_AcademicClassId",
                table: "AppliedStudent",
                column: "AcademicClassId");

            migrationBuilder.CreateIndex(
                name: "IX_AppliedStudent_AcademicSessionId",
                table: "AppliedStudent",
                column: "AcademicSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_AppliedStudent_BloodGroupId",
                table: "AppliedStudent",
                column: "BloodGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_AppliedStudent_GenderId",
                table: "AppliedStudent",
                column: "GenderId");

            migrationBuilder.CreateIndex(
                name: "IX_AppliedStudent_NationalityId",
                table: "AppliedStudent",
                column: "NationalityId");

            migrationBuilder.CreateIndex(
                name: "IX_AppliedStudent_PermanentDistrictId",
                table: "AppliedStudent",
                column: "PermanentDistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_AppliedStudent_PermanentDivisionId",
                table: "AppliedStudent",
                column: "PermanentDivisionId");

            migrationBuilder.CreateIndex(
                name: "IX_AppliedStudent_PermanentUpazilaId",
                table: "AppliedStudent",
                column: "PermanentUpazilaId");

            migrationBuilder.CreateIndex(
                name: "IX_AppliedStudent_PresentDistrictId",
                table: "AppliedStudent",
                column: "PresentDistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_AppliedStudent_PresentDivisionId",
                table: "AppliedStudent",
                column: "PresentDivisionId");

            migrationBuilder.CreateIndex(
                name: "IX_AppliedStudent_PresentUpazilaId",
                table: "AppliedStudent",
                column: "PresentUpazilaId");

            migrationBuilder.CreateIndex(
                name: "IX_AppliedStudent_ReligionId",
                table: "AppliedStudent",
                column: "ReligionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppliedStudent");
        }
    }
}
