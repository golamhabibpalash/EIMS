/* =====================================================================
   EIMS - Pre-deployment readiness check.

   Read-only. Changes nothing. Run BEFORE uploading the new build.
   DBeaver: Execute script (Alt+X).

   Everything comes back as ONE result grid so it can be pasted whole -
   an earlier version returned six separate grids and it was impossible
   to tell which section a row belonged to.

   The application calls EnsureCreatedAsync() on startup, NOT
   MigrateAsync(). EnsureCreated does nothing at all when the database
   already exists, so nothing below gets created for you by deploying.
   Anything reported MISSING must be added by hand first, or the feature
   that uses it throws at runtime.
   ===================================================================== */

SET NOCOUNT ON;

SELECT
    Section,
    Item,
    Status,
    FixWith
FROM (
    /* ---- 1. Columns the attendance SMS code reads to resolve a PIN --- */
    SELECT Section = '1. PIN RESOLUTION', Item = v.Item, SortKey = 1,
           Status  = CASE WHEN COL_LENGTH(v.T, v.C) IS NULL THEN 'MISSING' ELSE 'OK' END,
           FixWith = CASE WHEN COL_LENGTH(v.T, v.C) IS NULL
                          THEN '2026-08-26_AddAttendanceMachineAndFingerprintFields.sql' ELSE '' END
    FROM (VALUES
        ('Student.UniqueId',       'dbo.Student',  'UniqueId'),
        ('Student.MachineUserId',  'dbo.Student',  'MachineUserId'),
        ('Employee.MachineUserId', 'dbo.Employee', 'MachineUserId'),
        ('Employee.Phone',         'dbo.Employee', 'Phone')
    ) AS v(Item, T, C)

    UNION ALL
    /* ---- 2. Columns the punch pipeline writes ------------------------ */
    SELECT '2. PUNCH STORAGE', v.Item, 2,
           CASE WHEN COL_LENGTH(v.T, v.C) IS NULL THEN 'MISSING' ELSE 'OK' END,
           CASE WHEN COL_LENGTH(v.T, v.C) IS NULL
                THEN '2026-08-26_AddAttendanceMachineAndFingerprintFields.sql' ELSE '' END
    FROM (VALUES
        ('Tran_MachineRawPunch.MachineNo',       'dbo.Tran_MachineRawPunch', 'MachineNo'),
        ('Tran_MachineRawPunch.MachineSerialNo', 'dbo.Tran_MachineRawPunch', 'MachineSerialNo'),
        ('Tran_MachineRawPunch.VerifyMode',      'dbo.Tran_MachineRawPunch', 'VerifyMode'),
        ('Tran_MachineRawPunch.IsSynced',        'dbo.Tran_MachineRawPunch', 'IsSynced'),
        ('AttendanceMachines.SerialNumber',      'dbo.AttendanceMachines',   'SerialNumber'),
        ('AttendanceMachines.UsePushMode',       'dbo.AttendanceMachines',   'UsePushMode'),
        ('AttendanceMachines.LastError',         'dbo.AttendanceMachines',   'LastError'),
        ('AttendanceMachines.LastSyncAt',        'dbo.AttendanceMachines',   'LastSyncAt')
    ) AS v(Item, T, C)

    UNION ALL
    /* ---- 3. Columns the daily attendance report reads ---------------- */
    /*  SMSSent is now derived from PhoneSMS instead of hardcoded "".    */
    SELECT '3. REPORT / SMS LOG', v.Item, 3,
           CASE WHEN COL_LENGTH(v.T, v.C) IS NULL THEN 'MISSING' ELSE 'OK' END,
           CASE WHEN COL_LENGTH(v.T, v.C) IS NULL THEN 'investigate - report will fail' ELSE '' END
    FROM (VALUES
        ('PhoneSMS.MobileNumber', 'dbo.PhoneSMS', 'MobileNumber'),
        ('PhoneSMS.SMSType',      'dbo.PhoneSMS', 'SMSType'),
        ('PhoneSMS.CreatedAt',    'dbo.PhoneSMS', 'CreatedAt'),
        ('Student.Status',        'dbo.Student',  'Status'),
        ('Student.AcademicSessionId', 'dbo.Student', 'AcademicSessionId'),
        ('Employee.Status',       'dbo.Employee', 'Status')
    ) AS v(Item, T, C)

    UNION ALL
    /* ---- 4. Ticket module tables ------------------------------------ */
    SELECT '4. TICKET MODULE', v.Item, 4,
           CASE WHEN OBJECT_ID(v.T, 'U') IS NULL THEN 'MISSING' ELSE 'OK' END,
           CASE WHEN OBJECT_ID(v.T, 'U') IS NULL
                THEN '2026-08-28_AddTicketModule_schema_DBEAVER.sql' ELSE '' END
    FROM (VALUES
        ('Tickets',           'dbo.Tickets'),
        ('TicketComments',    'dbo.TicketComments'),
        ('TicketAttachments', 'dbo.TicketAttachments')
    ) AS v(Item, T)

    UNION ALL
    /* ---- 5. MachineNo must hold the 13-char device serial ------------ */
    SELECT '5. MACHINENO WIDTH',
           'Tran_MachineRawPunch.MachineNo = '
             + CAST(CASE WHEN c.max_length = -1 THEN -1
                         WHEN t.name LIKE 'n%' THEN c.max_length / 2
                         ELSE c.max_length END AS varchar(20)) + ' chars',
           5,
           CASE WHEN c.max_length = -1
                  OR (CASE WHEN t.name LIKE 'n%' THEN c.max_length/2 ELSE c.max_length END) >= 13
                THEN 'OK' ELSE 'TOO NARROW' END,
           CASE WHEN c.max_length = -1
                  OR (CASE WHEN t.name LIKE 'n%' THEN c.max_length/2 ELSE c.max_length END) >= 13
                THEN '' ELSE '2026-08-27_Fix_WidenMachineNo_5074safe.sql' END
    FROM sys.columns c
    JOIN sys.types   t ON t.user_type_id = c.user_type_id
    WHERE c.object_id = OBJECT_ID('dbo.Tran_MachineRawPunch')
      AND c.name      = 'MachineNo'
) AS checks
ORDER BY SortKey, Item;

/* ---- 6. SMS master switches, informational -------------------------- */
/*  The notifier honours SMSService and AttendanceSMSService plus the
    per-audience toggles. If SMS does not arrive after deploy, look here
    first: a 0 in either of the first two silences everything no matter
    what the code does.                                                  */
SELECT
    SMSService,
    AttendanceSMSService,
    CheckInSMSServiceForMaleStudent,
    CheckInSMSServiceForGirlsStudent,
    CheckOutSMSServiceForMaleStudent,
    CheckOutSMSServiceForGirlsStudent,
    CheckInSMSServiceForEmployees,
    CheckOutSMSServiceForEmployees
FROM dbo.SetupMobileSMS
WHERE Id = 1;
