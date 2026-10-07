using Microsoft.AspNetCore.Authorization;

namespace SMS_App.Configurations;

public class AuthorizationPolicies
{
    public static void ConfigureAuthorization(AuthorizationOptions options)
    {
        //Academic Class
        options.AddPolicy("IndexAcademicClassesPolicy", policy => policy.RequireClaim("View Academic Class"));
        options.AddPolicy("DetailsAcademicClassesPolicy", policy => policy.RequireClaim("Details View Academic Class"));
        options.AddPolicy("CreateAcademicClassesPolicy", policy => policy.RequireClaim("Create Academic Class"));
        options.AddPolicy("EditAcademicClassesPolicy", policy => policy.RequireClaim("Edit Academic Class"));

        //Academic Exam Group
        options.AddPolicy("IndexAcademicExamGroupPolicy", policy => policy.RequireClaim("View Academic Exam Group"));
        options.AddPolicy("DetailsAcademicExamGroupPolicy", policy => policy.RequireClaim("Details Academic Exam Group"));
        options.AddPolicy("CreateAcademicExamGroupPolicy", policy => policy.RequireClaim("Create Academic Exam Group"));
        options.AddPolicy("EditAcademicExamGroupPolicy", policy => policy.RequireClaim("Edit Academic Exam Group"));
        options.AddPolicy("DeleteAcademicExamGroupPolicy", policy => policy.RequireClaim("Delete Academic Exam Group"));

        //Academic Exam
        options.AddPolicy("IndexAcademicExamPolicy", policy => policy.RequireClaim("View Academic Exam"));
        options.AddPolicy("DetailsAcademicExamPolicy", policy => policy.RequireClaim("Details Academic Exam"));
        options.AddPolicy("CreateAcademicExamPolicy", policy => policy.RequireClaim("Create Academic Exam"));
        options.AddPolicy("EditAcademicExamPolicy", policy => policy.RequireClaim("Edit Academic Exam"));
        options.AddPolicy("DeleteAcademicExamPolicy", policy => policy.RequireClaim("Delete Academic Exam Group"));
        options.AddPolicy("ExamMarkSubmitAcademicExamPolicy", policy => policy.RequireClaim("Submit Academic Exam Marks"));
        options.AddPolicy("AdmitCardAcademicExamPolicy", policy => policy.RequireClaim("Report Admit Card"));
        options.AddPolicy("LockAcademicExamPolicy", policy => policy.RequireClaim("Lock Academic Exam"));

        //Academic Exam Type
        options.AddPolicy("IndexAcademicExamTypePolicy", policy => policy.RequireClaim("View Academic Exam Type"));
        options.AddPolicy("DetailsAcademicExamTypePolicy", policy => policy.RequireClaim("Details Academic Exam Type"));
        options.AddPolicy("CraeteAcademicExamTypePolicy", policy => policy.RequireClaim("Create Academic Exam Type"));
        options.AddPolicy("EditAcademicExamTypePolicy", policy => policy.RequireClaim("Edit Academic Exam Type"));
        options.AddPolicy("DeleteAcademicExamTypePolicy", policy => policy.RequireClaim("Delete Academic Exam Type"));

        //Academic Section
        options.AddPolicy("IndexAcademicSectionPolicy", policy => policy.RequireClaim("View Academic Section"));
        options.AddPolicy("DetailsAcademicSectionPolicy", policy => policy.RequireClaim("Details Academic Section"));
        options.AddPolicy("CreateAcademicSectionPolicy", policy => policy.RequireClaim("Create Academic Section"));
        options.AddPolicy("EditAcademicSectionPolicy", policy => policy.RequireClaim("Edit Academic Section"));
        options.AddPolicy("DeleteAcademicSectionPolicy", policy => policy.RequireClaim("Delete Academic Section"));

        //Academic Session
        options.AddPolicy("IndexAcademicSessionPolicy", policy => policy.RequireClaim("View Academic Session"));
        options.AddPolicy("DetailsAcademicSessionPolicy", policy => policy.RequireClaim("Details Academic Session"));
        options.AddPolicy("CreateAcademicSessionPolicy", policy => policy.RequireClaim("Create Academic Session"));
        options.AddPolicy("EditAcademicSessionPolicy", policy => policy.RequireClaim("Edit Academic Session"));
        options.AddPolicy("DeleteAcademicSessionPolicy", policy => policy.RequireClaim("Delete Academic Session"));

        //Academic Subjects
        options.AddPolicy("IndexAcademicSubjectPolicy", policy => policy.RequireClaim("View Academic Subject"));
        options.AddPolicy("DetailsAcademicSubjectPolicy", policy => policy.RequireClaim("Details Academic Subject"));
        options.AddPolicy("CreateAcademicSubjectPolicy", policy => policy.RequireClaim("Create Academic Subject"));
        options.AddPolicy("EditAcademicSubjectPolicy", policy => policy.RequireClaim("Edit Academic Subject"));
        options.AddPolicy("DeleteAcademicSubjectPolicy", policy => policy.RequireClaim("Delete Academic Subject"));
        options.AddPolicy("ViewClassWiseSubjectAllocationAcademicSubjectPolicy", policy => policy.RequireClaim("View Class-wise Subject Allocation"));
        options.AddPolicy("CreateClassWiseSubjectAllocationAcademicSubjectPolicy", policy => policy.RequireClaim("Create Class-wise Subject Allocation"));
        options.AddPolicy("DeleteClassWiseSubjectAllocationAcademicSubjectPolicy", policy => policy.RequireClaim("Delete Class-wise Subject Allocation"));

        //Academic Subject Types
        options.AddPolicy("IndexAcademicSubjectTypesPolicy", policy => policy.RequireClaim("View Subject Type"));
        options.AddPolicy("DetailsAcademicSubjectTypesPolicy", policy => policy.RequireClaim("Details Subject Type"));
        options.AddPolicy("CreateAcademicSubjectTypesPolicy", policy => policy.RequireClaim("Create  Subject Type"));
        options.AddPolicy("EditAcademicSubjectTypesPolicy", policy => policy.RequireClaim("Edit Subject Type"));
        options.AddPolicy("DeleteAcademicSubjectTypesPolicy", policy => policy.RequireClaim("Delete Subject Type"));

        //Accounts
        options.AddPolicy("RegisterAccountsPolicy", policy => policy.RequireClaim("Register New User"));
        options.AddPolicy("UserListAccountsPolicy", policy => policy.RequireClaim("User List"));
        options.AddPolicy("EditUserAccountsPolicy", policy => policy.RequireClaim("Edit User Accounts"));
        options.AddPolicy("DeleteUserAccountsPolicy", policy => policy.RequireClaim("Delete User Account"));
        options.AddPolicy("ViewRolesAccountsPolicy", policy => policy.RequireClaim("View User Role"));
        options.AddPolicy("CreateRoleAccountsPolicy", policy => policy.RequireClaim("Create User Role"));
        options.AddPolicy("EditRoleAccountsPolicy", policy => policy.RequireClaim("Edit User Role"));
        options.AddPolicy("AddOrRemoveUserAccountsPolicy", policy => policy.RequireClaim("Add or Remove User From Role"));

        //Administrations
        options.AddPolicy("ViewUserProfileAdministrationsPolicy", policy => policy.RequireClaim("View User Profile"));
        options.AddPolicy("EditUserProfileAdministrationsPolicy", policy => policy.RequireClaim("Edit User Profile"));

        //Attendance
        options.AddPolicy("IndexAttendanceMachinesPolicy", policy => policy.RequireClaim("View Attendances"));
        options.AddPolicy("DetailsAttendanceMachinesPolicy", policy => policy.RequireClaim("View Details Attendance"));
        options.AddPolicy("CreateAttendanceMachinesPolicy", policy => policy.RequireClaim("Create Attendance"));
        options.AddPolicy("EditAttendanceMachinesPolicy", policy => policy.RequireClaim("Edit Attendance"));
        options.AddPolicy("DeleteAttendanceMachinesPolicy", policy => policy.RequireClaim("Delete Attendance"));

        //Attendance Machine Devices (Fingerprint)
        options.AddPolicy("IndexAttendanceMachineDevicesPolicy", policy => policy.RequireClaim("View Attendance Machines"));
        options.AddPolicy("DetailsAttendanceMachineDevicesPolicy", policy => policy.RequireClaim("View Details Attendance Machines"));
        options.AddPolicy("CreateAttendanceMachineDevicesPolicy", policy => policy.RequireClaim("Create Attendance Machines"));
        options.AddPolicy("EditAttendanceMachineDevicesPolicy", policy => policy.RequireClaim("Edit Attendance Machines"));
        options.AddPolicy("DeleteAttendanceMachineDevicesPolicy", policy => policy.RequireClaim("Delete Attendance Machines"));
        options.AddPolicy("ManageMachineUsersPolicy", policy => policy.RequireClaim("Manage Machine Users"));

        //Academic Blood Groups
        options.AddPolicy("IndexBloodGroupsPolicy", policy => policy.RequireClaim("View Blood Groups"));
        options.AddPolicy("DetailsBloodGroupsPolicy", policy => policy.RequireClaim("Details Blood Groups"));
        options.AddPolicy("CreateBloodGroupsPolicy", policy => policy.RequireClaim("Create  Blood Groups"));
        options.AddPolicy("EditBloodGroupsPolicy", policy => policy.RequireClaim("Edit Blood Groups"));
        options.AddPolicy("DeleteBloodGroupsPolicy", policy => policy.RequireClaim("Delete Blood Groups"));

        //Chapters 
        options.AddPolicy("ViewChaptersPolicy", policy => policy.RequireClaim("View Chapter"));
        options.AddPolicy("CreateChaptersPolicy", policy => policy.RequireClaim("Edit Chapter"));

        //Claim Store
        options.AddPolicy("IndexClaimStoresPolicy", policy => policy.RequireClaim("View Claim Store"));
        options.AddPolicy("CreateClaimStoresPolicy", policy => policy.RequireClaim("Create Claim Store"));
        options.AddPolicy("EditClaimStoresPolicy", policy => policy.RequireClaim("Edit Claim Store"));
        options.AddPolicy("DeleteClaimStoresPolicy", policy => policy.RequireClaim("Delete Claim Store"));

        //Class Fee Lists
        options.AddPolicy("IndexClassFeeListsPolicy", policy => policy.RequireClaim("View Class Fee Lists"));
        options.AddPolicy("DetailsClassFeeListsPolicy", policy => policy.RequireClaim("Details Class Fee Lists"));
        options.AddPolicy("CreateClassFeeListsPolicy", policy => policy.RequireClaim("Create Class Fee Lists"));
        options.AddPolicy("EditClassFeeListsPolicy", policy => policy.RequireClaim("Edit Class Fee Lists"));
        options.AddPolicy("DeleteClassFeeListsPolicy", policy => policy.RequireClaim("Delete Class Fee Lists"));

        //Designation
        options.AddPolicy("IndexDesignationsPolicy", policy => policy.RequireClaim("View Designations"));
        options.AddPolicy("DetailsDesignationsPolicy", policy => policy.RequireClaim("View Details Designations"));
        options.AddPolicy("CreateDesignationsPolicy", policy => policy.RequireClaim("Create Designations"));
        options.AddPolicy("EditDesignationsPolicy", policy => policy.RequireClaim("Edit Designations"));
        options.AddPolicy("DeleteDesignationsPolicy", policy => policy.RequireClaim("Delete Designations"));

        //Designation Type
        options.AddPolicy("IndexDesignationTypesPolicy", policy => policy.RequireClaim("View Designation Types"));
        options.AddPolicy("DetailsDesignationTypesPolicy", policy => policy.RequireClaim("Details Designation Types"));
        options.AddPolicy("CreateDesignationTypesPolicy", policy => policy.RequireClaim("Create Designation Types"));
        options.AddPolicy("EditDesignationTypesPolicy", policy => policy.RequireClaim("Edit Designation Types"));
        options.AddPolicy("DeleteDesignationTypesPolicy", policy => policy.RequireClaim("Delete Designation Types"));

        //Districts
        options.AddPolicy("IndexDistrictsPolicy", policy => policy.RequireClaim("View Districts"));
        options.AddPolicy("DetailsDistrictsPolicy", policy => policy.RequireClaim("Details Districts"));
        options.AddPolicy("CreateDistrictsPolicy", policy => policy.RequireClaim("Create Districts"));
        options.AddPolicy("EditDistrictsPolicy", policy => policy.RequireClaim("Edit Districts"));
        options.AddPolicy("DeleteDistrictsPolicy", policy => policy.RequireClaim("Delete Districts"));

        //Divisions
        options.AddPolicy("IndexDivisionsPolicy", policy => policy.RequireClaim("View Divisions"));
        options.AddPolicy("DetailsDivisionsPolicy", policy => policy.RequireClaim("Details Divisions"));
        options.AddPolicy("CreateDivisionsPolicy", policy => policy.RequireClaim("Create Divisions"));
        options.AddPolicy("EditDivisionsPolicy", policy => policy.RequireClaim("Edit Divisions"));
        options.AddPolicy("DeleteDivisionsPolicy", policy => policy.RequireClaim("Delete Divisions"));

        //Employees
        options.AddPolicy("IndexEmployeesPolicy", policy => policy.RequireClaim("View Employee"));
        options.AddPolicy("DetailsEmployeesPolicy", policy => policy.RequireClaim("Details Employee"));
        options.AddPolicy("CreateEmployeesPolicy", policy => policy.RequireClaim("Create Employee"));
        options.AddPolicy("EditEmployeesPolicy", policy => policy.RequireClaim("Edit Employee"));
        options.AddPolicy("DeleteEmployeesPolicy", policy => policy.RequireClaim("Delete Employee"));

        //Emp Types
        options.AddPolicy("IndexEmpTypesPolicy", policy => policy.RequireClaim("View Employee Types"));
        options.AddPolicy("DetailsEmpTypesPolicy", policy => policy.RequireClaim("Details Employee Types"));
        options.AddPolicy("CreateEmpTypesPolicy", policy => policy.RequireClaim("Create Employee Types"));
        options.AddPolicy("EditEmpTypesPolicy", policy => policy.RequireClaim("Edit Employee Types"));
        options.AddPolicy("DeleteEmpTypesPolicy", policy => policy.RequireClaim("Delete Employee Types"));

        //Exam Results
        options.AddPolicy("IndexExamResultsPolicy", policy => policy.RequireClaim("View Student-Wise Exam Result"));
        options.AddPolicy("SubjectWiseResultExamResultsPolicy", policy => policy.RequireClaim("View Subject-Wise Current Result"));
        options.AddPolicy("ClassWiseResultExamResultsPolicy", policy => policy.RequireClaim("View Class-Wise Current Result"));
        options.AddPolicy("ClassWiseResultAfterProcessExamResultsPolicy", policy => policy.RequireClaim("View Class-wise Processed Result"));
        options.AddPolicy("SubjectWiseResultAfterProcessExamResultsPolicy", policy => policy.RequireClaim("View Subject-wise Processed Result"));
        options.AddPolicy("StudentWiseResultAfterProcessExamResultsPolicy", policy => policy.RequireClaim("View Student-Wise Processed Result"));
        options.AddPolicy("DetailsExamResultsPolicy", policy => policy.RequireClaim("Delete Employee Types"));
        options.AddPolicy("CreateExamResultsPolicy", policy => policy.RequireClaim("Create Exam Result"));
        options.AddPolicy("EditExamResultsPolicy", policy => policy.RequireClaim("Edit Exam Result"));
        options.AddPolicy("ProcessResultExamResultsPolicy", policy => policy.RequireClaim("Process Result"));
        options.AddPolicy("UpdateRankingExamResultsPolicy", policy => policy.RequireClaim("Update Ranking"));
        options.AddPolicy("DeleteExamResultsPolicy", policy => policy.RequireClaim("Delete Exam"));
        options.AddPolicy("DeleteResultExamResultsPolicy", policy => policy.RequireClaim("Delete Results"));
        options.AddPolicy("LiveResultExamResultsPolicy", policy => policy.RequireClaim("Live Results"));

        //Expense Types
        options.AddPolicy("IndexExpensTypesPolicy", policy => policy.RequireClaim("View Expense Types"));
        options.AddPolicy("DetailsExpensTypesPolicy", policy => policy.RequireClaim("Details Expense Types"));
        options.AddPolicy("CreateExpensTypesPolicy", policy => policy.RequireClaim("Create Expense Types"));
        options.AddPolicy("EditExpensTypesPolicy", policy => policy.RequireClaim("Edit Expense Types"));
        options.AddPolicy("DeleteExpensTypesPolicy", policy => policy.RequireClaim("Delete Expense Types"));

        //Genders
        options.AddPolicy("IndexGendersPolicy", policy => policy.RequireClaim("View Genders"));
        options.AddPolicy("DetailsGendersPolicy", policy => policy.RequireClaim("Details Genders"));
        options.AddPolicy("CreateGendersPolicy", policy => policy.RequireClaim("Create Genders"));
        options.AddPolicy("EditGendersPolicy", policy => policy.RequireClaim("Edit Genders"));
        options.AddPolicy("DeleteGendersPolicy", policy => policy.RequireClaim("Delete Genders"));

        //Grading Table
        options.AddPolicy("IndexGradingTablePolicy", policy => policy.RequireClaim("View Grading Table"));
        options.AddPolicy("DetailsGradingTablePolicy", policy => policy.RequireClaim("Details Grading Table"));
        options.AddPolicy("CreateGradingTablePolicy", policy => policy.RequireClaim("Create Grading Table"));
        options.AddPolicy("EditGradingTablePolicy", policy => policy.RequireClaim("Edit Grading Table"));
        options.AddPolicy("DeleteGradingTablePolicy", policy => policy.RequireClaim("Delete Grading Table"));

        //Institute Information
        options.AddPolicy("IndexInstitutesPolicy", policy => policy.RequireClaim("View Institutes Info"));
        options.AddPolicy("CreateInstitutesPolicy", policy => policy.RequireClaim("Create Institutes Info"));
        options.AddPolicy("EditInstitutesPolicy", policy => policy.RequireClaim("Edit Institutes Info"));
        options.AddPolicy("SchoolTimeTableInstitutesPolicy", policy => policy.RequireClaim("Edit School Time Table"));

        //Nationalities
        options.AddPolicy("IndexNationalitiesPolicy", policy => policy.RequireClaim("View Nationalities"));
        options.AddPolicy("DetailsNationalitiesPolicy", policy => policy.RequireClaim("Details Nationalities"));
        options.AddPolicy("CreateNationalitiesPolicy", policy => policy.RequireClaim("Create Nationalities"));
        options.AddPolicy("EditNationalitiesPolicy", policy => policy.RequireClaim("Edit Nationalities"));
        options.AddPolicy("DeleteNationalitiesPolicy", policy => policy.RequireClaim("Delete Nationalities"));

        //Off Days
        options.AddPolicy("IndexOffDaysPolicy", policy => policy.RequireClaim("View Off Days"));
        options.AddPolicy("DetailsOffDaysPolicy", policy => policy.RequireClaim("Details Off Days"));
        options.AddPolicy("CreateOffDaysPolicy", policy => policy.RequireClaim("Create Off Days"));
        options.AddPolicy("EditOffDaysPolicy", policy => policy.RequireClaim("Edit Off Days"));
        options.AddPolicy("DeleteOffDaysPolicy", policy => policy.RequireClaim("Delete Off Days"));

        //Off Day Types
        options.AddPolicy("IndexOffDayTypesPolicy", policy => policy.RequireClaim("View Off Day Types"));
        options.AddPolicy("DetailsOffDayTypesPolicy", policy => policy.RequireClaim("Details Off Day Types"));
        options.AddPolicy("CreateOffDayTypesPolicy", policy => policy.RequireClaim("Create Off Day Types"));
        options.AddPolicy("EditOffDayTypesPolicy", policy => policy.RequireClaim("Edit Off Day Types"));
        options.AddPolicy("DeleteOffDayTypesPolicy", policy => policy.RequireClaim("Delete Off Day Types"));

        //Param Bus Config
        options.AddPolicy("IndexParamBusConfigPolicy", policy => policy.RequireClaim("View Config Data"));
        options.AddPolicy("UpSertParamBusConfigPolicy", policy => policy.RequireClaim("Create/Update Config Data"));

        //Project Module
        options.AddPolicy("IndexProjectModulesPolicy", policy => policy.RequireClaim("View Project Modules"));
        options.AddPolicy("DetailsProjectModulesPolicy", policy => policy.RequireClaim("Details Project Modules"));
        options.AddPolicy("CreateProjectModulesPolicy", policy => policy.RequireClaim("Create Project Modules"));
        options.AddPolicy("EditProjectModulesPolicy", policy => policy.RequireClaim("Edit Project Modules"));
        options.AddPolicy("DeleteProjectModulesPolicy", policy => policy.RequireClaim("Delete Project Modules"));

        //Project Sub Module
        options.AddPolicy("IndexProjectSubModulesPolicy", policy => policy.RequireClaim("View Project Sub Modules"));
        options.AddPolicy("DetailsProjectSubModulesPolicy", policy => policy.RequireClaim("Details Project Sub Modules"));
        options.AddPolicy("CreateProjectSubModulesPolicy", policy => policy.RequireClaim("Create Project Sub Modules"));
        options.AddPolicy("EditProjectSubModulesPolicy", policy => policy.RequireClaim("Edit Project Sub Modules"));
        options.AddPolicy("DeleteProjectSubModulesPolicy", policy => policy.RequireClaim("Delete Project Sub Modules"));

        //Phone SMS
        options.AddPolicy("IndexPhoneSMSPolicy", policy => policy.RequireClaim("View Phone SMS"));
        options.AddPolicy("CreatePhoneSMSPolicy", policy => policy.RequireClaim("Create Phone SMS"));
        options.AddPolicy("GetAPIDataPhoneSMSPolicy", policy => policy.RequireClaim("View SMS API Data"));

        //Question Banks
        options.AddPolicy("IndexQuestionBanksPolicy", policy => policy.RequireClaim("View Question Banks"));
        options.AddPolicy("AllQuestionQuestionBanksPolicy", policy => policy.RequireClaim("View All Question Bank"));
        options.AddPolicy("CreateQuestionBanksPolicy", policy => policy.RequireClaim("Create Question Bank"));
        options.AddPolicy("EditQuestionBanksPolicy", policy => policy.RequireClaim("Edit Question Bank"));

        //Question Formation
        options.AddPolicy("IndexQuestionFormationPolicy", policy => policy.RequireClaim("View Question Formation"));
        options.AddPolicy("CreateQuestionFormationPolicy", policy => policy.RequireClaim("Create Question Formation"));
        options.AddPolicy("EditQuestionFormationPolicy", policy => policy.RequireClaim("Edit Question Formation"));

        //Religions
        options.AddPolicy("IndexReligionsPolicy", policy => policy.RequireClaim("View Religions"));
        options.AddPolicy("DetailsReligionsPolicy", policy => policy.RequireClaim("Details Religions"));
        options.AddPolicy("CreateReligionsPolicy", policy => policy.RequireClaim("Create Religions"));
        options.AddPolicy("EditReligionsPolicy", policy => policy.RequireClaim("Edit Religions"));
        options.AddPolicy("DeleteReligionsPolicy", policy => policy.RequireClaim("Delete Religions"));

        //Reports
        options.AddPolicy("StudentsReportsPolicy", policy => policy.RequireClaim("View Student List Report"));
        options.AddPolicy("DailyAttendanceReportsPolicy", policy => policy.RequireClaim("View Daily Attendance Report"));
        options.AddPolicy("AttendanceReportsPolicy", policy => policy.RequireClaim("View Attendance Report"));
        options.AddPolicy("SubjectWiseMarkSheetReportsPolicy", policy => policy.RequireClaim("View Subject-Wise Marksheet Report"));
        options.AddPolicy("StudentWiseMarkSheetReportsPolicy", policy => policy.RequireClaim("View Studetn-Wise Marksheet Report"));
        options.AddPolicy("StudentPaymentInfoReportsPolicy", policy => policy.RequireClaim("View Student Payment Details Report"));
        options.AddPolicy("StudentPaymentReportsPolicy", policy => policy.RequireClaim("View Student payment Report"));
        options.AddPolicy("ReceiptPaymentReportsPolicy", policy => policy.RequireClaim("View Payment Receipt Report"));
        options.AddPolicy("AdmitCardReportsPolicy", policy => policy.RequireClaim("View Admit card Report"));
        options.AddPolicy("SeatPlanReportsPolicy", policy => policy.RequireClaim("View Seat Plan Report"));

        //User Roles
        options.AddPolicy("IndexRolesPolicy", policy => policy.RequireClaim("View Roles"));
        options.AddPolicy("CreateRolesPolicy", policy => policy.RequireClaim("Create Roles"));
        options.AddPolicy("AssignRolesPolicy", policy => policy.RequireClaim("Assign Roles"));

        //Setup Control
        options.AddPolicy("SMSControlSetupPolicy", policy => policy.RequireClaim("SMS Control"));
        options.AddPolicy("StudentWiseSMSServiceSetupPolicy", policy => policy.RequireClaim("Student-Wise SMS Service Setup"));

        //Student Fee Allocation
        options.AddPolicy("IndexStudentFeeAllocationsPolicy", policy => policy.RequireClaim("View Student Fee Allocations"));
        options.AddPolicy("DetailsStudentFeeAllocationsPolicy", policy => policy.RequireClaim("Details Student Fee Allocations"));
        options.AddPolicy("CreateStudentFeeAllocationsPolicy", policy => policy.RequireClaim("Create Student Fee Allocations"));
        options.AddPolicy("EditStudentFeeAllocationsPolicy", policy => policy.RequireClaim("Edit Student Fee Allocations"));
        options.AddPolicy("DeleteStudentFeeAllocationsPolicy", policy => policy.RequireClaim("Delete Student Fee Allocations"));
        options.AddPolicy("GroupStudentFeeAllocationsPolicy", policy => policy.RequireClaim("Add Student Fee Allocations Group"));

        //Student Fee Head
        options.AddPolicy("IndexStudentFeeHeadsPolicy", policy => policy.RequireClaim("View Student Fee Heads"));
        options.AddPolicy("DetailsStudentFeeHeadsPolicy", policy => policy.RequireClaim("Details Student Fee Head"));
        options.AddPolicy("CreateStudentFeeHeadsPolicy", policy => policy.RequireClaim("Create Student Fee Head"));
        options.AddPolicy("EditStudentFeeHeadsPolicy", policy => policy.RequireClaim("Edit Student Fee Head"));
        options.AddPolicy("DeleteStudentFeeHeadsPolicy", policy => policy.RequireClaim("Delete Student Fee Head"));

        //Student Payment Details
        options.AddPolicy("IndexStudentPaymentDetailsPolicy", policy => policy.RequireClaim("View Student Payment Details"));
        options.AddPolicy("DetailsStudentPaymentDetailsPolicy", policy => policy.RequireClaim("Details Student Payment Details"));
        options.AddPolicy("CreateStudentPaymentDetailsPolicy", policy => policy.RequireClaim("Create Student Payment Details"));
        options.AddPolicy("EditStudentPaymentDetailsPolicy", policy => policy.RequireClaim("Edit Student Payment Details"));
        options.AddPolicy("DeleteStudentPaymentDetailsPolicy", policy => policy.RequireClaim("Delete Student Payment Details"));

        //Student Payments
        options.AddPolicy("IndexStudentPaymentsPolicy", policy => policy.RequireClaim("View Student Payment"));
        options.AddPolicy("PaymentStudentPaymentsPolicy", policy => policy.RequireClaim("Details Student Payment"));
        options.AddPolicy("CreateStudentPaymentsPolicy", policy => policy.RequireClaim("Create Student Payment"));
        options.AddPolicy("EditStudentPaymentsPolicy", policy => policy.RequireClaim("Edit Student Payment"));
        options.AddPolicy("DeleteStudentPaymentsPolicy", policy => policy.RequireClaim("Delete Student Payment"));
        options.AddPolicy("DuePaymentStudentPaymentsPolicy", policy => policy.RequireClaim("Student Due Payment"));
        options.AddPolicy("DueAmountStudentsPolicy", policy => policy.RequireClaim("View Due Amount"));
        options.AddPolicy("PreviousDuePaymentStudentPaymentsPolicy", policy => policy.RequireClaim("View Previous Due Amount"));

        //Students
        options.AddPolicy("IndexStudentsPolicy", policy => policy.RequireClaim("View Student List"));
        options.AddPolicy("DetailsStudentsPolicy", policy => policy.RequireClaim("Details Student"));
        options.AddPolicy("CreateStudentsPolicy", policy => policy.RequireClaim("Create Student"));
        options.AddPolicy("BulkUploadStudentsPolicy", policy => policy.RequireClaim("Bulk Upload Students"));
        options.AddPolicy("EditStudentsPolicy", policy => policy.RequireClaim("Edit Student"));
        options.AddPolicy("DeleteStudentsPolicy", policy => policy.RequireClaim("Delete Student"));
        options.AddPolicy("ProfileStudentsPolicy", policy => policy.RequireClaim("View Student Profile"));

        //Student Enrollment
        options.AddPolicy("SubjectEnrollSubjectEnrollmentPolicy", policy => policy.RequireClaim("View Enrollment List"));
        options.AddPolicy("SetOptionalSubjectSubjectEnrollmentPolicy", policy => policy.RequireClaim("Update Optional Subject"));

        //Tickets
        options.AddPolicy("IndexTicketsPolicy", policy => policy.RequireClaim("View Tickets"));
        options.AddPolicy("DetailsTicketsPolicy", policy => policy.RequireClaim("View Details Ticket"));
        options.AddPolicy("CreateTicketsPolicy", policy => policy.RequireClaim("Create Ticket"));
        options.AddPolicy("EditTicketsPolicy", policy => policy.RequireClaim("Edit Ticket"));
        options.AddPolicy("DeleteTicketsPolicy", policy => policy.RequireClaim("Delete Ticket"));
        options.AddPolicy("ChangeStatusTicketsPolicy", policy => policy.RequireClaim("Change Ticket Status"));
        options.AddPolicy("CommentTicketsPolicy", policy => policy.RequireClaim("Comment On Ticket"));

        //Upazila
        options.AddPolicy("IndexUpazilasPolicy", policy => policy.RequireClaim("View Upazilas"));
        options.AddPolicy("DetailsUpazilasPolicy", policy => policy.RequireClaim("View Details Upazila"));
        options.AddPolicy("CreateUpazilasPolicy", policy => policy.RequireClaim("Create Upazila"));
        options.AddPolicy("EditUpazilasPolicy", policy => policy.RequireClaim("Edit Upazila"));
        options.AddPolicy("DeleteUpazilasPolicy", policy => policy.RequireClaim("Delete Upazila"));
    }
}
