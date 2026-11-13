/*  EIMS - unlock /MachineEnrollment ("User Enrollment") for a user.  SQL Server.

    Why the page says "Access Denied"
    ---------------------------------
    MachineEnrollmentController.Index is gated by [Authorize(Policy = "ManageMachineUsersPolicy")],
    which is RequireClaim("Manage Machine Users") (AuthorizationPolicies.cs:97).
    That claim has NO ClaimStores row on any database, so it never appears in the
    Administrations/UserProfile claim picker and can never be granted through the UI.
    The user therefore has the SuperAdmin/Admin role but not the claim, and the policy fails.

    What this does
    --------------
    Part 1  Creates the ProjectSubModules row 'AttendanceMachinesSetup' if missing.
    Part 2  Creates the ClaimStores row ('Manage Machine Users' -> 'ManageMachineUsersPolicy')
            so the permission shows up in Administrations/UserProfile from now on.
    Part 3  Grants the claim directly to @TargetEmail so the page works immediately.

    Idempotent - safe to re-run. Set @TargetEmail below.
    LOG OUT AND BACK IN afterwards: claims are baked into the auth cookie at sign-in.
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @TargetEmail nvarchar(256) = N'admin@eims.com';   -- <<< change to the account that needs access

DECLARE @UserId nvarchar(450) = (SELECT Id FROM AspNetUsers WHERE Email = @TargetEmail);
IF @UserId IS NULL
BEGIN
    RAISERROR('User %s not found in AspNetUsers.', 16, 1, @TargetEmail);
    RETURN;
END

BEGIN TRANSACTION;

/* ---- Part 1: sub-module that owns the fingerprint-machine permissions ---- */
DECLARE @SubModuleId int =
    (SELECT TOP 1 Id FROM ProjectSubModules WHERE SubModuleName = N'AttendanceMachinesSetup');

IF @SubModuleId IS NULL
BEGIN
    DECLARE @AttendanceModuleId int =
        (SELECT TOP 1 Id FROM ProjectModules WHERE ModuleName = N'Attendance');

    IF @AttendanceModuleId IS NULL
    BEGIN
        INSERT INTO ProjectModules (ModuleName,Status,Remarks,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress)
        VALUES (N'Attendance',1,N'Attendance Related',N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
        SET @AttendanceModuleId = SCOPE_IDENTITY();
    END

    INSERT INTO ProjectSubModules (SubModuleName,ProjectModuleId,Status,Remarks,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress)
    VALUES (N'AttendanceMachinesSetup',@AttendanceModuleId,1,N'Fingerprint machine setup',N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    SET @SubModuleId = SCOPE_IDENTITY();
    PRINT 'Created ProjectSubModules row AttendanceMachinesSetup.';
END

/* ---- Part 2: make the permission assignable from Administrations/UserProfile ---- */
IF NOT EXISTS (SELECT 1 FROM ClaimStores WHERE ClaimType = N'Manage Machine Users')
BEGIN
    INSERT INTO ClaimStores (ClaimType,ClaimValue,SubModuleId,CreatedBy,CreatedAt,EditedBy,EditedAt,MACAddress)
    VALUES (N'Manage Machine Users',N'ManageMachineUsersPolicy',@SubModuleId,N'seed',SYSUTCDATETIME(),N'seed',SYSUTCDATETIME(),N'');
    PRINT 'Created ClaimStores row: Manage Machine Users.';
END
ELSE
    PRINT 'ClaimStores already has Manage Machine Users - skipped.';

/* ---- Part 3: grant the claim to the target user ---- */
IF NOT EXISTS (SELECT 1 FROM AspNetUserClaims
               WHERE UserId = @UserId AND ClaimType = N'Manage Machine Users')
BEGIN
    INSERT INTO AspNetUserClaims (UserId, ClaimType, ClaimValue)
    VALUES (@UserId, N'Manage Machine Users', N'ManageMachineUsersPolicy');
    PRINT 'Granted Manage Machine Users to ' + @TargetEmail + '.';
END
ELSE
    PRINT 'User already holds Manage Machine Users - skipped.';

COMMIT;

/* ---- Verify: the controller needs BOTH the role and the claim ---- */
SELECT @TargetEmail AS Account,
       (SELECT COUNT(*) FROM AspNetUserClaims
        WHERE UserId = @UserId AND ClaimType = N'Manage Machine Users')      AS HasClaim,
       (SELECT COUNT(*) FROM AspNetUserRoles ur
        JOIN AspNetRoles r ON r.Id = ur.RoleId
        WHERE ur.UserId = @UserId AND r.Name IN (N'SuperAdmin', N'Admin'))   AS HasRequiredRole;
