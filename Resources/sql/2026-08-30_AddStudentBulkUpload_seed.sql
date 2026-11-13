/* =====================================================================
   EIMS - Bulk student upload: permission row.

   Adds the ClaimStores row behind BulkUploadStudentsPolicy so the new
   Students > Bulk Upload screen can be granted from
   Admin > Security > User Profile, then grants it to everyone who can
   already create students (they are the same people).

   No schema change is needed - the feature reuses the Students table.
   No GO, no BEGIN/END, safe to re-run.
   ===================================================================== */

SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

/* ---- 1. Claim row, filed under the same sub-module as the other
          student permissions so it appears beside them in the picker. --- */
INSERT INTO dbo.ClaimStores (ClaimType, ClaimValue, SubModuleId, CreatedAt, EditedAt)
SELECT 'Bulk Upload Students', 'BulkUploadStudentsPolicy', s.SubModuleId, GETDATE(), GETDATE()
FROM (
        SELECT TOP 1 SubModuleId
        FROM dbo.ClaimStores
        WHERE ClaimValue = 'CreateStudentsPolicy'
     ) AS s
WHERE NOT EXISTS (
        SELECT 1 FROM dbo.ClaimStores WHERE ClaimValue = 'BulkUploadStudentsPolicy');

/* Fall back to any student sub-module if CreateStudentsPolicy is absent. */
INSERT INTO dbo.ClaimStores (ClaimType, ClaimValue, SubModuleId, CreatedAt, EditedAt)
SELECT 'Bulk Upload Students', 'BulkUploadStudentsPolicy', s.Id, GETDATE(), GETDATE()
FROM (SELECT TOP 1 Id FROM dbo.ProjectSubModules WHERE SubModuleName LIKE '%Student%' ORDER BY Id) AS s
WHERE NOT EXISTS (
        SELECT 1 FROM dbo.ClaimStores WHERE ClaimValue = 'BulkUploadStudentsPolicy');

/* ---- 2. Grant it to every user who can already create a student. ------ */
INSERT INTO dbo.AspNetUserClaims (UserId, ClaimType, ClaimValue)
SELECT uc.UserId, 'Bulk Upload Students', 'BulkUploadStudentsPolicy'
FROM dbo.AspNetUserClaims uc
WHERE uc.ClaimType = 'Create Student'
  AND NOT EXISTS (
        SELECT 1 FROM dbo.AspNetUserClaims x
        WHERE x.UserId = uc.UserId AND x.ClaimType = 'Bulk Upload Students');

/* ---- 3. Verification -------------------------------------------------- */
SELECT
    Claim_Row     = CASE WHEN EXISTS (SELECT 1 FROM dbo.ClaimStores
                                      WHERE ClaimValue = 'BulkUploadStudentsPolicy')
                         THEN 'OK' ELSE 'MISSING' END,
    Users_Granted = (SELECT COUNT(*) FROM dbo.AspNetUserClaims
                     WHERE ClaimType = 'Bulk Upload Students');
