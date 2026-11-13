/* =====================================================================
   EIMS - REQUIRED schema changes only.

   Fixes "Invalid column name 'MachineUserId'" on /Students and /Employees,
   and creates the AttendanceMachines table.

   The optional MachineNo / CardNo column resizes have been REMOVED: they
   give no functional benefit (EF does not check column width when it
   queries) and they fail on a live database whose columns carry dependent
   default constraints - error 5074.

   No GO, no BEGIN/END, one statement per change. Safe to re-run.
   Run with Execute script (Alt+X).
   ===================================================================== */

SET NOCOUNT ON;

IF COL_LENGTH('dbo.Student', 'MachineUserId') IS NULL
    EXEC('ALTER TABLE dbo.Student ADD MachineUserId nvarchar(20) NULL');

IF COL_LENGTH('dbo.Employee', 'MachineUserId') IS NULL
    EXEC('ALTER TABLE dbo.Employee ADD MachineUserId nvarchar(20) NULL');

IF COL_LENGTH('dbo.Tran_MachineRawPunch', 'IsSynced') IS NULL
    EXEC('ALTER TABLE dbo.Tran_MachineRawPunch ADD IsSynced bit NOT NULL CONSTRAINT DF_Tran_MachineRawPunch_IsSynced DEFAULT (0)');

IF COL_LENGTH('dbo.Tran_MachineRawPunch', 'MachineSerialNo') IS NULL
    EXEC('ALTER TABLE dbo.Tran_MachineRawPunch ADD MachineSerialNo nvarchar(50) NULL');

IF COL_LENGTH('dbo.Tran_MachineRawPunch', 'VerifyMode') IS NULL
    EXEC('ALTER TABLE dbo.Tran_MachineRawPunch ADD VerifyMode int NULL');

IF OBJECT_ID('dbo.AttendanceMachines', 'U') IS NULL
    EXEC('CREATE TABLE dbo.AttendanceMachines (Id int IDENTITY(1,1) NOT NULL, Name nvarchar(100) NOT NULL, IPAddress nvarchar(50) NOT NULL, Port int NOT NULL, SerialNumber nvarchar(50) NULL, Model nvarchar(50) NULL, Brand int NOT NULL, Location nvarchar(200) NULL, IsActive bit NOT NULL, UsePushMode bit NOT NULL, LastSyncAt datetime2 NULL, LastError nvarchar(500) NULL, PushEndpoint nvarchar(200) NULL, Username nvarchar(50) NULL, Password nvarchar(50) NULL, CreatedBy nvarchar(max) NULL, CreatedAt datetime2 NOT NULL, EditedBy nvarchar(max) NULL, EditedAt datetime2 NOT NULL, MACAddress nvarchar(max) NULL, CONSTRAINT PK_AttendanceMachines PRIMARY KEY CLUSTERED (Id))');

IF OBJECT_ID('dbo.__EFMigrationsHistory', 'U') IS NOT NULL
    EXEC('IF NOT EXISTS (SELECT 1 FROM dbo.__EFMigrationsHistory WHERE MigrationId = ''20260821192919_AddAttendanceMachineAndFingerprintFields'') INSERT INTO dbo.__EFMigrationsHistory (MigrationId, ProductVersion) VALUES (''20260821192919_AddAttendanceMachineAndFingerprintFields'', ''7.0.11'')');

SELECT
    Student_MachineUserId    = CASE WHEN COL_LENGTH('dbo.Student','MachineUserId')                IS NULL THEN 'MISSING' ELSE 'OK' END,
    Employee_MachineUserId   = CASE WHEN COL_LENGTH('dbo.Employee','MachineUserId')               IS NULL THEN 'MISSING' ELSE 'OK' END,
    Punch_IsSynced           = CASE WHEN COL_LENGTH('dbo.Tran_MachineRawPunch','IsSynced')        IS NULL THEN 'MISSING' ELSE 'OK' END,
    Punch_MachineSerialNo    = CASE WHEN COL_LENGTH('dbo.Tran_MachineRawPunch','MachineSerialNo') IS NULL THEN 'MISSING' ELSE 'OK' END,
    Punch_VerifyMode         = CASE WHEN COL_LENGTH('dbo.Tran_MachineRawPunch','VerifyMode')      IS NULL THEN 'MISSING' ELSE 'OK' END,
    AttendanceMachines_Table = CASE WHEN OBJECT_ID('dbo.AttendanceMachines','U')                  IS NULL THEN 'MISSING' ELSE 'OK' END;
