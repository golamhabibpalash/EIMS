/* =====================================================================
   EIMS - bring a live SQL Server database up to the schema that commit
   d913aed3 ("Attendance Machine incorporate") expects.

   Fixes: Invalid column name 'MachineUserId' on /Students and /Employees,
   and the missing AttendanceMachines table behind the Fingerprint Machines
   pages.

   Safe to run more than once - every step checks before it acts.
   Applies only the 8 real changes; it deliberately does NOT touch the 166
   foreign-key drop/create operations that "dotnet ef database update" would
   also run against a live database.
   ===================================================================== */

SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

/* ---------- REQUIRED: without these the app cannot query students -------- */

IF COL_LENGTH('dbo.Student', 'MachineUserId') IS NULL
BEGIN
    ALTER TABLE dbo.Student ADD MachineUserId nvarchar(20) NULL;
    PRINT 'added  Student.MachineUserId';
END
ELSE PRINT 'exists Student.MachineUserId';
GO

IF COL_LENGTH('dbo.Employee', 'MachineUserId') IS NULL
BEGIN
    ALTER TABLE dbo.Employee ADD MachineUserId nvarchar(20) NULL;
    PRINT 'added  Employee.MachineUserId';
END
ELSE PRINT 'exists Employee.MachineUserId';
GO

IF COL_LENGTH('dbo.Tran_MachineRawPunch', 'IsSynced') IS NULL
BEGIN
    ALTER TABLE dbo.Tran_MachineRawPunch
        ADD IsSynced bit NOT NULL CONSTRAINT DF_Tran_MachineRawPunch_IsSynced DEFAULT (0);
    PRINT 'added  Tran_MachineRawPunch.IsSynced';
END
ELSE PRINT 'exists Tran_MachineRawPunch.IsSynced';
GO

IF COL_LENGTH('dbo.Tran_MachineRawPunch', 'MachineSerialNo') IS NULL
BEGIN
    ALTER TABLE dbo.Tran_MachineRawPunch ADD MachineSerialNo nvarchar(50) NULL;
    PRINT 'added  Tran_MachineRawPunch.MachineSerialNo';
END
ELSE PRINT 'exists Tran_MachineRawPunch.MachineSerialNo';
GO

IF COL_LENGTH('dbo.Tran_MachineRawPunch', 'VerifyMode') IS NULL
BEGIN
    ALTER TABLE dbo.Tran_MachineRawPunch ADD VerifyMode int NULL;
    PRINT 'added  Tran_MachineRawPunch.VerifyMode';
END
ELSE PRINT 'exists Tran_MachineRawPunch.VerifyMode';
GO

IF OBJECT_ID('dbo.AttendanceMachines', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.AttendanceMachines
    (
        Id            int IDENTITY(1,1) NOT NULL,
        Name          nvarchar(100)  NOT NULL,
        IPAddress     nvarchar(50)   NOT NULL,
        Port          int            NOT NULL,
        SerialNumber  nvarchar(50)   NULL,
        Model         nvarchar(50)   NULL,
        Brand         int            NOT NULL,
        Location      nvarchar(200)  NULL,
        IsActive      bit            NOT NULL,
        UsePushMode   bit            NOT NULL,
        LastSyncAt    datetime2      NULL,
        LastError     nvarchar(500)  NULL,
        PushEndpoint  nvarchar(200)  NULL,
        Username      nvarchar(50)   NULL,
        Password      nvarchar(50)   NULL,
        CreatedBy     nvarchar(max)  NULL,
        CreatedAt     datetime2      NOT NULL,
        EditedBy      nvarchar(max)  NULL,
        EditedAt      datetime2      NOT NULL,
        MACAddress    nvarchar(max)  NULL,
        CONSTRAINT PK_AttendanceMachines PRIMARY KEY CLUSTERED (Id)
    );
    PRINT 'created AttendanceMachines';
END
ELSE PRINT 'exists  AttendanceMachines';
GO

/* ---------- OPTIONAL: column resizes ------------------------------------
   The app runs correctly without these - EF does not check column width
   when it queries. They only realign the database with the model, and
   CardNo shrinks, so the script refuses if any row would not fit.        */

IF EXISTS (SELECT 1 FROM sys.columns
           WHERE object_id = OBJECT_ID('dbo.Tran_MachineRawPunch')
             AND name = 'MachineNo' AND (max_length <> 100 OR is_nullable = 0))
BEGIN
    ALTER TABLE dbo.Tran_MachineRawPunch ALTER COLUMN MachineNo nvarchar(50) NULL;
    PRINT 'resized Tran_MachineRawPunch.MachineNo -> nvarchar(50) NULL';
END
ELSE PRINT 'ok      Tran_MachineRawPunch.MachineNo';
GO

IF EXISTS (SELECT 1 FROM sys.columns
           WHERE object_id = OBJECT_ID('dbo.Tran_MachineRawPunch')
             AND name = 'CardNo' AND (max_length <> 40 OR is_nullable = 0))
BEGIN
    IF EXISTS (SELECT 1 FROM dbo.Tran_MachineRawPunch WHERE LEN(CardNo) > 20)
        PRINT 'SKIPPED CardNo resize - rows exist with CardNo longer than 20 characters';
    ELSE
    BEGIN
        ALTER TABLE dbo.Tran_MachineRawPunch ALTER COLUMN CardNo nvarchar(20) NULL;
        PRINT 'resized Tran_MachineRawPunch.CardNo -> nvarchar(20) NULL';
    END
END
ELSE PRINT 'ok      Tran_MachineRawPunch.CardNo';
GO

/* ---------- Record the migration as applied -----------------------------
   Stops a future "dotnet ef database update" from replaying this migration
   and its 166 foreign-key rebuilds against live data.                     */

IF OBJECT_ID('dbo.__EFMigrationsHistory', 'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM dbo.__EFMigrationsHistory
                   WHERE MigrationId = '20260821192919_AddAttendanceMachineAndFingerprintFields')
BEGIN
    INSERT INTO dbo.__EFMigrationsHistory (MigrationId, ProductVersion)
    VALUES ('20260821192919_AddAttendanceMachineAndFingerprintFields', '7.0.11');
    PRINT 'recorded migration in __EFMigrationsHistory';
END
ELSE PRINT 'ok      __EFMigrationsHistory';
GO

/* ---------- Verification ------------------------------------------------ */

SELECT
    Student_MachineUserId    = CASE WHEN COL_LENGTH('dbo.Student','MachineUserId')              IS NULL THEN 'MISSING' ELSE 'OK' END,
    Employee_MachineUserId   = CASE WHEN COL_LENGTH('dbo.Employee','MachineUserId')             IS NULL THEN 'MISSING' ELSE 'OK' END,
    Punch_IsSynced           = CASE WHEN COL_LENGTH('dbo.Tran_MachineRawPunch','IsSynced')      IS NULL THEN 'MISSING' ELSE 'OK' END,
    Punch_MachineSerialNo    = CASE WHEN COL_LENGTH('dbo.Tran_MachineRawPunch','MachineSerialNo') IS NULL THEN 'MISSING' ELSE 'OK' END,
    Punch_VerifyMode         = CASE WHEN COL_LENGTH('dbo.Tran_MachineRawPunch','VerifyMode')    IS NULL THEN 'MISSING' ELSE 'OK' END,
    AttendanceMachines_Table = CASE WHEN OBJECT_ID('dbo.AttendanceMachines','U')                IS NULL THEN 'MISSING' ELSE 'OK' END;
GO
