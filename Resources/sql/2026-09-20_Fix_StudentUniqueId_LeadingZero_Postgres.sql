-- =====================================================================
-- EIMS - Strip a leading zero from "Student"."UniqueId" (PostgreSQL).
-- See the SQL Server version of this script (same folder) for the full
-- explanation of why this is needed, and why StudentPayment and
-- StudentFeeAllocations - which each store their OWN copy of the
-- student's UniqueId, not a join to Student.Id - are fixed alongside it
-- in the same transaction.
--
-- Identifiers below are quoted PascalCase to match what EF Core's
-- Npgsql provider creates by default (no snake_case naming convention
-- is configured in this project) - but the PostgreSQL database here is
-- provisioned from a dump, not from these migrations (see CLAUDE.md),
-- so confirm the real names first, e.g.:
--   \d "Student"
-- and adjust the quoted identifiers below if the dump differs.
--
-- Idempotent: rerun any time; rows already fixed are simply skipped.
-- Wrapped in a transaction, so a problem rolls back cleanly.
-- =====================================================================

BEGIN;

CREATE TEMP TABLE unique_id_fix ON COMMIT DROP AS
SELECT
    s."Id",
    s."ClassRoll",
    s."UniqueId" AS old_unique_id,
    (s."UniqueId"::bigint)::text AS new_unique_id,
    EXISTS (
        SELECT 1 FROM "Student" s2
        WHERE s2."ClassRoll" = s."ClassRoll"
          AND s2."UniqueId"  = (s."UniqueId"::bigint)::text
          AND s2."Id" <> s."Id"
    ) AS skip
FROM "Student" s
WHERE s."UniqueId" LIKE '0%'
  AND s."UniqueId" ~ '^[0-9]+$';

-- 1) Preview - everyone this run would touch (skip = true means "left for manual review").
SELECT * FROM unique_id_fix ORDER BY skip DESC, "Id";

-- Payment and fee-allocation history first, keyed off the old value, so at no point does a row
-- exist pointing at a UniqueId that doesn't (yet, or any more) belong to any student.
UPDATE "StudentPayment" sp
SET "UniqueId" = f.new_unique_id
FROM unique_id_fix f
WHERE f.old_unique_id = sp."UniqueId"
  AND f.skip = false;

UPDATE "StudentFeeAllocations" sfa
SET "UniqueId" = f.new_unique_id
FROM unique_id_fix f
WHERE f.old_unique_id = sfa."UniqueId"
  AND f.skip = false;

UPDATE "Student" s
SET "UniqueId" = f.new_unique_id
FROM unique_id_fix f
WHERE f."Id" = s."Id"
  AND f.skip = false;

COMMIT;

-- 2) Verify - should return only rows already flagged skip = true above (if any); those need a
--    human to correct the DOB/roll clash by hand before they can be fixed.
SELECT "Id", "Name", "ClassRoll", "UniqueId"
FROM "Student"
WHERE "UniqueId" LIKE '0%'
  AND "UniqueId" ~ '^[0-9]+$';
