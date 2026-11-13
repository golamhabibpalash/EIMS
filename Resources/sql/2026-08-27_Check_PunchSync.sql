/* =====================================================================
   EIMS - Is the SenseFace T1 actually delivering punches?

   Read-only. Changes nothing. Run any time. DBeaver: Alt+X.

   Read the results in order - each one narrows down where a problem is:
     1. Has the device contacted the server at all?
     2. Have any punches arrived, and how recently?
     3. The last 20 punches, newest first.
     4. Do the PINs the device sends match anybody enrolled?
        This is the usual failure: punches arrive fine but belong to
        nobody, so they never surface as a student's attendance.
   ===================================================================== */

SET NOCOUNT ON;

/* ---- 1. Device check-in status ------------------------------------- */
SELECT
    Section      = '1. DEVICE',
    Name,
    SerialNumber,
    IsActive,
    UsePushMode,
    LastCheckIn  = LastSyncAt,
    MinutesAgo   = DATEDIFF(MINUTE, LastSyncAt, GETDATE()),
    LastError,
    Verdict      = CASE
                     WHEN LastSyncAt IS NULL
                       THEN 'NEVER CONTACTED - check device Cloud Server settings'
                     WHEN DATEDIFF(MINUTE, LastSyncAt, GETDATE()) <= 15
                       THEN 'HEALTHY - talking to the server now'
                     ELSE 'STALE - device stopped checking in'
                   END
FROM dbo.AttendanceMachines
WHERE SerialNumber = 'WEF5261400145';

/* ---- 2. Punch volume ----------------------------------------------- */
SELECT
    Section          = '2. PUNCH TOTALS',
    TotalFromDevice  = COUNT(*),
    Today            = SUM(CASE WHEN CAST(PunchDatetime AS date) = CAST(GETDATE() AS date) THEN 1 ELSE 0 END),
    Last24h          = SUM(CASE WHEN PunchDatetime >= DATEADD(HOUR, -24, GETDATE()) THEN 1 ELSE 0 END),
    EarliestPunch    = MIN(PunchDatetime),
    LatestPunch      = MAX(PunchDatetime)
FROM dbo.Tran_MachineRawPunch
WHERE MachineSerialNo = 'WEF5261400145'
   OR MachineNo       = 'WEF5261400145';

/* ---- 3. The punches themselves, newest first ------------------------ */
SELECT TOP 20
    Section    = '3. RECENT PUNCHES',
    Tran_MachineRawPunchId,
    PIN        = CardNo,
    PunchDatetime,
    VerifyMode,          -- 1 = fingerprint, 15 = face, 2 = card, 0 = password
    MachineNo,
    MachineSerialNo,
    ISManual
FROM dbo.Tran_MachineRawPunch
WHERE MachineSerialNo = 'WEF5261400145'
   OR MachineNo       = 'WEF5261400145'
ORDER BY Tran_MachineRawPunchId DESC;

/* ---- 4. Do those PINs belong to anyone? ----------------------------- */
/*  A punch whose PIN matches nothing is stored but can never be shown
    as somebody's attendance, and no SMS can be sent for it.            */
SELECT TOP 20
    Section      = '4. PIN -> PERSON',
    PIN          = p.CardNo,
    Punches      = COUNT(*),
    MatchedStudent = MAX(CASE WHEN s.Id IS NOT NULL THEN s.Name END),
    MatchedByRoll  = MAX(CASE WHEN sr.Id IS NOT NULL THEN sr.Name END),
    MatchedEmployee= MAX(CASE WHEN e.Id IS NOT NULL THEN e.EmployeeName END),
    Verdict      = CASE
                     WHEN MAX(CASE WHEN s.Id  IS NOT NULL THEN 1 ELSE 0 END) = 1
                       THEN 'OK - matches Student.MachineUserId'
                     WHEN MAX(CASE WHEN e.Id  IS NOT NULL THEN 1 ELSE 0 END) = 1
                       THEN 'OK - matches Employee.MachineUserId'
                     WHEN MAX(CASE WHEN sr.Id IS NOT NULL THEN 1 ELSE 0 END) = 1
                       THEN 'ClassRoll only - SMS works, enrollment field is empty'
                     ELSE 'UNMATCHED - nobody is enrolled with this PIN'
                   END
FROM dbo.Tran_MachineRawPunch p
LEFT JOIN dbo.Student  s  ON s.MachineUserId = p.CardNo
LEFT JOIN dbo.Employee e  ON e.MachineUserId = p.CardNo
LEFT JOIN dbo.Student  sr ON TRY_CAST(p.CardNo AS int) IS NOT NULL
                         AND sr.ClassRoll = TRY_CAST(p.CardNo AS int)
WHERE p.MachineSerialNo = 'WEF5261400145'
   OR p.MachineNo       = 'WEF5261400145'
GROUP BY p.CardNo
ORDER BY COUNT(*) DESC;
