using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SMS.DB.Migrations
{
    /// <inheritdoc />
    public partial class AddAttendanceMachineAndFingerprintFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AcademicClassSubjects_AcademicClass_AcademicClassId",
                table: "AcademicClassSubjects");

            migrationBuilder.DropForeignKey(
                name: "FK_AcademicClassSubjects_AcademicSubject_AcademicSubjectId",
                table: "AcademicClassSubjects");

            migrationBuilder.DropForeignKey(
                name: "FK_AcademicExamDetails_AcademicExams_AcademicExamId",
                table: "AcademicExamDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_AcademicExamDetails_Student_StudentId",
                table: "AcademicExamDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_AcademicExamGroups_AcademicExamTypes_AcademicExamTypeId",
                table: "AcademicExamGroups");

            migrationBuilder.DropForeignKey(
                name: "FK_AcademicExamGroups_AcademicSession_AcademicSessionId",
                table: "AcademicExamGroups");

            migrationBuilder.DropForeignKey(
                name: "FK_AcademicExams_AcademicClass_AcademicClassId",
                table: "AcademicExams");

            migrationBuilder.DropForeignKey(
                name: "FK_AcademicExams_AcademicExamGroups_AcademicExamGroupId",
                table: "AcademicExams");

            migrationBuilder.DropForeignKey(
                name: "FK_AcademicExams_AcademicSubject_AcademicSubjectId",
                table: "AcademicExams");

            migrationBuilder.DropForeignKey(
                name: "FK_AcademicExams_Employee_EmployeeId",
                table: "AcademicExams");

            migrationBuilder.DropForeignKey(
                name: "FK_AcademicSection_AcademicClass_AcademicClassId",
                table: "AcademicSection");

            migrationBuilder.DropForeignKey(
                name: "FK_AcademicSection_AcademicSession_AcademicSessionId",
                table: "AcademicSection");

            migrationBuilder.DropForeignKey(
                name: "FK_AcademicSubject_AcademicSubjectType_AcademicSubjectTypeId",
                table: "AcademicSubject");

            migrationBuilder.DropForeignKey(
                name: "FK_AppliedStudent_District_PermanentDistrictId",
                table: "AppliedStudent");

            migrationBuilder.DropForeignKey(
                name: "FK_AppliedStudent_District_PresentDistrictId",
                table: "AppliedStudent");

            migrationBuilder.DropForeignKey(
                name: "FK_AppliedStudent_Division_PermanentDivisionId",
                table: "AppliedStudent");

            migrationBuilder.DropForeignKey(
                name: "FK_AppliedStudent_Division_PresentDivisionId",
                table: "AppliedStudent");

            migrationBuilder.DropForeignKey(
                name: "FK_AppliedStudent_Upazila_PermanentUpazilaId",
                table: "AppliedStudent");

            migrationBuilder.DropForeignKey(
                name: "FK_AppliedStudent_Upazila_PresentUpazilaId",
                table: "AppliedStudent");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                table: "AspNetRoleClaims");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                table: "AspNetUserClaims");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                table: "AspNetUserLogins");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                table: "AspNetUserRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                table: "AspNetUserRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                table: "AspNetUserTokens");

            migrationBuilder.DropForeignKey(
                name: "FK_AttachDocs_AttachDocType_AttachDocTypeId",
                table: "AttachDocs");

            migrationBuilder.DropForeignKey(
                name: "FK_Chapters_AcademicSubject_AcademicSubjectId",
                table: "Chapters");

            migrationBuilder.DropForeignKey(
                name: "FK_ClaimStores_ProjectSubModules_SubModuleId",
                table: "ClaimStores");

            migrationBuilder.DropForeignKey(
                name: "FK_ClassFeeList_AcademicClass_AcademicClassId",
                table: "ClassFeeList");

            migrationBuilder.DropForeignKey(
                name: "FK_ClassFeeList_AcademicSession_AcademicSessionId",
                table: "ClassFeeList");

            migrationBuilder.DropForeignKey(
                name: "FK_ClassFeeList_StudentFeeHead_StudentFeeHeadId",
                table: "ClassFeeList");

            migrationBuilder.DropForeignKey(
                name: "FK_ClassRoutines_AcademicClass_AcademicClassId",
                table: "ClassRoutines");

            migrationBuilder.DropForeignKey(
                name: "FK_ClassRoutines_AcademicSubject_AcademicSubjectId",
                table: "ClassRoutines");

            migrationBuilder.DropForeignKey(
                name: "FK_ClassRoutines_ClassRooms_ClassRoomId",
                table: "ClassRoutines");

            migrationBuilder.DropForeignKey(
                name: "FK_ClassRoutines_Days_DaysId",
                table: "ClassRoutines");

            migrationBuilder.DropForeignKey(
                name: "FK_ClassRoutines_Employee_EmployeeId",
                table: "ClassRoutines");

            migrationBuilder.DropForeignKey(
                name: "FK_Designation_DesignationType_DesignationTypeId",
                table: "Designation");

            migrationBuilder.DropForeignKey(
                name: "FK_District_Division_DivisionId",
                table: "District");

            migrationBuilder.DropForeignKey(
                name: "FK_Employee_BloodGroup_BloodGroupId",
                table: "Employee");

            migrationBuilder.DropForeignKey(
                name: "FK_Employee_Designation_DesignationId",
                table: "Employee");

            migrationBuilder.DropForeignKey(
                name: "FK_Employee_District_PermanentDistrictId",
                table: "Employee");

            migrationBuilder.DropForeignKey(
                name: "FK_Employee_District_PresentDistrictId",
                table: "Employee");

            migrationBuilder.DropForeignKey(
                name: "FK_Employee_Division_PermanentDivisionId",
                table: "Employee");

            migrationBuilder.DropForeignKey(
                name: "FK_Employee_Division_PresentDivisionId",
                table: "Employee");

            migrationBuilder.DropForeignKey(
                name: "FK_Employee_EmpType_EmpTypeId",
                table: "Employee");

            migrationBuilder.DropForeignKey(
                name: "FK_Employee_Gender_GenderId",
                table: "Employee");

            migrationBuilder.DropForeignKey(
                name: "FK_Employee_Nationality_NationalityId",
                table: "Employee");

            migrationBuilder.DropForeignKey(
                name: "FK_Employee_Religion_ReligionId",
                table: "Employee");

            migrationBuilder.DropForeignKey(
                name: "FK_Employee_Upazila_PermanentUpazilaId",
                table: "Employee");

            migrationBuilder.DropForeignKey(
                name: "FK_Employee_Upazila_PresentUpazilaId",
                table: "Employee");

            migrationBuilder.DropForeignKey(
                name: "FK_ExamResultDetails_AcademicSubject_AcademicSubjectId",
                table: "ExamResultDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_ExamResultDetails_ExamResults_ExamResultId",
                table: "ExamResultDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_ExamResults_AcademicClass_AcademicClassId",
                table: "ExamResults");

            migrationBuilder.DropForeignKey(
                name: "FK_ExamResults_AcademicExamGroups_AcademicExamGroupId",
                table: "ExamResults");

            migrationBuilder.DropForeignKey(
                name: "FK_ExamResults_Student_StudentId",
                table: "ExamResults");

            migrationBuilder.DropForeignKey(
                name: "FK_GradingTableHists_AcademicExamGroups_AcademicExamGroupId",
                table: "GradingTableHists");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_NotificationEvents_NotificationEventId",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_OffDays_OffDayTypes_OffDayTypeId",
                table: "OffDays");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectSubModules_ProjectModules_ProjectModuleId",
                table: "ProjectSubModules");

            migrationBuilder.DropForeignKey(
                name: "FK_QuestionDetails_Questions_QuestionId",
                table: "QuestionDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_Questions_Chapters_ChapterId",
                table: "Questions");

            migrationBuilder.DropForeignKey(
                name: "FK_Student_AcademicClass_AcademicClassId",
                table: "Student");

            migrationBuilder.DropForeignKey(
                name: "FK_Student_AcademicSession_AcademicSessionId",
                table: "Student");

            migrationBuilder.DropForeignKey(
                name: "FK_Student_District_PermanentDistrictId",
                table: "Student");

            migrationBuilder.DropForeignKey(
                name: "FK_Student_District_PresentDistrictId",
                table: "Student");

            migrationBuilder.DropForeignKey(
                name: "FK_Student_Division_PermanentDivisionId",
                table: "Student");

            migrationBuilder.DropForeignKey(
                name: "FK_Student_Division_PresentDivisionId",
                table: "Student");

            migrationBuilder.DropForeignKey(
                name: "FK_Student_Gender_GenderId",
                table: "Student");

            migrationBuilder.DropForeignKey(
                name: "FK_Student_Nationality_NationalityId",
                table: "Student");

            migrationBuilder.DropForeignKey(
                name: "FK_Student_Religion_ReligionId",
                table: "Student");

            migrationBuilder.DropForeignKey(
                name: "FK_Student_Upazila_PermanentUpazilaId",
                table: "Student");

            migrationBuilder.DropForeignKey(
                name: "FK_Student_Upazila_PresentUpazilaId",
                table: "Student");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentFeeAllocations_StudentFeeHead_StudentFeeHeadId",
                table: "StudentFeeAllocations");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentFeeAllocations_Student_StudentId",
                table: "StudentFeeAllocations");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentPayment_Student_StudentId",
                table: "StudentPayment");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentPaymentDetails_StudentFeeHead_StudentFeeHeadId",
                table: "StudentPaymentDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentPaymentDetails_StudentPayment_StudentPaymentId",
                table: "StudentPaymentDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_SubjectEnrollmentDetails_AcademicSubjectType_AcademicSubjectTypeId",
                table: "SubjectEnrollmentDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_SubjectEnrollmentDetails_AcademicSubject_AcademicSubjectId",
                table: "SubjectEnrollmentDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_SubjectEnrollmentDetails_SubjectEnrollments_SubjectEnrollmentId",
                table: "SubjectEnrollmentDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_SubjectEnrollments_Student_StudentId",
                table: "SubjectEnrollments");

            migrationBuilder.DropForeignKey(
                name: "FK_TeacherSubjectMaps_AcademicClassSubjects_AcademicClassSubjectId",
                table: "TeacherSubjectMaps");

            migrationBuilder.DropForeignKey(
                name: "FK_Upazila_District_DistrictId",
                table: "Upazila");

            migrationBuilder.AlterColumn<string>(
                name: "MachineNo",
                table: "Tran_MachineRawPunch",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(5)",
                oldMaxLength: 5,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CardNo",
                table: "Tran_MachineRawPunch",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10,
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsSynced",
                table: "Tran_MachineRawPunch",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "MachineSerialNo",
                table: "Tran_MachineRawPunch",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VerifyMode",
                table: "Tran_MachineRawPunch",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MachineUserId",
                table: "Student",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MachineUserId",
                table: "Employee",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AttendanceMachines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IPAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Port = table.Column<int>(type: "int", nullable: false),
                    SerialNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Model = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Brand = table.Column<int>(type: "int", nullable: false),
                    Location = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    UsePushMode = table.Column<bool>(type: "bit", nullable: false),
                    LastSyncAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastError = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PushEndpoint = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Password = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EditedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EditedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MACAddress = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttendanceMachines", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_AcademicClassSubjects_AcademicClass_AcademicClassId",
                table: "AcademicClassSubjects",
                column: "AcademicClassId",
                principalTable: "AcademicClass",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AcademicClassSubjects_AcademicSubject_AcademicSubjectId",
                table: "AcademicClassSubjects",
                column: "AcademicSubjectId",
                principalTable: "AcademicSubject",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AcademicExamDetails_AcademicExams_AcademicExamId",
                table: "AcademicExamDetails",
                column: "AcademicExamId",
                principalTable: "AcademicExams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AcademicExamDetails_Student_StudentId",
                table: "AcademicExamDetails",
                column: "StudentId",
                principalTable: "Student",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AcademicExamGroups_AcademicExamTypes_AcademicExamTypeId",
                table: "AcademicExamGroups",
                column: "AcademicExamTypeId",
                principalTable: "AcademicExamTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AcademicExamGroups_AcademicSession_AcademicSessionId",
                table: "AcademicExamGroups",
                column: "AcademicSessionId",
                principalTable: "AcademicSession",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AcademicExams_AcademicClass_AcademicClassId",
                table: "AcademicExams",
                column: "AcademicClassId",
                principalTable: "AcademicClass",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AcademicExams_AcademicExamGroups_AcademicExamGroupId",
                table: "AcademicExams",
                column: "AcademicExamGroupId",
                principalTable: "AcademicExamGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AcademicExams_AcademicSubject_AcademicSubjectId",
                table: "AcademicExams",
                column: "AcademicSubjectId",
                principalTable: "AcademicSubject",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AcademicExams_Employee_EmployeeId",
                table: "AcademicExams",
                column: "EmployeeId",
                principalTable: "Employee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AcademicSection_AcademicClass_AcademicClassId",
                table: "AcademicSection",
                column: "AcademicClassId",
                principalTable: "AcademicClass",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AcademicSection_AcademicSession_AcademicSessionId",
                table: "AcademicSection",
                column: "AcademicSessionId",
                principalTable: "AcademicSession",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AcademicSubject_AcademicSubjectType_AcademicSubjectTypeId",
                table: "AcademicSubject",
                column: "AcademicSubjectTypeId",
                principalTable: "AcademicSubjectType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AppliedStudent_District_PermanentDistrictId",
                table: "AppliedStudent",
                column: "PermanentDistrictId",
                principalTable: "District",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AppliedStudent_District_PresentDistrictId",
                table: "AppliedStudent",
                column: "PresentDistrictId",
                principalTable: "District",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AppliedStudent_Division_PermanentDivisionId",
                table: "AppliedStudent",
                column: "PermanentDivisionId",
                principalTable: "Division",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AppliedStudent_Division_PresentDivisionId",
                table: "AppliedStudent",
                column: "PresentDivisionId",
                principalTable: "Division",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AppliedStudent_Upazila_PermanentUpazilaId",
                table: "AppliedStudent",
                column: "PermanentUpazilaId",
                principalTable: "Upazila",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AppliedStudent_Upazila_PresentUpazilaId",
                table: "AppliedStudent",
                column: "PresentUpazilaId",
                principalTable: "Upazila",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId",
                principalTable: "AspNetRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                table: "AspNetUserClaims",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                table: "AspNetUserLogins",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId",
                principalTable: "AspNetRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                table: "AspNetUserRoles",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                table: "AspNetUserTokens",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AttachDocs_AttachDocType_AttachDocTypeId",
                table: "AttachDocs",
                column: "AttachDocTypeId",
                principalTable: "AttachDocType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Chapters_AcademicSubject_AcademicSubjectId",
                table: "Chapters",
                column: "AcademicSubjectId",
                principalTable: "AcademicSubject",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ClaimStores_ProjectSubModules_SubModuleId",
                table: "ClaimStores",
                column: "SubModuleId",
                principalTable: "ProjectSubModules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ClassFeeList_AcademicClass_AcademicClassId",
                table: "ClassFeeList",
                column: "AcademicClassId",
                principalTable: "AcademicClass",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ClassFeeList_AcademicSession_AcademicSessionId",
                table: "ClassFeeList",
                column: "AcademicSessionId",
                principalTable: "AcademicSession",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ClassFeeList_StudentFeeHead_StudentFeeHeadId",
                table: "ClassFeeList",
                column: "StudentFeeHeadId",
                principalTable: "StudentFeeHead",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ClassRoutines_AcademicClass_AcademicClassId",
                table: "ClassRoutines",
                column: "AcademicClassId",
                principalTable: "AcademicClass",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ClassRoutines_AcademicSubject_AcademicSubjectId",
                table: "ClassRoutines",
                column: "AcademicSubjectId",
                principalTable: "AcademicSubject",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ClassRoutines_ClassRooms_ClassRoomId",
                table: "ClassRoutines",
                column: "ClassRoomId",
                principalTable: "ClassRooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ClassRoutines_Days_DaysId",
                table: "ClassRoutines",
                column: "DaysId",
                principalTable: "Days",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ClassRoutines_Employee_EmployeeId",
                table: "ClassRoutines",
                column: "EmployeeId",
                principalTable: "Employee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Designation_DesignationType_DesignationTypeId",
                table: "Designation",
                column: "DesignationTypeId",
                principalTable: "DesignationType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_District_Division_DivisionId",
                table: "District",
                column: "DivisionId",
                principalTable: "Division",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Employee_BloodGroup_BloodGroupId",
                table: "Employee",
                column: "BloodGroupId",
                principalTable: "BloodGroup",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Employee_Designation_DesignationId",
                table: "Employee",
                column: "DesignationId",
                principalTable: "Designation",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Employee_District_PermanentDistrictId",
                table: "Employee",
                column: "PermanentDistrictId",
                principalTable: "District",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Employee_District_PresentDistrictId",
                table: "Employee",
                column: "PresentDistrictId",
                principalTable: "District",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Employee_Division_PermanentDivisionId",
                table: "Employee",
                column: "PermanentDivisionId",
                principalTable: "Division",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Employee_Division_PresentDivisionId",
                table: "Employee",
                column: "PresentDivisionId",
                principalTable: "Division",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Employee_EmpType_EmpTypeId",
                table: "Employee",
                column: "EmpTypeId",
                principalTable: "EmpType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Employee_Gender_GenderId",
                table: "Employee",
                column: "GenderId",
                principalTable: "Gender",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Employee_Nationality_NationalityId",
                table: "Employee",
                column: "NationalityId",
                principalTable: "Nationality",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Employee_Religion_ReligionId",
                table: "Employee",
                column: "ReligionId",
                principalTable: "Religion",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Employee_Upazila_PermanentUpazilaId",
                table: "Employee",
                column: "PermanentUpazilaId",
                principalTable: "Upazila",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Employee_Upazila_PresentUpazilaId",
                table: "Employee",
                column: "PresentUpazilaId",
                principalTable: "Upazila",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ExamResultDetails_AcademicSubject_AcademicSubjectId",
                table: "ExamResultDetails",
                column: "AcademicSubjectId",
                principalTable: "AcademicSubject",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ExamResultDetails_ExamResults_ExamResultId",
                table: "ExamResultDetails",
                column: "ExamResultId",
                principalTable: "ExamResults",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ExamResults_AcademicClass_AcademicClassId",
                table: "ExamResults",
                column: "AcademicClassId",
                principalTable: "AcademicClass",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ExamResults_AcademicExamGroups_AcademicExamGroupId",
                table: "ExamResults",
                column: "AcademicExamGroupId",
                principalTable: "AcademicExamGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ExamResults_Student_StudentId",
                table: "ExamResults",
                column: "StudentId",
                principalTable: "Student",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GradingTableHists_AcademicExamGroups_AcademicExamGroupId",
                table: "GradingTableHists",
                column: "AcademicExamGroupId",
                principalTable: "AcademicExamGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_NotificationEvents_NotificationEventId",
                table: "Notifications",
                column: "NotificationEventId",
                principalTable: "NotificationEvents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OffDays_OffDayTypes_OffDayTypeId",
                table: "OffDays",
                column: "OffDayTypeId",
                principalTable: "OffDayTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectSubModules_ProjectModules_ProjectModuleId",
                table: "ProjectSubModules",
                column: "ProjectModuleId",
                principalTable: "ProjectModules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_QuestionDetails_Questions_QuestionId",
                table: "QuestionDetails",
                column: "QuestionId",
                principalTable: "Questions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Questions_Chapters_ChapterId",
                table: "Questions",
                column: "ChapterId",
                principalTable: "Chapters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Student_AcademicClass_AcademicClassId",
                table: "Student",
                column: "AcademicClassId",
                principalTable: "AcademicClass",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Student_AcademicSession_AcademicSessionId",
                table: "Student",
                column: "AcademicSessionId",
                principalTable: "AcademicSession",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Student_District_PermanentDistrictId",
                table: "Student",
                column: "PermanentDistrictId",
                principalTable: "District",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Student_District_PresentDistrictId",
                table: "Student",
                column: "PresentDistrictId",
                principalTable: "District",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Student_Division_PermanentDivisionId",
                table: "Student",
                column: "PermanentDivisionId",
                principalTable: "Division",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Student_Division_PresentDivisionId",
                table: "Student",
                column: "PresentDivisionId",
                principalTable: "Division",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Student_Gender_GenderId",
                table: "Student",
                column: "GenderId",
                principalTable: "Gender",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Student_Nationality_NationalityId",
                table: "Student",
                column: "NationalityId",
                principalTable: "Nationality",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Student_Religion_ReligionId",
                table: "Student",
                column: "ReligionId",
                principalTable: "Religion",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Student_Upazila_PermanentUpazilaId",
                table: "Student",
                column: "PermanentUpazilaId",
                principalTable: "Upazila",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Student_Upazila_PresentUpazilaId",
                table: "Student",
                column: "PresentUpazilaId",
                principalTable: "Upazila",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentFeeAllocations_StudentFeeHead_StudentFeeHeadId",
                table: "StudentFeeAllocations",
                column: "StudentFeeHeadId",
                principalTable: "StudentFeeHead",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentFeeAllocations_Student_StudentId",
                table: "StudentFeeAllocations",
                column: "StudentId",
                principalTable: "Student",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentPayment_Student_StudentId",
                table: "StudentPayment",
                column: "StudentId",
                principalTable: "Student",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentPaymentDetails_StudentFeeHead_StudentFeeHeadId",
                table: "StudentPaymentDetails",
                column: "StudentFeeHeadId",
                principalTable: "StudentFeeHead",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentPaymentDetails_StudentPayment_StudentPaymentId",
                table: "StudentPaymentDetails",
                column: "StudentPaymentId",
                principalTable: "StudentPayment",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SubjectEnrollmentDetails_AcademicSubjectType_AcademicSubjectTypeId",
                table: "SubjectEnrollmentDetails",
                column: "AcademicSubjectTypeId",
                principalTable: "AcademicSubjectType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SubjectEnrollmentDetails_AcademicSubject_AcademicSubjectId",
                table: "SubjectEnrollmentDetails",
                column: "AcademicSubjectId",
                principalTable: "AcademicSubject",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SubjectEnrollmentDetails_SubjectEnrollments_SubjectEnrollmentId",
                table: "SubjectEnrollmentDetails",
                column: "SubjectEnrollmentId",
                principalTable: "SubjectEnrollments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SubjectEnrollments_Student_StudentId",
                table: "SubjectEnrollments",
                column: "StudentId",
                principalTable: "Student",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TeacherSubjectMaps_AcademicClassSubjects_AcademicClassSubjectId",
                table: "TeacherSubjectMaps",
                column: "AcademicClassSubjectId",
                principalTable: "AcademicClassSubjects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Upazila_District_DistrictId",
                table: "Upazila",
                column: "DistrictId",
                principalTable: "District",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AcademicClassSubjects_AcademicClass_AcademicClassId",
                table: "AcademicClassSubjects");

            migrationBuilder.DropForeignKey(
                name: "FK_AcademicClassSubjects_AcademicSubject_AcademicSubjectId",
                table: "AcademicClassSubjects");

            migrationBuilder.DropForeignKey(
                name: "FK_AcademicExamDetails_AcademicExams_AcademicExamId",
                table: "AcademicExamDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_AcademicExamDetails_Student_StudentId",
                table: "AcademicExamDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_AcademicExamGroups_AcademicExamTypes_AcademicExamTypeId",
                table: "AcademicExamGroups");

            migrationBuilder.DropForeignKey(
                name: "FK_AcademicExamGroups_AcademicSession_AcademicSessionId",
                table: "AcademicExamGroups");

            migrationBuilder.DropForeignKey(
                name: "FK_AcademicExams_AcademicClass_AcademicClassId",
                table: "AcademicExams");

            migrationBuilder.DropForeignKey(
                name: "FK_AcademicExams_AcademicExamGroups_AcademicExamGroupId",
                table: "AcademicExams");

            migrationBuilder.DropForeignKey(
                name: "FK_AcademicExams_AcademicSubject_AcademicSubjectId",
                table: "AcademicExams");

            migrationBuilder.DropForeignKey(
                name: "FK_AcademicExams_Employee_EmployeeId",
                table: "AcademicExams");

            migrationBuilder.DropForeignKey(
                name: "FK_AcademicSection_AcademicClass_AcademicClassId",
                table: "AcademicSection");

            migrationBuilder.DropForeignKey(
                name: "FK_AcademicSection_AcademicSession_AcademicSessionId",
                table: "AcademicSection");

            migrationBuilder.DropForeignKey(
                name: "FK_AcademicSubject_AcademicSubjectType_AcademicSubjectTypeId",
                table: "AcademicSubject");

            migrationBuilder.DropForeignKey(
                name: "FK_AppliedStudent_District_PermanentDistrictId",
                table: "AppliedStudent");

            migrationBuilder.DropForeignKey(
                name: "FK_AppliedStudent_District_PresentDistrictId",
                table: "AppliedStudent");

            migrationBuilder.DropForeignKey(
                name: "FK_AppliedStudent_Division_PermanentDivisionId",
                table: "AppliedStudent");

            migrationBuilder.DropForeignKey(
                name: "FK_AppliedStudent_Division_PresentDivisionId",
                table: "AppliedStudent");

            migrationBuilder.DropForeignKey(
                name: "FK_AppliedStudent_Upazila_PermanentUpazilaId",
                table: "AppliedStudent");

            migrationBuilder.DropForeignKey(
                name: "FK_AppliedStudent_Upazila_PresentUpazilaId",
                table: "AppliedStudent");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                table: "AspNetRoleClaims");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                table: "AspNetUserClaims");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                table: "AspNetUserLogins");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                table: "AspNetUserRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                table: "AspNetUserRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                table: "AspNetUserTokens");

            migrationBuilder.DropForeignKey(
                name: "FK_AttachDocs_AttachDocType_AttachDocTypeId",
                table: "AttachDocs");

            migrationBuilder.DropForeignKey(
                name: "FK_Chapters_AcademicSubject_AcademicSubjectId",
                table: "Chapters");

            migrationBuilder.DropForeignKey(
                name: "FK_ClaimStores_ProjectSubModules_SubModuleId",
                table: "ClaimStores");

            migrationBuilder.DropForeignKey(
                name: "FK_ClassFeeList_AcademicClass_AcademicClassId",
                table: "ClassFeeList");

            migrationBuilder.DropForeignKey(
                name: "FK_ClassFeeList_AcademicSession_AcademicSessionId",
                table: "ClassFeeList");

            migrationBuilder.DropForeignKey(
                name: "FK_ClassFeeList_StudentFeeHead_StudentFeeHeadId",
                table: "ClassFeeList");

            migrationBuilder.DropForeignKey(
                name: "FK_ClassRoutines_AcademicClass_AcademicClassId",
                table: "ClassRoutines");

            migrationBuilder.DropForeignKey(
                name: "FK_ClassRoutines_AcademicSubject_AcademicSubjectId",
                table: "ClassRoutines");

            migrationBuilder.DropForeignKey(
                name: "FK_ClassRoutines_ClassRooms_ClassRoomId",
                table: "ClassRoutines");

            migrationBuilder.DropForeignKey(
                name: "FK_ClassRoutines_Days_DaysId",
                table: "ClassRoutines");

            migrationBuilder.DropForeignKey(
                name: "FK_ClassRoutines_Employee_EmployeeId",
                table: "ClassRoutines");

            migrationBuilder.DropForeignKey(
                name: "FK_Designation_DesignationType_DesignationTypeId",
                table: "Designation");

            migrationBuilder.DropForeignKey(
                name: "FK_District_Division_DivisionId",
                table: "District");

            migrationBuilder.DropForeignKey(
                name: "FK_Employee_BloodGroup_BloodGroupId",
                table: "Employee");

            migrationBuilder.DropForeignKey(
                name: "FK_Employee_Designation_DesignationId",
                table: "Employee");

            migrationBuilder.DropForeignKey(
                name: "FK_Employee_District_PermanentDistrictId",
                table: "Employee");

            migrationBuilder.DropForeignKey(
                name: "FK_Employee_District_PresentDistrictId",
                table: "Employee");

            migrationBuilder.DropForeignKey(
                name: "FK_Employee_Division_PermanentDivisionId",
                table: "Employee");

            migrationBuilder.DropForeignKey(
                name: "FK_Employee_Division_PresentDivisionId",
                table: "Employee");

            migrationBuilder.DropForeignKey(
                name: "FK_Employee_EmpType_EmpTypeId",
                table: "Employee");

            migrationBuilder.DropForeignKey(
                name: "FK_Employee_Gender_GenderId",
                table: "Employee");

            migrationBuilder.DropForeignKey(
                name: "FK_Employee_Nationality_NationalityId",
                table: "Employee");

            migrationBuilder.DropForeignKey(
                name: "FK_Employee_Religion_ReligionId",
                table: "Employee");

            migrationBuilder.DropForeignKey(
                name: "FK_Employee_Upazila_PermanentUpazilaId",
                table: "Employee");

            migrationBuilder.DropForeignKey(
                name: "FK_Employee_Upazila_PresentUpazilaId",
                table: "Employee");

            migrationBuilder.DropForeignKey(
                name: "FK_ExamResultDetails_AcademicSubject_AcademicSubjectId",
                table: "ExamResultDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_ExamResultDetails_ExamResults_ExamResultId",
                table: "ExamResultDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_ExamResults_AcademicClass_AcademicClassId",
                table: "ExamResults");

            migrationBuilder.DropForeignKey(
                name: "FK_ExamResults_AcademicExamGroups_AcademicExamGroupId",
                table: "ExamResults");

            migrationBuilder.DropForeignKey(
                name: "FK_ExamResults_Student_StudentId",
                table: "ExamResults");

            migrationBuilder.DropForeignKey(
                name: "FK_GradingTableHists_AcademicExamGroups_AcademicExamGroupId",
                table: "GradingTableHists");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_NotificationEvents_NotificationEventId",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_OffDays_OffDayTypes_OffDayTypeId",
                table: "OffDays");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectSubModules_ProjectModules_ProjectModuleId",
                table: "ProjectSubModules");

            migrationBuilder.DropForeignKey(
                name: "FK_QuestionDetails_Questions_QuestionId",
                table: "QuestionDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_Questions_Chapters_ChapterId",
                table: "Questions");

            migrationBuilder.DropForeignKey(
                name: "FK_Student_AcademicClass_AcademicClassId",
                table: "Student");

            migrationBuilder.DropForeignKey(
                name: "FK_Student_AcademicSession_AcademicSessionId",
                table: "Student");

            migrationBuilder.DropForeignKey(
                name: "FK_Student_District_PermanentDistrictId",
                table: "Student");

            migrationBuilder.DropForeignKey(
                name: "FK_Student_District_PresentDistrictId",
                table: "Student");

            migrationBuilder.DropForeignKey(
                name: "FK_Student_Division_PermanentDivisionId",
                table: "Student");

            migrationBuilder.DropForeignKey(
                name: "FK_Student_Division_PresentDivisionId",
                table: "Student");

            migrationBuilder.DropForeignKey(
                name: "FK_Student_Gender_GenderId",
                table: "Student");

            migrationBuilder.DropForeignKey(
                name: "FK_Student_Nationality_NationalityId",
                table: "Student");

            migrationBuilder.DropForeignKey(
                name: "FK_Student_Religion_ReligionId",
                table: "Student");

            migrationBuilder.DropForeignKey(
                name: "FK_Student_Upazila_PermanentUpazilaId",
                table: "Student");

            migrationBuilder.DropForeignKey(
                name: "FK_Student_Upazila_PresentUpazilaId",
                table: "Student");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentFeeAllocations_StudentFeeHead_StudentFeeHeadId",
                table: "StudentFeeAllocations");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentFeeAllocations_Student_StudentId",
                table: "StudentFeeAllocations");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentPayment_Student_StudentId",
                table: "StudentPayment");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentPaymentDetails_StudentFeeHead_StudentFeeHeadId",
                table: "StudentPaymentDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentPaymentDetails_StudentPayment_StudentPaymentId",
                table: "StudentPaymentDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_SubjectEnrollmentDetails_AcademicSubjectType_AcademicSubjectTypeId",
                table: "SubjectEnrollmentDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_SubjectEnrollmentDetails_AcademicSubject_AcademicSubjectId",
                table: "SubjectEnrollmentDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_SubjectEnrollmentDetails_SubjectEnrollments_SubjectEnrollmentId",
                table: "SubjectEnrollmentDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_SubjectEnrollments_Student_StudentId",
                table: "SubjectEnrollments");

            migrationBuilder.DropForeignKey(
                name: "FK_TeacherSubjectMaps_AcademicClassSubjects_AcademicClassSubjectId",
                table: "TeacherSubjectMaps");

            migrationBuilder.DropForeignKey(
                name: "FK_Upazila_District_DistrictId",
                table: "Upazila");

            migrationBuilder.DropTable(
                name: "AttendanceMachines");

            migrationBuilder.DropColumn(
                name: "IsSynced",
                table: "Tran_MachineRawPunch");

            migrationBuilder.DropColumn(
                name: "MachineSerialNo",
                table: "Tran_MachineRawPunch");

            migrationBuilder.DropColumn(
                name: "VerifyMode",
                table: "Tran_MachineRawPunch");

            migrationBuilder.DropColumn(
                name: "MachineUserId",
                table: "Student");

            migrationBuilder.DropColumn(
                name: "MachineUserId",
                table: "Employee");

            migrationBuilder.AlterColumn<string>(
                name: "MachineNo",
                table: "Tran_MachineRawPunch",
                type: "nvarchar(5)",
                maxLength: 5,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CardNo",
                table: "Tran_MachineRawPunch",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AcademicClassSubjects_AcademicClass_AcademicClassId",
                table: "AcademicClassSubjects",
                column: "AcademicClassId",
                principalTable: "AcademicClass",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AcademicClassSubjects_AcademicSubject_AcademicSubjectId",
                table: "AcademicClassSubjects",
                column: "AcademicSubjectId",
                principalTable: "AcademicSubject",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AcademicExamDetails_AcademicExams_AcademicExamId",
                table: "AcademicExamDetails",
                column: "AcademicExamId",
                principalTable: "AcademicExams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AcademicExamDetails_Student_StudentId",
                table: "AcademicExamDetails",
                column: "StudentId",
                principalTable: "Student",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AcademicExamGroups_AcademicExamTypes_AcademicExamTypeId",
                table: "AcademicExamGroups",
                column: "AcademicExamTypeId",
                principalTable: "AcademicExamTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AcademicExamGroups_AcademicSession_AcademicSessionId",
                table: "AcademicExamGroups",
                column: "AcademicSessionId",
                principalTable: "AcademicSession",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AcademicExams_AcademicClass_AcademicClassId",
                table: "AcademicExams",
                column: "AcademicClassId",
                principalTable: "AcademicClass",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AcademicExams_AcademicExamGroups_AcademicExamGroupId",
                table: "AcademicExams",
                column: "AcademicExamGroupId",
                principalTable: "AcademicExamGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AcademicExams_AcademicSubject_AcademicSubjectId",
                table: "AcademicExams",
                column: "AcademicSubjectId",
                principalTable: "AcademicSubject",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AcademicExams_Employee_EmployeeId",
                table: "AcademicExams",
                column: "EmployeeId",
                principalTable: "Employee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AcademicSection_AcademicClass_AcademicClassId",
                table: "AcademicSection",
                column: "AcademicClassId",
                principalTable: "AcademicClass",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AcademicSection_AcademicSession_AcademicSessionId",
                table: "AcademicSection",
                column: "AcademicSessionId",
                principalTable: "AcademicSession",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AcademicSubject_AcademicSubjectType_AcademicSubjectTypeId",
                table: "AcademicSubject",
                column: "AcademicSubjectTypeId",
                principalTable: "AcademicSubjectType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

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
                name: "FK_AppliedStudent_Division_PermanentDivisionId",
                table: "AppliedStudent",
                column: "PermanentDivisionId",
                principalTable: "Division",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AppliedStudent_Division_PresentDivisionId",
                table: "AppliedStudent",
                column: "PresentDivisionId",
                principalTable: "Division",
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

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId",
                principalTable: "AspNetRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                table: "AspNetUserClaims",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                table: "AspNetUserLogins",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId",
                principalTable: "AspNetRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                table: "AspNetUserRoles",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                table: "AspNetUserTokens",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AttachDocs_AttachDocType_AttachDocTypeId",
                table: "AttachDocs",
                column: "AttachDocTypeId",
                principalTable: "AttachDocType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Chapters_AcademicSubject_AcademicSubjectId",
                table: "Chapters",
                column: "AcademicSubjectId",
                principalTable: "AcademicSubject",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ClaimStores_ProjectSubModules_SubModuleId",
                table: "ClaimStores",
                column: "SubModuleId",
                principalTable: "ProjectSubModules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ClassFeeList_AcademicClass_AcademicClassId",
                table: "ClassFeeList",
                column: "AcademicClassId",
                principalTable: "AcademicClass",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ClassFeeList_AcademicSession_AcademicSessionId",
                table: "ClassFeeList",
                column: "AcademicSessionId",
                principalTable: "AcademicSession",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ClassFeeList_StudentFeeHead_StudentFeeHeadId",
                table: "ClassFeeList",
                column: "StudentFeeHeadId",
                principalTable: "StudentFeeHead",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ClassRoutines_AcademicClass_AcademicClassId",
                table: "ClassRoutines",
                column: "AcademicClassId",
                principalTable: "AcademicClass",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ClassRoutines_AcademicSubject_AcademicSubjectId",
                table: "ClassRoutines",
                column: "AcademicSubjectId",
                principalTable: "AcademicSubject",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ClassRoutines_ClassRooms_ClassRoomId",
                table: "ClassRoutines",
                column: "ClassRoomId",
                principalTable: "ClassRooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ClassRoutines_Days_DaysId",
                table: "ClassRoutines",
                column: "DaysId",
                principalTable: "Days",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ClassRoutines_Employee_EmployeeId",
                table: "ClassRoutines",
                column: "EmployeeId",
                principalTable: "Employee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Designation_DesignationType_DesignationTypeId",
                table: "Designation",
                column: "DesignationTypeId",
                principalTable: "DesignationType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_District_Division_DivisionId",
                table: "District",
                column: "DivisionId",
                principalTable: "Division",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Employee_BloodGroup_BloodGroupId",
                table: "Employee",
                column: "BloodGroupId",
                principalTable: "BloodGroup",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Employee_Designation_DesignationId",
                table: "Employee",
                column: "DesignationId",
                principalTable: "Designation",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Employee_District_PermanentDistrictId",
                table: "Employee",
                column: "PermanentDistrictId",
                principalTable: "District",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Employee_District_PresentDistrictId",
                table: "Employee",
                column: "PresentDistrictId",
                principalTable: "District",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Employee_Division_PermanentDivisionId",
                table: "Employee",
                column: "PermanentDivisionId",
                principalTable: "Division",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Employee_Division_PresentDivisionId",
                table: "Employee",
                column: "PresentDivisionId",
                principalTable: "Division",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Employee_EmpType_EmpTypeId",
                table: "Employee",
                column: "EmpTypeId",
                principalTable: "EmpType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Employee_Gender_GenderId",
                table: "Employee",
                column: "GenderId",
                principalTable: "Gender",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Employee_Nationality_NationalityId",
                table: "Employee",
                column: "NationalityId",
                principalTable: "Nationality",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Employee_Religion_ReligionId",
                table: "Employee",
                column: "ReligionId",
                principalTable: "Religion",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Employee_Upazila_PermanentUpazilaId",
                table: "Employee",
                column: "PermanentUpazilaId",
                principalTable: "Upazila",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Employee_Upazila_PresentUpazilaId",
                table: "Employee",
                column: "PresentUpazilaId",
                principalTable: "Upazila",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ExamResultDetails_AcademicSubject_AcademicSubjectId",
                table: "ExamResultDetails",
                column: "AcademicSubjectId",
                principalTable: "AcademicSubject",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ExamResultDetails_ExamResults_ExamResultId",
                table: "ExamResultDetails",
                column: "ExamResultId",
                principalTable: "ExamResults",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ExamResults_AcademicClass_AcademicClassId",
                table: "ExamResults",
                column: "AcademicClassId",
                principalTable: "AcademicClass",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ExamResults_AcademicExamGroups_AcademicExamGroupId",
                table: "ExamResults",
                column: "AcademicExamGroupId",
                principalTable: "AcademicExamGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ExamResults_Student_StudentId",
                table: "ExamResults",
                column: "StudentId",
                principalTable: "Student",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GradingTableHists_AcademicExamGroups_AcademicExamGroupId",
                table: "GradingTableHists",
                column: "AcademicExamGroupId",
                principalTable: "AcademicExamGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_NotificationEvents_NotificationEventId",
                table: "Notifications",
                column: "NotificationEventId",
                principalTable: "NotificationEvents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OffDays_OffDayTypes_OffDayTypeId",
                table: "OffDays",
                column: "OffDayTypeId",
                principalTable: "OffDayTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectSubModules_ProjectModules_ProjectModuleId",
                table: "ProjectSubModules",
                column: "ProjectModuleId",
                principalTable: "ProjectModules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_QuestionDetails_Questions_QuestionId",
                table: "QuestionDetails",
                column: "QuestionId",
                principalTable: "Questions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Questions_Chapters_ChapterId",
                table: "Questions",
                column: "ChapterId",
                principalTable: "Chapters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Student_AcademicClass_AcademicClassId",
                table: "Student",
                column: "AcademicClassId",
                principalTable: "AcademicClass",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Student_AcademicSession_AcademicSessionId",
                table: "Student",
                column: "AcademicSessionId",
                principalTable: "AcademicSession",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Student_District_PermanentDistrictId",
                table: "Student",
                column: "PermanentDistrictId",
                principalTable: "District",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Student_District_PresentDistrictId",
                table: "Student",
                column: "PresentDistrictId",
                principalTable: "District",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Student_Division_PermanentDivisionId",
                table: "Student",
                column: "PermanentDivisionId",
                principalTable: "Division",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Student_Division_PresentDivisionId",
                table: "Student",
                column: "PresentDivisionId",
                principalTable: "Division",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Student_Gender_GenderId",
                table: "Student",
                column: "GenderId",
                principalTable: "Gender",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Student_Nationality_NationalityId",
                table: "Student",
                column: "NationalityId",
                principalTable: "Nationality",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Student_Religion_ReligionId",
                table: "Student",
                column: "ReligionId",
                principalTable: "Religion",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Student_Upazila_PermanentUpazilaId",
                table: "Student",
                column: "PermanentUpazilaId",
                principalTable: "Upazila",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Student_Upazila_PresentUpazilaId",
                table: "Student",
                column: "PresentUpazilaId",
                principalTable: "Upazila",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentFeeAllocations_StudentFeeHead_StudentFeeHeadId",
                table: "StudentFeeAllocations",
                column: "StudentFeeHeadId",
                principalTable: "StudentFeeHead",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentFeeAllocations_Student_StudentId",
                table: "StudentFeeAllocations",
                column: "StudentId",
                principalTable: "Student",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentPayment_Student_StudentId",
                table: "StudentPayment",
                column: "StudentId",
                principalTable: "Student",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentPaymentDetails_StudentFeeHead_StudentFeeHeadId",
                table: "StudentPaymentDetails",
                column: "StudentFeeHeadId",
                principalTable: "StudentFeeHead",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentPaymentDetails_StudentPayment_StudentPaymentId",
                table: "StudentPaymentDetails",
                column: "StudentPaymentId",
                principalTable: "StudentPayment",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SubjectEnrollmentDetails_AcademicSubjectType_AcademicSubjectTypeId",
                table: "SubjectEnrollmentDetails",
                column: "AcademicSubjectTypeId",
                principalTable: "AcademicSubjectType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SubjectEnrollmentDetails_AcademicSubject_AcademicSubjectId",
                table: "SubjectEnrollmentDetails",
                column: "AcademicSubjectId",
                principalTable: "AcademicSubject",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SubjectEnrollmentDetails_SubjectEnrollments_SubjectEnrollmentId",
                table: "SubjectEnrollmentDetails",
                column: "SubjectEnrollmentId",
                principalTable: "SubjectEnrollments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SubjectEnrollments_Student_StudentId",
                table: "SubjectEnrollments",
                column: "StudentId",
                principalTable: "Student",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TeacherSubjectMaps_AcademicClassSubjects_AcademicClassSubjectId",
                table: "TeacherSubjectMaps",
                column: "AcademicClassSubjectId",
                principalTable: "AcademicClassSubjects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Upazila_District_DistrictId",
                table: "Upazila",
                column: "DistrictId",
                principalTable: "District",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
