-- ============================================================================
-- "Reset User Password" permission - SQL SERVER
-- Adds the permission (policy ResetUserPasswordPolicy) used by Admin > Security >
-- Reset Password, so it can be given to roles/users in Administrations > User Profile,
-- and grants it to every SuperAdmin account.
-- IDEMPOTENT: safe to re-run. Single batch, no GO. Optional: the app runs without it,
-- but nobody gets the menu until the permission exists and is assigned.
-- Users must LOG OUT and back in (or wait up to 15 minutes) for it to take effect.
-- ============================================================================

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;

IF NOT EXISTS (SELECT 1 FROM [ProjectModules] WHERE [ModuleName] = N'Administration')
    INSERT INTO [ProjectModules] ([ModuleName], [Status], [Remarks], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Administration', 1, N'Admin', N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
DECLARE @Module_Administration INT = (SELECT [Id] FROM [ProjectModules] WHERE [ModuleName] = N'Administration');

IF NOT EXISTS (SELECT 1 FROM [ProjectSubModules] WHERE [SubModuleName] = N'SecuritySetup' AND [ProjectModuleId] = @Module_Administration)
    INSERT INTO [ProjectSubModules] ([SubModuleName], [ProjectModuleId], [Status], [Remarks], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'SecuritySetup', @Module_Administration, 1, N'Security', N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
DECLARE @Sub_SecuritySetup INT = (SELECT [Id] FROM [ProjectSubModules] WHERE [SubModuleName] = N'SecuritySetup' AND [ProjectModuleId] = @Module_Administration);

IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'ResetUserPasswordPolicy' AND [ClaimType] = N'Reset User Password')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Reset User Password', N'ResetUserPasswordPolicy', @Sub_SecuritySetup, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');

-- Grant to every SuperAdmin.
INSERT INTO [AspNetUserClaims] ([UserId], [ClaimType], [ClaimValue])
SELECT ur.[UserId], N'Reset User Password', N'ResetUserPasswordPolicy'
FROM [AspNetUserRoles] ur
JOIN [AspNetRoles] r ON r.[Id] = ur.[RoleId] AND r.[NormalizedName] = N'SUPERADMIN'
WHERE NOT EXISTS (SELECT 1 FROM [AspNetUserClaims] c
                  WHERE c.[UserId] = ur.[UserId] AND c.[ClaimType] = N'Reset User Password');
