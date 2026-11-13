/*  EIMS - bootstrap permissions for an admin account (SQL Server).

    Why this is needed
    ------------------
    DbSeeder.cs creates admin@eims.com with the SuperAdmin + Admin ROLES but grants
    no CLAIMS. Every policy in AuthorizationPolicies.cs is RequireClaim(...), matched
    against AspNetUserClaims. A user with roles but no claims logs in successfully and
    is then denied on every page, with no UI route out: ClaimStores/Index and
    Administrations/UserProfile are themselves claim-gated.

    What it does
    ------------
    Part 1  Seeds ProjectModules / ProjectSubModules / ClaimStores - ONLY on a database
            where ClaimStores is empty (a fresh EnsureCreatedAsync() install).
    Part 2  Grants every ClaimStores claim to @TargetEmail.
    Part 3  Grants all 243 claim types required by AuthorizationPolicies.cs. Many have
            no ClaimStores row on any database (the whole user-management area among
            them), so Part 2 alone can never reach them. A policy only reads
            AspNetUserClaims, so granting directly unlocks the page - it just won't
            appear in the Administrations/UserProfile picker.

    Idempotent - safe to re-run. Set @TargetEmail below to choose the account.
    LOG OUT AND BACK IN afterwards: claims are baked into the auth cookie at sign-in.
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @TargetEmail nvarchar(256) = N'admin@eims.com';   -- <<< change if needed

DECLARE @AdminId nvarchar(450) = (SELECT Id FROM AspNetUsers WHERE Email = @TargetEmail);
IF @AdminId IS NULL
BEGIN
    RAISERROR('User %s not found. Start the app once so DbSeeder creates it, then re-run.', 16, 1, @TargetEmail);
    RETURN;
END

BEGIN TRANSACTION;

/* ---- Part 1: reference data (only on an empty database) ---- */
IF NOT EXISTS (SELECT 1 FROM ClaimStores)
BEGIN
    PRINT 'ClaimStores empty - seeding reference data...';
    SET IDENTITY_INSERT ProjectModules ON;
    INSERT INTO ProjectModules (Id,ModuleName,Status,Remarks,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (1,N'Accounts',1,N'Accounting related all of the working portion',N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ProjectModules (Id,ModuleName,Status,Remarks,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (2,N'Academic',1,N'Academic working options',N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ProjectModules (Id,ModuleName,Status,Remarks,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (3,N'Administration',1,N'Administrative work',N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ProjectModules (Id,ModuleName,Status,Remarks,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (6,N'Security',1,N'Various types of security module',N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ProjectModules (Id,ModuleName,Status,Remarks,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (7,N'Reporting',1,N'Report Related works',N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ProjectModules (Id,ModuleName,Status,Remarks,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (8,N'Exam_Result',1,N'Examination and Result Related',N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ProjectModules (Id,ModuleName,Status,Remarks,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (9,N'ApplicationSetup',1,N'Application Related various setup',N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ProjectModules (Id,ModuleName,Status,Remarks,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (10,N'StudentManagement',1,N'Student Related Info',N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ProjectModules (Id,ModuleName,Status,Remarks,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (11,N'HR',1,N'Human and Resource',N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ProjectModules (Id,ModuleName,Status,Remarks,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (12,N'Attendance',1,N'Attendance Related',N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ProjectModules (Id,ModuleName,Status,Remarks,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (13,N'Finance',1,N'Finance and Accounts',N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ProjectModules (Id,ModuleName,Status,Remarks,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (14,N'Communication',1,N'',N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ProjectModules (Id,ModuleName,Status,Remarks,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (15,N'Ticket',1,N'User tickets raised for the developer',N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    SET IDENTITY_INSERT ProjectModules OFF;
    SET IDENTITY_INSERT ProjectSubModules ON;
    INSERT INTO ProjectSubModules (Id,SubModuleName,ProjectModuleId,Status,Remarks,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (3,N'Payment Collection',1,1,N'Payment Collection related work',N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ProjectSubModules (Id,SubModuleName,ProjectModuleId,Status,Remarks,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (4,N'Expense',1,1,N'Any kind of Expenses',N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ProjectSubModules (Id,SubModuleName,ProjectModuleId,Status,Remarks,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (8,N'User Profile',6,1,N'User profile related sub modules',N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ProjectSubModules (Id,SubModuleName,ProjectModuleId,Status,Remarks,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (9,N'Academic Class',2,1,N'Academic Class CRUD Related Operation',N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ProjectSubModules (Id,SubModuleName,ProjectModuleId,Status,Remarks,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (10,N'Module/Sub-Module',6,1,N'Projct Module and Sub-Module Related Task',N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ProjectSubModules (Id,SubModuleName,ProjectModuleId,Status,Remarks,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (11,N'Attendance Reporting',7,1,N'Attendance Related Report',N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ProjectSubModules (Id,SubModuleName,ProjectModuleId,Status,Remarks,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (12,N'Payment Report',7,1,N'Payment Related Report',N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ProjectSubModules (Id,SubModuleName,ProjectModuleId,Status,Remarks,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (13,N'User Claim',6,1,N'Uer Claim Related Task',N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ProjectSubModules (Id,SubModuleName,ProjectModuleId,Status,Remarks,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (14,N'Students Reporting',7,1,N'Student Related all reports',N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ProjectSubModules (Id,SubModuleName,ProjectModuleId,Status,Remarks,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (15,N'Employee Report',7,1,N'Employee Related All Reporting',N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ProjectSubModules (Id,SubModuleName,ProjectModuleId,Status,Remarks,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (16,N'Other''s Report',7,1,N'All of the reporting which have no sub-modules',N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ProjectSubModules (Id,SubModuleName,ProjectModuleId,Status,Remarks,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (17,N'Fee Head',1,1,N'Payment Related Setup',N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ProjectSubModules (Id,SubModuleName,ProjectModuleId,Status,Remarks,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (18,N'Designation Setup',3,1,N'Designation Control or Setup related task',N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ProjectSubModules (Id,SubModuleName,ProjectModuleId,Status,Remarks,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (19,N'Academic Subject',2,1,N'Subject Related tasks',N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ProjectSubModules (Id,SubModuleName,ProjectModuleId,Status,Remarks,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (20,N'Institute Information',3,1,N'Institute Related Information',N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ProjectSubModules (Id,SubModuleName,ProjectModuleId,Status,Remarks,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (21,N'Exam Group',8,1,N'Examination Group',N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ProjectSubModules (Id,SubModuleName,ProjectModuleId,Status,Remarks,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (22,N'Result',8,1,N'Result Related Info',N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ProjectSubModules (Id,SubModuleName,ProjectModuleId,Status,Remarks,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (23,N'Exam Setup',8,1,N'Examination Setup Related',N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ProjectSubModules (Id,SubModuleName,ProjectModuleId,Status,Remarks,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (24,N'Exams',8,1,N'All of the Examination and lists',N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ProjectSubModules (Id,SubModuleName,ProjectModuleId,Status,Remarks,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (25,N'Employee',3,1,N'Employee Related',N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ProjectSubModules (Id,SubModuleName,ProjectModuleId,Status,Remarks,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (26,N'Students',2,1,N'Students Basic Operations',N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ProjectSubModules (Id,SubModuleName,ProjectModuleId,Status,Remarks,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (27,N'Settings',3,1,N'Various types of settings',N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ProjectSubModules (Id,SubModuleName,ProjectModuleId,Status,Remarks,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (28,N'Phone SMS',3,1,N'SMS Read Write',N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ProjectSubModules (Id,SubModuleName,ProjectModuleId,Status,Remarks,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (29,N'Exam Result',7,1,N'Result Related All Report',N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ProjectSubModules (Id,SubModuleName,ProjectModuleId,Status,Remarks,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (30,N'Class Fee Setup',1,1,N'Only Class Fee releted',N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ProjectSubModules (Id,SubModuleName,ProjectModuleId,Status,Remarks,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (31,N'Academic Section',2,1,N'Section Related all info',N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ProjectSubModules (Id,SubModuleName,ProjectModuleId,Status,Remarks,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (32,N'Academic Session',2,1,N'Academic Session Related all work',N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ProjectSubModules (Id,SubModuleName,ProjectModuleId,Status,Remarks,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (33,N'Subject Type',2,1,N'Academic Subject Type Changes',N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ProjectSubModules (Id,SubModuleName,ProjectModuleId,Status,Remarks,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (34,N'Subject Allocation',2,1,N'Class wise Subject Allocation',N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ProjectSubModules (Id,SubModuleName,ProjectModuleId,Status,Remarks,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (35,N'Attendance',3,1,N'Attendance Related Module',N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ProjectSubModules (Id,SubModuleName,ProjectModuleId,Status,Remarks,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (36,N'Grading System',2,0,N'Exam Grading Related',N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ProjectSubModules (Id,SubModuleName,ProjectModuleId,Status,Remarks,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (37,N'Holidays',3,1,N'Holiday related Configurations',N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ProjectSubModules (Id,SubModuleName,ProjectModuleId,Status,Remarks,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (38,N'Fee Allocation',1,1,N'To Allocate any fee to any students',N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ProjectSubModules (Id,SubModuleName,ProjectModuleId,Status,Remarks,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (40,N'AttendanceMachinesSetup',12,1,N'',N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ProjectSubModules (Id,SubModuleName,ProjectModuleId,Status,Remarks,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (41,N'TicketDesk',15,1,N'Ticket desk',N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    SET IDENTITY_INSERT ProjectSubModules OFF;
    SET IDENTITY_INSERT ClaimStores ON;
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (1,N'Create Student Payment',N'CreateStudentPaymentsPolicy',3,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (4,N'View User Profile',N'UserProfileGetAdministrationsPolicy',8,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (5,N'Edit User Profile',N'UserProfilePostAdministrationsPolicy',8,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (6,N'View Academic Class',N'IndexAcademicClassesPolicy',9,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (7,N'View Claim Store',N'IndexClaimStoresPolicy',13,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (8,N'View Project Modules',N'IndexProjectModulesPolicy',10,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (9,N'View Project Sub Modules',N'IndexProjectSubModulesPolicy',8,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (10,N'Create Project Modules',N'CreateProjectModulesPolicy',10,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (11,N'Edit Project Modules',N'EditProjectModulesPolicy',8,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (12,N'Delete Project Modules',N'DeleteProjectModulesPolicy',10,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (13,N'Create Project Sub Modules',N'CreateProjectSubModulesPolicy',10,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (14,N'Create Claim Store',N'CreateClaimStoresPolicy',13,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (15,N'Edit Claim Store',N'EditClaimStoresPolicy',13,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (16,N'Delete Claim Store',N'DeleteClaimStoresPolicy',13,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (17,N'View Attendances',N'IndexAttendanceMachinesPolicy',11,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (18,N'View Student Payment',N'IndexStudentPaymentsPolicy',3,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (19,N'Details Student Payment',N'PaymentStudentPaymentsPolicy',3,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (20,N'Edit Student Payment',N'EditStudentPaymentsPolicy',3,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (21,N'Delete Student Payment',N'DeleteStudentPaymentsPolicy',3,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (22,N'Student Due Payment',N'DuePaymentStudentPaymentsPolicy',3,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (23,N'View Student Fee Heads',N'IndexStudentFeeHeadsPolicy',17,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (24,N'Details Student Fee Head',N'DetailsStudentFeeHeadsPolicy',17,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (25,N'Create Student Fee Allocations',N'CreateStudentFeeAllocationsPolicy',38,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (26,N'View Designations',N'IndexDesignationsPolicy',18,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (27,N'View Details Designations',N'DetailsDesignationsPolicy',18,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (28,N'Create Designations',N'CreateDesignationsPolicy',18,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (29,N'View Academic Subject',N'IndexAcademicSubjectPolicy',19,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (30,N'Details Academic Subject',N'DetailsAcademicSubjectPolicy',19,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (31,N'Create Academic Subject',N'CreateAcademicSubjectPolicy',19,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (32,N'Edit Academic Subject',N'EditAcademicSubjectPolicy',19,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (33,N'View Student List Report',N'StudentsReportsPolicy',14,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (34,N'View Daily Attendance Report',N'DailyAttendanceReportsPolicy',11,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (35,N'View Attendance Report',N'AttendanceReportsPolicy',11,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (36,N'View Admit card Report',N'AdmitCardReportsPolicy',16,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (37,N'View Payment Receipt Report',N'ReceiptPaymentReportsPolicy',12,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (38,N'View Student payment Report',N'StudentPaymentReportsPolicy',12,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (39,N'View Student Payment Details Report',N'StudentPaymentInfoReportsPolicy',12,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (40,N'View Subject-Wise Marksheet Report',N'SubjectWiseMarkSheetReportsPolicy',29,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (41,N'View Institutes Info',N'IndexInstitutesPolicy',20,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (42,N'Create Institutes Info',N'CreateInstitutesPolicy',20,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (43,N'Edit Institutes Info',N'EditInstitutesPolicy',20,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (44,N'Edit School Time Table',N'SchoolTimeTableInstitutesPolicy',20,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (45,N'View Academic Exam Group',N'IndexAcademicExamGroupPolicy',21,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (46,N'Details Academic Exam Group',N'DetailsAcademicExamGroupPolicy',21,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (47,N'Create Academic Exam Group',N'CreateAcademicExamGroupPolicy',21,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (48,N'Edit Academic Exam Group',N'EditAcademicExamGroupPolicy',21,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (49,N'Delete Academic Exam Group',N'DeleteAcademicExamGroupPolicy',21,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (50,N'Edit Project Sub Modules',N'EditProjectSubModulesPolicy',10,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (51,N'View Academic Exam',N'IndexAcademicExamPolicy',24,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (54,N'Details Academic Exam',N'DetailsAcademicExamPolicy',24,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (55,N'Create Academic Exam',N'CreateAcademicExamPolicy',24,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (56,N'Edit Academic Exam',N'EditAcademicExamPolicy',24,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (57,N'Delete Academic Exam Group',N'DeleteAcademicExamPolicy',24,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (58,N'Submit Academic Exam Marks',N'ExamMarkSubmitAcademicExamPolicy',24,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (59,N'Report Admit Card',N'AdmitCardAcademicExamPolicy',24,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (60,N'Lock Academic Exam',N'LockAcademicExamPolicy',24,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (61,N'View Academic Exam Type',N'IndexAcademicExamTypePolicy',23,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (62,N'Details Academic Exam Type',N'DetailsAcademicExamTypePolicy',23,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (63,N'Create Academic Exam Type',N'CraeteAcademicExamTypePolicy',23,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (64,N'Edit Academic Exam Type',N'EditAcademicExamTypePolicy',23,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (65,N'Delete Academic Exam Type',N'DeleteAcademicExamTypePolicy',23,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (66,N'View Student-Wise Exam Result',N'IndexExamResultsPolicy',22,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (67,N'View Subject-Wise Current Result',N'SubjectWiseResultExamResultsPolicy',22,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (68,N'View Class-Wise Current Result',N'ClassWiseResultExamResultsPolicy',22,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (69,N'View Class-wise Processed Result',N'ClassWiseResultAfterProcessExamResultsPolicy',22,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (70,N'View Subject-wise Processed Result',N'SubjectWiseResultAfterProcessExamResultsPolicy',22,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (71,N'View Student-Wise Processed Result',N'StudentWiseResultAfterProcessExamResultsPolicy',22,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (72,N'Delete Employee Types',N'DetailsExamResultsPolicy',22,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (73,N'Create Exam Result',N'CreateExamResultsPolicy',22,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (74,N'Edit Exam Result',N'EditExamResultsPolicy',22,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (75,N'Process Result',N'ProcessResultExamResultsPolicy',22,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (76,N'Update Ranking',N'UpdateRankingExamResultsPolicy',22,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (77,N'Delete Exam',N'DeleteExamResultsPolicy',22,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (78,N'Delete Results',N'DeleteResultExamResultsPolicy',22,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (79,N'View Employee',N'IndexEmployeesPolicy',25,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (80,N'Details Employee',N'DetailsEmployeesPolicy',25,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (81,N'Create Employee',N'CreateEmployeesPolicy',25,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (82,N'Edit Employee',N'EditEmployeesPolicy',25,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (83,N'Delete Employee',N'DeleteEmployeesPolicy',25,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (84,N'View Student List',N'IndexStudentsPolicy',26,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (85,N'Details Student',N'DetailsStudentsPolicy',26,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (86,N'Create Student',N'CreateStudentsPolicy',26,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (87,N'Edit Student',N'EditStudentsPolicy',26,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (88,N'SMS Control',N'SMSControlSetupPolicy',27,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (89,N'Student-Wise SMS Service Setup',N'StudentWiseSMSServiceSetupPolicy',27,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (90,N'View Phone SMS',N'IndexPhoneSMSPolicy',28,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (91,N'Create Phone SMS',N'CreatePhoneSMSPolicy',28,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (92,N'Edit Student Fee Head',N'EditStudentFeeHeadsPolicy',17,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (94,N'Create Class Fee Lists',N'CreateClassFeeListsPolicy',30,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (96,N'View Studetn-Wise Marksheet Report',N'StudentWiseMarkSheetReportsPolicy',29,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (97,N'Create Student Fee Head',N'CreateStudentFeeHeadsPolicy',17,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (98,N'View Class Fee Lists',N'IndexClassFeeListsPolicy',30,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (99,N'Details Class Fee Lists',N'DetailsClassFeeListsPolicy',30,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (101,N'Edit Class Fee Lists',N'EditClassFeeListsPolicy',30,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (102,N'Delete Class Fee Lists',N'DeleteClassFeeListsPolicy',30,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (103,N'Details View Academic Class',N'DetailsAcademicClassesPolicy',9,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (104,N'Create Academic Class',N'CreateAcademicClassesPolicy',9,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (105,N'Edit Academic Class',N'EditAcademicClassesPolicy',9,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (106,N'View Academic Section',N'IndexAcademicSectionPolicy',31,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (107,N'Details Academic Section',N'DetailsAcademicSectionPolicy',31,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (108,N'Create Academic Section',N'CreateAcademicSectionPolicy',31,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (109,N'Edit Academic Section',N'EditAcademicSectionPolicy',31,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (110,N'Delete Academic Section',N'DeleteAcademicSectionPolicy',31,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (111,N'View Academic Session',N'IndexAcademicSessionPolicy',32,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (112,N'Create Academic Session',N'CreateAcademicSessionPolicy',32,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (117,N'View Subject Type',N'IndexAcademicSubjectTypesPolicy',33,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (118,N'Details Subject Type',N'DetailsAcademicSubjectTypesPolicy',33,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (119,N'Create  Subject Type',N'CreateAcademicSubjectTypesPolicy',33,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (120,N'Edit Subject Type',N'EditAcademicSubjectTypesPolicy',33,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (121,N'Delete Subject Type',N'DeleteAcademicSubjectTypesPolicy',33,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (122,N'View Class-wise Subject Allocation',N'ViewClassWiseSubjectAllocationAcademicSubjectPolicy',34,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (123,N'Create Class-wise Subject Allocation',N'CreateClassWiseSubjectAllocationAcademicSubjectPolicy',34,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (124,N'Delete Class-wise Subject Allocation',N'DeleteClassWiseSubjectAllocationAcademicSubjectPolicy',34,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (125,N'View Config Data',N'IndexParamBusConfigPolicy',27,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (126,N'Create/Update Config Data',N'UpSertParamBusConfigPolicy',27,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (128,N'View Details Attendance',N'DetailsAttendanceMachinesPolicy',35,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (129,N'Create Attendance',N'CreateAttendanceMachinesPolicy',35,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (130,N'Edit Attendance',N'EditAttendanceMachinesPolicy',35,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (131,N'Delete Attendance',N'DeleteAttendanceMachinesPolicy',35,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (132,N'View Grading Table',N'IndexGradingTablePolicy',36,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (133,N'Details Grading Table',N'DetailsGradingTablePolicy',36,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (134,N'Create Grading Table',N'CreateGradingTablePolicy',36,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (135,N'Edit Grading Table',N'EditGradingTablePolicy',36,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (136,N'Delete Grading Table',N'DeleteGradingTablePolicy',36,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (137,N'View Off Days',N'IndexOffDaysPolicy',37,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (138,N'Create Off Days',N'CreateOffDaysPolicy',37,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (139,N'Edit Off Days',N'EditOffDaysPolicy',37,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (140,N'Delete Off Days',N'DeleteOffDaysPolicy',37,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (141,N'View Student Fee Allocations',N'ViewStudentFeeAllocationsPolicy',38,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (142,N'Delete Student Fee Allocations',N'DeleteStudentFeeAllocationsPolicy',38,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (143,N'Edit Student Fee Allocations',N'EditStudentFeeAllocationsPolicy',38,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (144,N'Edit Academic Session',N'EditAcademicSessionPolicy',32,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (145,N'Details Academic Session',N'DetailsAcademicSessionPolicy',32,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (146,N'Delete Academic Session',N'DeleteAcademicSessionPolicy',32,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (148,N'View User Role',N'ViewRolesAccountsPolicy',27,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (149,N'Add or Remove User From Role',N'AddOrRemoveUserAccountsPolicy',27,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (150,N'Edit User Role',N'EditRoleAccountsPolicy',27,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (151,N'Add Student Fee Allocations Group',N'GroupStudentFeeAllocationsPolicy',38,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (152,N'Edit Designations',N'EditDesignationsPolicy',18,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (153,N'Live Results',N'LiveResultExamResultsPolicy',22,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (154,N'View Previous Due Amount',N'PreviousDuePaymentStudentPaymentsPolicy',3,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (155,N'View User Profile',N'ViewUserProfileAdministrationsPolicy',27,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (156,N'Register New User',N'RegisterAccountsPolicy',27,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (157,N'View SMS API Data',N'GetAPIDataPhoneSMSPolicy',28,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (158,N'View Enrollment List',N'SubjectEnrollSubjectEnrollmentPolicy',19,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (159,N'Update Optional Subject',N'SetOptionalSubjectSubjectEnrollmentPolicy',19,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (161,N'View Attendance Machines',N'IndexAttendanceMachineDevicesPolicy',40,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (162,N'View Details Attendance Machines',N'DetailsAttendanceMachineDevicesPolicy',40,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (163,N'Create Attendance Machines',N'CreateAttendanceMachineDevicesPolicy',40,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (164,N'Edit Attendance Machines',N'EditAttendanceMachineDevicesPolicy',40,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (165,N'Delete Attendance Machines',N'DeleteAttendanceMachineDevicesPolicy',40,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (166,N'Manage Machine Users',N'ManageMachineUsersPolicy',40,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (167,N'View Tickets',N'IndexTicketsPolicy',41,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (168,N'View Details Ticket',N'DetailsTicketsPolicy',41,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (169,N'Create Ticket',N'CreateTicketsPolicy',41,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (170,N'Edit Ticket',N'EditTicketsPolicy',41,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (171,N'Delete Ticket',N'DeleteTicketsPolicy',41,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (172,N'Change Ticket Status',N'ChangeStatusTicketsPolicy',41,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    INSERT INTO ClaimStores (Id,ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress) VALUES (173,N'Comment On Ticket',N'CommentTicketsPolicy',41,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    SET IDENTITY_INSERT ClaimStores OFF;
END
ELSE
    PRINT 'ClaimStores already populated - skipping reference seed.';

/* ---- Part 2: grant everything listed in ClaimStores ---- */
INSERT INTO AspNetUserClaims (UserId, ClaimType, ClaimValue)
SELECT @AdminId, x.ClaimType, x.ClaimValue
FROM (SELECT ClaimType, MIN(ClaimValue) AS ClaimValue FROM ClaimStores GROUP BY ClaimType) x
WHERE NOT EXISTS (
    SELECT 1 FROM AspNetUserClaims uc
    WHERE uc.UserId = @AdminId AND uc.ClaimType = x.ClaimType);
DECLARE @FromStore int = @@ROWCOUNT;

/* ---- Part 3: every claim type AuthorizationPolicies.cs requires ---- */
DECLARE @Policy TABLE (ClaimType nvarchar(450) PRIMARY KEY);
INSERT INTO @Policy (ClaimType) VALUES
    (N'Add or Remove User From Role'),
    (N'Add Student Fee Allocations Group'),
    (N'Assign Roles'),
    (N'Change Ticket Status'),
    (N'Comment On Ticket'),
    (N'Create  Blood Groups'),
    (N'Create  Subject Type'),
    (N'Create Academic Class'),
    (N'Create Academic Exam'),
    (N'Create Academic Exam Group'),
    (N'Create Academic Exam Type'),
    (N'Create Academic Section'),
    (N'Create Academic Session'),
    (N'Create Academic Subject'),
    (N'Create Attendance'),
    (N'Create Attendance Machines'),
    (N'Create Claim Store'),
    (N'Create Class Fee Lists'),
    (N'Create Class-wise Subject Allocation'),
    (N'Create Designation Types'),
    (N'Create Designations'),
    (N'Create Districts'),
    (N'Create Divisions'),
    (N'Create Employee'),
    (N'Create Employee Types'),
    (N'Create Exam Result'),
    (N'Create Expense Types'),
    (N'Create Genders'),
    (N'Create Grading Table'),
    (N'Create Institutes Info'),
    (N'Create Nationalities'),
    (N'Create Off Day Types'),
    (N'Create Off Days'),
    (N'Create Phone SMS'),
    (N'Create Project Modules'),
    (N'Create Project Sub Modules'),
    (N'Create Question Bank'),
    (N'Create Question Formation'),
    (N'Create Religions'),
    (N'Create Roles'),
    (N'Create Student'),
    (N'Create Student Fee Allocations'),
    (N'Create Student Fee Head'),
    (N'Create Student Payment'),
    (N'Create Student Payment Details'),
    (N'Create Ticket'),
    (N'Create Upazila'),
    (N'Create User Role'),
    (N'Create/Update Config Data'),
    (N'Delete Academic Exam Group'),
    (N'Delete Academic Exam Type'),
    (N'Delete Academic Section'),
    (N'Delete Academic Session'),
    (N'Delete Academic Subject'),
    (N'Delete Attendance'),
    (N'Delete Attendance Machines'),
    (N'Delete Blood Groups'),
    (N'Delete Claim Store'),
    (N'Delete Class Fee Lists'),
    (N'Delete Class-wise Subject Allocation'),
    (N'Delete Designation Types'),
    (N'Delete Designations'),
    (N'Delete Districts'),
    (N'Delete Divisions'),
    (N'Delete Employee'),
    (N'Delete Employee Types'),
    (N'Delete Exam'),
    (N'Delete Expense Types'),
    (N'Delete Genders'),
    (N'Delete Grading Table'),
    (N'Delete Nationalities'),
    (N'Delete Off Day Types'),
    (N'Delete Off Days'),
    (N'Delete Project Modules'),
    (N'Delete Project Sub Modules'),
    (N'Delete Religions'),
    (N'Delete Results'),
    (N'Delete Student'),
    (N'Delete Student Fee Allocations'),
    (N'Delete Student Fee Head'),
    (N'Delete Student Payment'),
    (N'Delete Student Payment Details'),
    (N'Delete Subject Type'),
    (N'Delete Ticket'),
    (N'Delete Upazila'),
    (N'Delete User Account'),
    (N'Details Academic Exam'),
    (N'Details Academic Exam Group'),
    (N'Details Academic Exam Type'),
    (N'Details Academic Section'),
    (N'Details Academic Session'),
    (N'Details Academic Subject'),
    (N'Details Blood Groups'),
    (N'Details Class Fee Lists'),
    (N'Details Designation Types'),
    (N'Details Districts'),
    (N'Details Divisions'),
    (N'Details Employee'),
    (N'Details Employee Types'),
    (N'Details Expense Types'),
    (N'Details Genders'),
    (N'Details Grading Table'),
    (N'Details Nationalities'),
    (N'Details Off Day Types'),
    (N'Details Off Days'),
    (N'Details Project Modules'),
    (N'Details Project Sub Modules'),
    (N'Details Religions'),
    (N'Details Student'),
    (N'Details Student Fee Allocations'),
    (N'Details Student Fee Head'),
    (N'Details Student Payment'),
    (N'Details Student Payment Details'),
    (N'Details Subject Type'),
    (N'Details View Academic Class'),
    (N'Edit Academic Class'),
    (N'Edit Academic Exam'),
    (N'Edit Academic Exam Group'),
    (N'Edit Academic Exam Type'),
    (N'Edit Academic Section'),
    (N'Edit Academic Session'),
    (N'Edit Academic Subject'),
    (N'Edit Attendance'),
    (N'Edit Attendance Machines'),
    (N'Edit Blood Groups'),
    (N'Edit Chapter'),
    (N'Edit Claim Store'),
    (N'Edit Class Fee Lists'),
    (N'Edit Designation Types'),
    (N'Edit Designations'),
    (N'Edit Districts'),
    (N'Edit Divisions'),
    (N'Edit Employee'),
    (N'Edit Employee Types'),
    (N'Edit Exam Result'),
    (N'Edit Expense Types'),
    (N'Edit Genders'),
    (N'Edit Grading Table'),
    (N'Edit Institutes Info'),
    (N'Edit Nationalities'),
    (N'Edit Off Day Types'),
    (N'Edit Off Days'),
    (N'Edit Project Modules'),
    (N'Edit Project Sub Modules'),
    (N'Edit Question Bank'),
    (N'Edit Question Formation'),
    (N'Edit Religions'),
    (N'Edit School Time Table'),
    (N'Edit Student'),
    (N'Edit Student Fee Allocations'),
    (N'Edit Student Fee Head'),
    (N'Edit Student Payment'),
    (N'Edit Student Payment Details'),
    (N'Edit Subject Type'),
    (N'Edit Ticket'),
    (N'Edit Upazila'),
    (N'Edit User Accounts'),
    (N'Edit User Profile'),
    (N'Edit User Role'),
    (N'Live Results'),
    (N'Lock Academic Exam'),
    (N'Manage Machine Users'),
    (N'Process Result'),
    (N'Register New User'),
    (N'Report Admit Card'),
    (N'SMS Control'),
    (N'Student Due Payment'),
    (N'Student-Wise SMS Service Setup'),
    (N'Submit Academic Exam Marks'),
    (N'Update Optional Subject'),
    (N'Update Ranking'),
    (N'User List'),
    (N'View Academic Class'),
    (N'View Academic Exam'),
    (N'View Academic Exam Group'),
    (N'View Academic Exam Type'),
    (N'View Academic Section'),
    (N'View Academic Session'),
    (N'View Academic Subject'),
    (N'View Admit card Report'),
    (N'View All Question Bank'),
    (N'View Attendance Machines'),
    (N'View Attendance Report'),
    (N'View Attendances'),
    (N'View Blood Groups'),
    (N'View Chapter'),
    (N'View Claim Store'),
    (N'View Class Fee Lists'),
    (N'View Class-Wise Current Result'),
    (N'View Class-wise Processed Result'),
    (N'View Class-wise Subject Allocation'),
    (N'View Config Data'),
    (N'View Daily Attendance Report'),
    (N'View Designation Types'),
    (N'View Designations'),
    (N'View Details Attendance'),
    (N'View Details Attendance Machines'),
    (N'View Details Designations'),
    (N'View Details Ticket'),
    (N'View Details Upazila'),
    (N'View Districts'),
    (N'View Divisions'),
    (N'View Due Amount'),
    (N'View Employee'),
    (N'View Employee Types'),
    (N'View Enrollment List'),
    (N'View Expense Types'),
    (N'View Genders'),
    (N'View Grading Table'),
    (N'View Institutes Info'),
    (N'View Nationalities'),
    (N'View Off Day Types'),
    (N'View Off Days'),
    (N'View Payment Receipt Report'),
    (N'View Phone SMS'),
    (N'View Previous Due Amount'),
    (N'View Project Modules'),
    (N'View Project Sub Modules'),
    (N'View Question Banks'),
    (N'View Question Formation'),
    (N'View Religions'),
    (N'View Roles'),
    (N'View SMS API Data'),
    (N'View Student Fee Allocations'),
    (N'View Student Fee Heads'),
    (N'View Student List'),
    (N'View Student List Report'),
    (N'View Student Payment'),
    (N'View Student Payment Details'),
    (N'View Student Payment Details Report'),
    (N'View Student payment Report'),
    (N'View Student Profile'),
    (N'View Student-Wise Exam Result'),
    (N'View Student-Wise Processed Result'),
    (N'View Studetn-Wise Marksheet Report'),
    (N'View Subject Type'),
    (N'View Subject-Wise Current Result'),
    (N'View Subject-Wise Marksheet Report'),
    (N'View Subject-wise Processed Result'),
    (N'View Tickets'),
    (N'View Upazilas'),
    (N'View User Profile'),
    (N'View User Role');

INSERT INTO AspNetUserClaims (UserId, ClaimType, ClaimValue)
SELECT @AdminId, p.ClaimType, ISNULL(cs.ClaimValue, p.ClaimType)
FROM @Policy p
OUTER APPLY (SELECT TOP 1 ClaimValue FROM ClaimStores c WHERE c.ClaimType = p.ClaimType) cs
WHERE NOT EXISTS (
    SELECT 1 FROM AspNetUserClaims uc
    WHERE uc.UserId = @AdminId AND uc.ClaimType = p.ClaimType);
DECLARE @FromPolicy int = @@ROWCOUNT;

DECLARE @Total int = (SELECT COUNT(DISTINCT ClaimType) FROM AspNetUserClaims WHERE UserId = @AdminId);
DECLARE @Need  int = (SELECT COUNT(*) FROM @Policy);
DECLARE @Gap   int = (SELECT COUNT(*) FROM @Policy p
                      WHERE NOT EXISTS (SELECT 1 FROM AspNetUserClaims uc
                                        WHERE uc.UserId = @AdminId AND uc.ClaimType = p.ClaimType));

PRINT 'Target account            : ' + @TargetEmail;
PRINT 'Granted from ClaimStores  : ' + CAST(@FromStore  AS varchar);
PRINT 'Granted from policy list  : ' + CAST(@FromPolicy AS varchar);
PRINT 'Distinct claims on account: ' + CAST(@Total AS varchar);
PRINT 'Policy claims required    : ' + CAST(@Need  AS varchar);
PRINT 'Policy claims STILL missing: ' + CAST(@Gap AS varchar) + '   <-- must be 0';

IF @Gap = 0
    COMMIT TRANSACTION;
ELSE
BEGIN
    ROLLBACK TRANSACTION;
    RAISERROR('Coverage incomplete - rolled back.', 16, 1);
END
