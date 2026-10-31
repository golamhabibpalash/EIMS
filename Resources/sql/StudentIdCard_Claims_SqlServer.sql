-- ============================================================================
-- Student ID Card permission - SQL SERVER
-- Adds the "View Student ID Card" permission (policy IdCardStudentsPolicy) used by
-- the Students > ID Cards screen, under the StudentManagement > StudentRecords menu.
--
-- IDEMPOTENT: every INSERT is guarded, safe to re-run. Single batch, no GO
-- (shared-hosting SQL consoles reject GO).
--
-- Part 1 ensures the module/submodule rows exist (they normally already do from
--        Seed_Modules_SubModules_ClaimStores_FromSiteMap.sql).
-- Part 2 creates the ClaimStores row so the permission appears in
--        Administrations > User Profile and can be assigned to roles/users.
-- Part 3 (optional) grants the claim directly to one account for immediate access.
--        Edit @TargetEmail, or delete Part 3 and assign via the UI instead.
-- After running, the user must LOG OUT and back in for the new claim to take effect.
-- ============================================================================

-- ---- Part 1: module + submodule ----
IF NOT EXISTS (SELECT 1 FROM [ProjectModules] WHERE [ModuleName] = N'StudentManagement')
    INSERT INTO [ProjectModules] ([ModuleName], [Status], [Remarks], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'StudentManagement', 1, N'Students', N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
DECLARE @Module_StudentManagement INT = (SELECT [Id] FROM [ProjectModules] WHERE [ModuleName] = N'StudentManagement');

IF NOT EXISTS (SELECT 1 FROM [ProjectSubModules] WHERE [SubModuleName] = N'StudentRecords' AND [ProjectModuleId] = @Module_StudentManagement)
    INSERT INTO [ProjectSubModules] ([SubModuleName], [ProjectModuleId], [Status], [Remarks], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'StudentRecords', @Module_StudentManagement, 1, N'Records', N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
DECLARE @Sub_StudentRecords INT = (SELECT [Id] FROM [ProjectSubModules] WHERE [SubModuleName] = N'StudentRecords' AND [ProjectModuleId] = @Module_StudentManagement);

-- ---- Part 2: the permission ----
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'IdCardStudentsPolicy' AND [ClaimType] = N'View Student ID Card')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'View Student ID Card', N'IdCardStudentsPolicy', @Sub_StudentRecords, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');

-- ---- Part 3 (optional): grant to one account for immediate access ----
DECLARE @TargetEmail NVARCHAR(256) = N'admin@eims.com';   -- <<< change to the account that needs access
DECLARE @UserId NVARCHAR(450) = (SELECT Id FROM AspNetUsers WHERE Email = @TargetEmail);
IF @UserId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM AspNetUserClaims WHERE UserId = @UserId AND ClaimType = N'View Student ID Card')
    INSERT INTO AspNetUserClaims (UserId, ClaimType, ClaimValue)
    VALUES (@UserId, N'View Student ID Card', N'IdCardStudentsPolicy');
