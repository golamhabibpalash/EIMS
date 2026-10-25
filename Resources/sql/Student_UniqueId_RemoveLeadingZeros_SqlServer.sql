-- ============================================================================
-- Remove leading zeros from Student.UniqueId - SQL SERVER
-- ----------------------------------------------------------------------------
-- New students already get UniqueIds without a leading zero (StudentsController.GenerateUniquId);
-- older rows like "0901125..." are left over. The app matches punches either way, but a zero-free
-- ID keeps every screen, export and login consistent.
--
-- What it changes (for each affected student, all in ONE transaction):
--   Student.UniqueId, StudentPayment.UniqueId, StudentFeeAllocations.UniqueId,
--   and the student's login (AspNetUsers.UserName / NormalizedUserName).
-- Students who used to type "0901..." to log in can still do so - the login page now accepts
-- the ID with or without leading zeros. Attendance machines need no change.
--
-- HOW TO RUN
--   1. Run as is (@Apply = 0): PREVIEW only - nothing is changed. Check the list and that the
--      "conflicts" result is empty.
--   2. Take a database backup.
--   3. Set @Apply = 1 and run again. If any conflict exists it stops without changing anything.
-- Safe to re-run: once IDs have no leading zero there is nothing left to do.
-- ============================================================================
SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @Apply bit = 0;   -- <- 0 = preview only, 1 = make the changes

SELECT s.Id AS StudentId, s.ClassRoll, s.Name, s.Status,
       LTRIM(RTRIM(s.UniqueId)) AS OldUniqueId,
       CAST(TRY_CAST(LTRIM(RTRIM(s.UniqueId)) AS bigint) AS varchar(20)) AS NewUniqueId
INTO #Map
FROM Student s
WHERE LTRIM(RTRIM(s.UniqueId)) LIKE '0%'
  AND TRY_CAST(LTRIM(RTRIM(s.UniqueId)) AS bigint) IS NOT NULL
  AND TRY_CAST(LTRIM(RTRIM(s.UniqueId)) AS bigint) > 0;

-- The new ID must not already belong to another student or another login.
SELECT m.StudentId, m.ClassRoll, m.Name, m.OldUniqueId, m.NewUniqueId,
       CASE WHEN EXISTS (SELECT 1 FROM Student o WHERE LTRIM(RTRIM(o.UniqueId)) = m.NewUniqueId AND o.Id <> m.StudentId)
                THEN 'Another student already has this UniqueId'
            WHEN EXISTS (SELECT 1 FROM AspNetUsers u WHERE u.UserName = m.NewUniqueId)
                THEN 'A login with this username already exists'
            WHEN (SELECT COUNT(*) FROM #Map d WHERE d.NewUniqueId = m.NewUniqueId) > 1
                THEN 'Two students would get the same UniqueId'
       END AS Conflict
INTO #Conflicts
FROM #Map m;
DELETE FROM #Conflicts WHERE Conflict IS NULL;

-- Preview
SELECT m.ClassRoll, m.Name, CASE WHEN m.Status = 1 THEN 'Active' ELSE 'Inactive' END AS Status,
       m.OldUniqueId, m.NewUniqueId,
       (SELECT COUNT(*) FROM StudentPayment p WHERE LTRIM(RTRIM(p.UniqueId)) = m.OldUniqueId) AS Payments,
       (SELECT COUNT(*) FROM StudentFeeAllocations a WHERE LTRIM(RTRIM(a.UniqueId)) = m.OldUniqueId) AS FeeAllocations,
       (SELECT COUNT(*) FROM AspNetUsers u WHERE u.UserName = m.OldUniqueId) AS Logins
FROM #Map m
ORDER BY m.ClassRoll;

SELECT * FROM #Conflicts ORDER BY ClassRoll;   -- must be empty before applying

IF @Apply = 1
BEGIN
    IF EXISTS (SELECT 1 FROM #Conflicts)
    BEGIN
        PRINT 'NOT APPLIED: resolve the conflicts listed above first. Nothing was changed.';
    END
    ELSE
    BEGIN
        BEGIN TRANSACTION;

        UPDATE p SET p.UniqueId = m.NewUniqueId
        FROM StudentPayment p JOIN #Map m ON LTRIM(RTRIM(p.UniqueId)) = m.OldUniqueId;

        UPDATE a SET a.UniqueId = m.NewUniqueId
        FROM StudentFeeAllocations a JOIN #Map m ON LTRIM(RTRIM(a.UniqueId)) = m.OldUniqueId;

        UPDATE u SET u.UserName = m.NewUniqueId, u.NormalizedUserName = UPPER(m.NewUniqueId)
        FROM AspNetUsers u JOIN #Map m ON u.UserName = m.OldUniqueId;

        UPDATE s SET s.UniqueId = m.NewUniqueId
        FROM Student s JOIN #Map m ON m.StudentId = s.Id;

        COMMIT TRANSACTION;
        DECLARE @Updated int = (SELECT COUNT(*) FROM #Map);
        PRINT CONCAT('APPLIED: ', @Updated, ' student UniqueId(s) updated.');
    END
END
ELSE
BEGIN
    PRINT 'PREVIEW ONLY: nothing was changed. Set @Apply = 1 to apply.';
END

DROP TABLE #Conflicts;
DROP TABLE #Map;
