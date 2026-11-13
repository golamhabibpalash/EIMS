/* =====================================================================
   EIMS - Why have no punches appeared?

   Read-only. Run in DBeaver with Alt+X and report all five results.

   Ordered by likelihood, most probable cause first.
   ===================================================================== */

SET NOCOUNT ON;

/* ---- 1. THE PRIME SUSPECT: is MachineNo wide enough? ---------------
   The ADMS path writes the serial 'WEF5261400145' (13 chars) into
   MachineNo. If the column is narrower, EVERY insert fails with a
   truncation error and no punch can ever be stored.                   */
SELECT
    Section  = '1. COLUMN WIDTH',
    Column_  = c.name,
    MaxChars = CASE WHEN c.max_length = -1 THEN -1
                    WHEN t.name LIKE 'n%' THEN c.max_length / 2
                    ELSE c.max_length END,
    Needed   = 13,
    Verdict  = CASE
                 WHEN c.max_length = -1 THEN 'OK (max)'
                 WHEN (CASE WHEN t.name LIKE 'n%' THEN c.max_length/2 ELSE c.max_length END) >= 13
                   THEN 'OK'
                 ELSE '*** TOO NARROW - THIS IS THE BUG. Run the widening script. ***'
               END
FROM sys.columns c
JOIN sys.types   t ON t.user_type_id = c.user_type_id
WHERE c.object_id = OBJECT_ID('dbo.Tran_MachineRawPunch')
  AND c.name IN ('MachineNo','CardNo','MachineSerialNo');

/* ---- 2. Did the app log an error? MinimumLevel=Error, so real
          failures DO land here even though normal activity does not. */
SELECT TOP 20
    Section = '2. RECENT ERRORS',
    Id, TimeStamp, Level,
    Message = LEFT(CAST(Message AS nvarchar(max)), 400),
    Exception = LEFT(CAST(Exception AS nvarchar(max)), 600)
FROM dbo.Logs
WHERE TimeStamp >= DATEADD(HOUR, -6, GETDATE())
ORDER BY Id DESC;

/* ---- 3. ANY punches at all recently, from any machine? -------------
   Rules out the possibility that rows landed under a different
   MachineNo than the one the earlier query filtered on.              */
SELECT TOP 20
    Section = '3. ANY RECENT PUNCH',
    Tran_MachineRawPunchId, CardNo, PunchDatetime,
    MachineNo, MachineSerialNo, VerifyMode, ISManual
FROM dbo.Tran_MachineRawPunch
ORDER BY Tran_MachineRawPunchId DESC;

/* ---- 4. Grand totals, so 'empty' is unambiguous -------------------- */
SELECT
    Section        = '4. TOTALS',
    AllPunchesEver = (SELECT COUNT(*) FROM dbo.Tran_MachineRawPunch),
    Today          = (SELECT COUNT(*) FROM dbo.Tran_MachineRawPunch
                      WHERE CAST(PunchDatetime AS date) = CAST(GETDATE() AS date)),
    FromThisDevice = (SELECT COUNT(*) FROM dbo.Tran_MachineRawPunch
                      WHERE MachineSerialNo = 'WEF5261400145'
                         OR MachineNo       = 'WEF5261400145');

/* ---- 5. Device state (confirms the row exists and is checking in) -- */
SELECT
    Section = '5. DEVICE',
    Id, Name, SerialNumber, IsActive, UsePushMode,
    LastSyncAt,
    SecondsSinceCheckIn = DATEDIFF(SECOND, LastSyncAt, GETDATE()),
    LastError
FROM dbo.AttendanceMachines;
