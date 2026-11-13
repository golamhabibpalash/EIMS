-- ============================================================================
-- Attendance diagnosis - SQL SERVER - READ ONLY (no data is changed)
-- ----------------------------------------------------------------------------
-- Why does a student show "absent" although they punched? Run this against the school's
-- database. Set @Day and the roll list below, then read the result sets:
--   1. The reported students: their UniqueId / MachineUserId / status and the first punch
--      found for them on @Day (by any key the app accepts, leading zeros ignored).
--   2. Punches on @Day that match NO active student or employee, with any student who has the
--      same birth date (the first 6 digits of a UniqueId) - a punch under an old or wrong PIN
--      shows up here next to the student it belongs to.
--   3. Students whose UniqueId starts with 0, and UniqueIds that clash once zeros are ignored.
-- PINs are compared as numbers so "0901..." and "901..." are the same, like the app does.
-- ============================================================================
SET NOCOUNT ON;

DECLARE @Day date = CAST(GETDATE() AS date);          -- <- the day to check
DECLARE @Rolls TABLE (ClassRoll int PRIMARY KEY);
INSERT INTO @Rolls (ClassRoll) VALUES                  -- <- the rolls reported as wrongly absent
    (2606015),(2606024),(2607021),(2607023),(2608008),
    (2608019),(2609026),(2609036),(2610031),(2610043);

-- Punches on the day, with a numeric form of the PIN.
SELECT LTRIM(RTRIM(p.CardNo)) AS CardNo,
       TRY_CAST(LTRIM(RTRIM(p.CardNo)) AS bigint) AS CardNum,
       p.PunchDatetime, p.MachineNo
INTO #Punches
FROM Tran_MachineRawPunch p
WHERE p.PunchDatetime >= @Day AND p.PunchDatetime < DATEADD(day, 1, @Day);

-- Every PIN each student can punch under (MachineUserId, UniqueId, ClassRoll).
SELECT s.Id AS StudentId, k.Pin, TRY_CAST(k.Pin AS bigint) AS PinNum, k.KeyType
INTO #StudentPins
FROM Student s
CROSS APPLY (VALUES (LTRIM(RTRIM(s.MachineUserId)), 'MachineUserId'),
                    (LTRIM(RTRIM(s.UniqueId)), 'UniqueId'),
                    (CAST(s.ClassRoll AS varchar(20)), 'ClassRoll')) k(Pin, KeyType)
WHERE k.Pin IS NOT NULL AND k.Pin <> '';

-- 1. The reported students ---------------------------------------------------------------
SELECT s.ClassRoll, s.Name, s.UniqueId, s.MachineUserId,
       CASE WHEN s.Status = 1 THEN 'Active' ELSE 'INACTIVE (hidden from attendance)' END AS StudentStatus,
       sess.Name AS AcademicSession,
       (SELECT MIN(p.PunchDatetime) FROM #Punches p JOIN #StudentPins sp ON sp.StudentId = s.Id
         WHERE (p.CardNum IS NOT NULL AND p.CardNum = sp.PinNum) OR p.CardNo = sp.Pin) AS FirstPunchOnDay,
       (SELECT TOP 1 sp.KeyType FROM #Punches p JOIN #StudentPins sp ON sp.StudentId = s.Id
         WHERE (p.CardNum IS NOT NULL AND p.CardNum = sp.PinNum) OR p.CardNo = sp.Pin) AS MatchedBy,
       (SELECT MAX(p.PunchDatetime) FROM Tran_MachineRawPunch p JOIN #StudentPins sp ON sp.StudentId = s.Id
         WHERE TRY_CAST(LTRIM(RTRIM(p.CardNo)) AS bigint) = sp.PinNum OR LTRIM(RTRIM(p.CardNo)) = sp.Pin) AS LastPunchEver
FROM Student s
JOIN @Rolls r ON r.ClassRoll = s.ClassRoll
LEFT JOIN AcademicSession sess ON sess.Id = s.AcademicSessionId
ORDER BY s.ClassRoll, s.Status DESC;

-- 2. Punches on the day that match nobody ------------------------------------------------
SELECT u.CardNo, u.FirstPunch, u.Punches, u.Machines,
       cand.ClassRoll AS SameBirthDate_Roll, cand.Name AS SameBirthDate_Name,
       cand.UniqueId AS SameBirthDate_UniqueId, cand.MachineUserId AS SameBirthDate_MachineUserId
FROM (
    SELECT p.CardNo, MAX(p.CardNum) AS CardNum, MIN(p.PunchDatetime) AS FirstPunch, COUNT(*) AS Punches,
           COUNT(DISTINCT p.MachineNo) AS Machines
    FROM #Punches p
    WHERE NOT EXISTS (SELECT 1 FROM #StudentPins sp JOIN Student s ON s.Id = sp.StudentId AND s.Status = 1
                      WHERE (p.CardNum IS NOT NULL AND p.CardNum = sp.PinNum) OR p.CardNo = sp.Pin)
      AND NOT EXISTS (SELECT 1 FROM Employee e WHERE e.Status = 1 AND (
                          (p.CardNum IS NOT NULL AND p.CardNum = TRY_CAST(LTRIM(RTRIM(e.MachineUserId)) AS bigint))
                          OR p.CardNo = LTRIM(RTRIM(e.MachineUserId))
                          OR p.CardNum = e.Id))
    GROUP BY p.CardNo
) u
-- A 9-digit UniqueId is DOB (yyMMdd) + session digit + roll's last 2 digits; compare the DOB part.
OUTER APPLY (
    SELECT s.ClassRoll, s.Name, s.UniqueId, s.MachineUserId
    FROM Student s
    WHERE s.Status = 1 AND u.CardNum IS NOT NULL
      AND TRY_CAST(s.UniqueId AS bigint) IS NOT NULL
      AND LEFT(RIGHT('000000000' + CAST(TRY_CAST(s.UniqueId AS bigint) AS varchar(20)), 9), 6)
        = LEFT(RIGHT('000000000' + CAST(u.CardNum AS varchar(20)), 9), 6)
) cand
ORDER BY u.FirstPunch, u.CardNo;

-- 3. UniqueIds with leading zeros, and clashes once zeros are ignored --------------------
SELECT COUNT(*) AS StudentsWithLeadingZeroUniqueId,
       SUM(CASE WHEN Status = 1 THEN 1 ELSE 0 END) AS OfWhichActive
FROM Student
WHERE LTRIM(RTRIM(UniqueId)) LIKE '0%';

SELECT TRY_CAST(UniqueId AS bigint) AS SameIdIgnoringZeros, COUNT(*) AS Students,
       MIN(UniqueId) AS Example1, MAX(UniqueId) AS Example2
FROM Student
WHERE TRY_CAST(UniqueId AS bigint) IS NOT NULL
GROUP BY TRY_CAST(UniqueId AS bigint)
HAVING COUNT(*) > 1;

DROP TABLE #Punches;
DROP TABLE #StudentPins;
