/* =====================================================================
   EIMS - Strip a leading zero from Student.UniqueId (SQL Server).

   WHY
   UniqueId is generated as DOB("yyMMdd") + the session name's last digit
   + the last 2 digits of ClassRoll. A birth year ending 00-09 (2000,
   2005, 2109, ...) puts a leading zero on it, e.g. DOB 2005-03-14 ->
   "050314...". "0123" and "123" are numerically the same PIN, but two
   screens used to compare UniqueId as plain text, so a student like
   this could show Present on one screen (numeric match) and Absent on
   another (string match). The app no longer generates a leading zero
   and compares PINs numerically everywhere (see AttendancePinMatcher in
   SMS.Entities) - this script cleans up rows created before that fix.

   dbo.StudentPayment and dbo.StudentFeeAllocations each store their OWN
   copy of the student's UniqueId (not a join to Student.Id) - fixing
   Student alone would leave their history pointing at a UniqueId no
   student has any more, so all three tables are updated together, in
   one transaction, from a single old-to-new mapping computed up front.

   WHAT IT DOES
   For every Student whose UniqueId starts with '0' and is fully
   numeric, strips the leading zero(s) - e.g. "070314405" -> "70314405"
   - in Student, StudentPayment and StudentFeeAllocations alike.
   (UniqueId, ClassRoll) has a unique index on Student, so a student is
   skipped (left for a human to look at) rather than fixed if stripping
   its zero would collide with another student sharing the same
   ClassRoll and the same resulting UniqueId.

   >>> HOW TO RUN IN DBEAVER <<<
   Select the ENTIRE script, then press Ctrl+Enter (Execute SQL
   Statement), not Alt+X (Execute Script splits on ';' into separate
   batches, which drops the temp table and the transaction).

   Idempotent: rerun any time; rows already fixed are simply skipped.
   Wrapped in a transaction, so a problem rolls back cleanly.
   ===================================================================== */

SET NOCOUNT ON;
SET XACT_ABORT ON;

IF OBJECT_ID('tempdb..#UniqueIdFix') IS NOT NULL DROP TABLE #UniqueIdFix;

-- One row per affected student: old value, its stripped replacement, and whether it's safe to
-- apply. (TRY_CAST rejects anything non-numeric rather than accepting it, unlike ISNUMERIC.)
SELECT
    s.Id,
    s.ClassRoll,
    OldUniqueId = s.UniqueId,
    NewUniqueId = CAST(TRY_CAST(s.UniqueId AS BIGINT) AS VARCHAR(20)),
    Skip = CASE WHEN EXISTS (
                SELECT 1 FROM dbo.Student s2
                WHERE s2.ClassRoll = s.ClassRoll
                  AND s2.UniqueId  = CAST(TRY_CAST(s.UniqueId AS BIGINT) AS VARCHAR(20))
                  AND s2.Id <> s.Id
            ) THEN 1 ELSE 0 END
INTO #UniqueIdFix
FROM dbo.Student s
WHERE s.UniqueId LIKE '0%'
  AND TRY_CAST(s.UniqueId AS BIGINT) IS NOT NULL;

-- 1) Preview - everyone this run would touch (Skip = 1 means "left for manual review").
SELECT * FROM #UniqueIdFix ORDER BY Skip DESC, Id;

BEGIN TRANSACTION;

    -- Payment and fee-allocation history first, keyed off the OLD value, so at no point does a
    -- row exist pointing at a UniqueId that doesn't (yet, or any more) belong to any student.
    UPDATE sp
    SET sp.UniqueId = f.NewUniqueId
    FROM dbo.StudentPayment sp
    JOIN #UniqueIdFix f ON f.OldUniqueId = sp.UniqueId
    WHERE f.Skip = 0;

    PRINT CAST(@@ROWCOUNT AS varchar(10)) + ' StudentPayment row(s) updated.';

    UPDATE sfa
    SET sfa.UniqueId = f.NewUniqueId
    FROM dbo.StudentFeeAllocations sfa
    JOIN #UniqueIdFix f ON f.OldUniqueId = sfa.UniqueId
    WHERE f.Skip = 0;

    PRINT CAST(@@ROWCOUNT AS varchar(10)) + ' StudentFeeAllocations row(s) updated.';

    UPDATE s
    SET s.UniqueId = f.NewUniqueId
    FROM dbo.Student s
    JOIN #UniqueIdFix f ON f.Id = s.Id
    WHERE f.Skip = 0;

    PRINT CAST(@@ROWCOUNT AS varchar(10)) + ' Student row(s) updated.';

COMMIT TRANSACTION;

DROP TABLE #UniqueIdFix;

-- 2) Verify - should return only rows already flagged Skip = 1 above (if any); those need a human
--    to correct the DOB/roll clash by hand before they can be fixed.
SELECT Id, Name, ClassRoll, UniqueId
FROM dbo.Student
WHERE UniqueId LIKE '0%'
  AND TRY_CAST(UniqueId AS BIGINT) IS NOT NULL;
