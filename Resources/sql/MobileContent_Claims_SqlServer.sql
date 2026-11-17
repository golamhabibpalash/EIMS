-- ============================================================================
-- Mobile Notice + Mobile Slider permissions - SQL SERVER
-- Adds 8 permissions (view/create/edit/delete for notices and for the slider) used by
-- Communications > SMS Panel > Notice / Student Slider (MobileContentController), so they
-- can be given to users in Administrations > User Profile, and grants all of them to the
-- SuperAdmin and Admin roles. Run MobileContent_SqlServer.sql first (it creates the tables).
-- IDEMPOTENT: safe to re-run. Single batch, no GO.
-- Users must LOG OUT and back in (or wait up to 15 minutes) for new permissions to apply.
-- ============================================================================

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;

IF NOT EXISTS (SELECT 1 FROM [ProjectModules] WHERE [ModuleName] = N'Communication')
    INSERT INTO [ProjectModules] ([ModuleName], [Status], [Remarks], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Communication', 1, N'Communications', N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
DECLARE @Module_Communication INT = (SELECT [Id] FROM [ProjectModules] WHERE [ModuleName] = N'Communication');

IF NOT EXISTS (SELECT 1 FROM [ProjectSubModules] WHERE [SubModuleName] = N'Messaging' AND [ProjectModuleId] = @Module_Communication)
    INSERT INTO [ProjectSubModules] ([SubModuleName], [ProjectModuleId], [Status], [Remarks], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Messaging', @Module_Communication, 1, N'SMS Panel', N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
DECLARE @Sub_Messaging INT = (SELECT [Id] FROM [ProjectSubModules] WHERE [SubModuleName] = N'Messaging' AND [ProjectModuleId] = @Module_Communication);

DECLARE @Perm TABLE (ClaimType NVARCHAR(256), ClaimValue NVARCHAR(256));
INSERT INTO @Perm (ClaimType, ClaimValue) VALUES
    (N'View Mobile Notices', N'IndexMobileNoticesPolicy'),
    (N'Create Mobile Notice', N'CreateMobileNoticesPolicy'),
    (N'Edit Mobile Notice', N'EditMobileNoticesPolicy'),
    (N'Delete Mobile Notice', N'DeleteMobileNoticesPolicy'),
    (N'View Mobile Slider', N'IndexMobileSlidersPolicy'),
    (N'Create Mobile Slider', N'CreateMobileSlidersPolicy'),
    (N'Edit Mobile Slider', N'EditMobileSlidersPolicy'),
    (N'Delete Mobile Slider', N'DeleteMobileSlidersPolicy');

INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
SELECT p.ClaimType, p.ClaimValue, @Sub_Messaging, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N''
FROM @Perm p
WHERE NOT EXISTS (SELECT 1 FROM [ClaimStores] c WHERE c.[ClaimValue] = p.ClaimValue AND c.[ClaimType] = p.ClaimType);

-- Grant all of them to the SuperAdmin and Admin ROLES (role claims), so every current and
-- future SuperAdmin/Admin has them. Other users get them one by one in User Profile.
INSERT INTO [AspNetRoleClaims] ([RoleId], [ClaimType], [ClaimValue])
SELECT r.[Id], p.ClaimType, p.ClaimValue
FROM [AspNetRoles] r
CROSS JOIN @Perm p
WHERE r.[NormalizedName] IN (N'SUPERADMIN', N'ADMIN')
  AND NOT EXISTS (SELECT 1 FROM [AspNetRoleClaims] c
                  WHERE c.[RoleId] = r.[Id] AND c.[ClaimType] = p.ClaimType AND c.[ClaimValue] = p.ClaimValue);
