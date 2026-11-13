/* =====================================================================
   EIMS - unlock /MachineEnrollment ("User Enrollment").  SQL Server.
   Web-console version of 2026-08-30_AddManageMachineUsers_Claim.sql:
   no DECLARE, no PRINT, no RAISERROR, no explicit transaction.
   Every step is ONE self-contained statement - paste and run them in
   order, one at a time, in Plesk / cPanel / any restricted SQL editor.

   Why the page says "Access Denied"
   ---------------------------------
   MachineEnrollmentController.Index is gated by
   [Authorize(Policy = "ManageMachineUsersPolicy")], which is
   RequireClaim("Manage Machine Users") (AuthorizationPolicies.cs:97).
   That claim has no ClaimStores row on any database, so it never appears
   in the Administrations/UserProfile picker and was never grantable.

   BEFORE YOU START: replace admin@eims.com in steps 0, 4 and 5 with the
   account that needs access. It appears once per statement.

   Every step is idempotent - re-running one changes nothing.
   LOG OUT AND BACK IN at the end: claims are baked into the auth cookie
   at sign-in, so an open session keeps showing Access Denied.
   ===================================================================== */


/* ---- STEP 0: read-only. Confirm the account exists and see what it has.
        Expect 1 row. HasClaim should be 0 (that is the bug); if
        HasRequiredRole is 0 the account also needs the SuperAdmin or Admin
        role, which steps 1-5 do NOT grant - see the note at the bottom. */

SELECT u.Email,
       (SELECT COUNT(*) FROM AspNetUserClaims c
        WHERE c.UserId = u.Id AND c.ClaimType = N'Manage Machine Users')    AS HasClaim,
       (SELECT COUNT(*) FROM AspNetUserRoles ur
          JOIN AspNetRoles r ON r.Id = ur.RoleId
        WHERE ur.UserId = u.Id AND r.Name IN (N'SuperAdmin', N'Admin'))     AS HasRequiredRole
FROM AspNetUsers u
WHERE u.Email = N'admin@eims.com';


/* ---- STEP 1: parent module. Almost certainly already there (0 rows
        affected); this only matters on a bare database. */

INSERT INTO ProjectModules
       (ModuleName, Status, Remarks, CreatedBy, CreatedAt, EditedBy, EditedAt, MACAddress)
SELECT N'Attendance', 1, N'Attendance Related',
       N'seed', SYSUTCDATETIME(), N'seed', SYSUTCDATETIME(), N''
WHERE NOT EXISTS (SELECT 1 FROM ProjectModules WHERE ModuleName = N'Attendance');


/* ---- STEP 2: sub-module the fingerprint-machine permissions hang off.
        Existing databases already have it (seeded as Id 40) -> 0 rows. */

INSERT INTO ProjectSubModules
       (SubModuleName, ProjectModuleId, Status, Remarks, CreatedBy, CreatedAt, EditedBy, EditedAt, MACAddress)
SELECT N'AttendanceMachinesSetup',
       (SELECT MIN(Id) FROM ProjectModules WHERE ModuleName = N'Attendance'),
       1, N'Fingerprint machine setup',
       N'seed', SYSUTCDATETIME(), N'seed', SYSUTCDATETIME(), N''
WHERE NOT EXISTS (SELECT 1 FROM ProjectSubModules WHERE SubModuleName = N'AttendanceMachinesSetup')
  AND EXISTS     (SELECT 1 FROM ProjectModules    WHERE ModuleName    = N'Attendance');


/* ---- STEP 3: the actual fix. Adds the missing permission so it becomes
        assignable from Administrations -> User Profile from now on.
        Expect 1 row affected. */

INSERT INTO ClaimStores
       (ClaimType, ClaimValue, SubModuleId, CreatedBy, CreatedAt, EditedBy, EditedAt, MACAddress)
SELECT N'Manage Machine Users', N'ManageMachineUsersPolicy',
       (SELECT MIN(Id) FROM ProjectSubModules WHERE SubModuleName = N'AttendanceMachinesSetup'),
       N'seed', SYSUTCDATETIME(), N'seed', SYSUTCDATETIME(), N''
WHERE NOT EXISTS (SELECT 1 FROM ClaimStores       WHERE ClaimType    = N'Manage Machine Users')
  AND EXISTS     (SELECT 1 FROM ProjectSubModules WHERE SubModuleName = N'AttendanceMachinesSetup');


/* ---- STEP 4: grant the claim to your account so the page works now,
        without waiting to tick it in the UI.  <<< EDIT THE EMAIL
        Expect 1 row affected. 0 rows means the email did not match any
        AspNetUsers row (check spelling) or the claim was already granted. */

INSERT INTO AspNetUserClaims (UserId, ClaimType, ClaimValue)
SELECT u.Id, N'Manage Machine Users', N'ManageMachineUsersPolicy'
FROM AspNetUsers u
WHERE u.Email = N'admin@eims.com'
  AND NOT EXISTS (SELECT 1 FROM AspNetUserClaims c
                  WHERE c.UserId = u.Id AND c.ClaimType = N'Manage Machine Users');


/* ---- STEP 5: verify. Same query as step 0.  <<< EDIT THE EMAIL
        HasClaim must now be 1. HasRequiredRole must also be 1 - the
        controller carries [Authorize(Roles = "SuperAdmin, Admin")] on top
        of the policy, so both gates have to pass. */

SELECT u.Email,
       (SELECT COUNT(*) FROM AspNetUserClaims c
        WHERE c.UserId = u.Id AND c.ClaimType = N'Manage Machine Users')    AS HasClaim,
       (SELECT COUNT(*) FROM AspNetUserRoles ur
          JOIN AspNetRoles r ON r.Id = ur.RoleId
        WHERE ur.UserId = u.Id AND r.Name IN (N'SuperAdmin', N'Admin'))     AS HasRequiredRole
FROM AspNetUsers u
WHERE u.Email = N'admin@eims.com';


/* ---------------------------------------------------------------------
   If HasRequiredRole came back 0, the account is missing the role, not the
   claim. Grant it through Administrations -> Assign Roles in the app
   rather than by hand here, so ASP.NET Identity keeps its own bookkeeping
   consistent.

   Other users: now that step 3 has run, tick "Manage Machine Users" for
   them under Administrations -> User Profile. No SQL needed.
   --------------------------------------------------------------------- */
