-- ============================================================================
-- Student Testimonial + Transfer Certificate GENERATORS - PERMISSIONS (SQL SERVER)
-- Both are on-demand PDF certificates (nothing is stored). Only three claims are
-- used, seeded under StudentManagement > StudentDocuments so they appear in
-- Administrations > User Profile. IDEMPOTENT. Single batch, no GO.
-- Part 3 optionally grants all three to one account (edit @TargetEmail). Log out/in after.
-- (If you ran the earlier 11-claim version, see TestimonialsAndTC_Cleanup_SqlServer.sql
--  to remove the 8 now-unused claims and the unused tables - optional.)
-- ============================================================================

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;

-- ---- Part 1: module + submodule ----
IF NOT EXISTS (SELECT 1 FROM [ProjectModules] WHERE [ModuleName] = N'StudentManagement')
    INSERT INTO [ProjectModules] ([ModuleName], [Status], [Remarks], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'StudentManagement', 1, N'Students', N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
DECLARE @Module_StudentManagement INT = (SELECT [Id] FROM [ProjectModules] WHERE [ModuleName] = N'StudentManagement');

IF NOT EXISTS (SELECT 1 FROM [ProjectSubModules] WHERE [SubModuleName] = N'StudentDocuments' AND [ProjectModuleId] = @Module_StudentManagement)
    INSERT INTO [ProjectSubModules] ([SubModuleName], [ProjectModuleId], [Status], [Remarks], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'StudentDocuments', @Module_StudentManagement, 1, N'Certificates & Testimonials', N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
DECLARE @Sub INT = (SELECT [Id] FROM [ProjectSubModules] WHERE [SubModuleName] = N'StudentDocuments' AND [ProjectModuleId] = @Module_StudentManagement);

-- ---- Part 2: the three live permissions ----
INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
SELECT c.ClaimType, c.ClaimValue, @Sub, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N''
FROM (VALUES
    (N'View Testimonials',             N'IndexTestimonialsPolicy'),
    (N'View Transfer Certificates',    N'IndexTransferCertificatesPolicy'),
    (N'Generate Transfer Certificate', N'GenerateTransferCertificatesPolicy')
) c(ClaimType, ClaimValue)
WHERE NOT EXISTS (SELECT 1 FROM [ClaimStores] cs WHERE cs.ClaimValue = c.ClaimValue AND cs.ClaimType = c.ClaimType);

-- ---- Part 3 (optional): grant to one account ----
DECLARE @TargetEmail NVARCHAR(256) = N'admin@eims.com';   -- <<< change to the account that needs access
DECLARE @UserId NVARCHAR(450) = (SELECT Id FROM AspNetUsers WHERE Email = @TargetEmail);
IF @UserId IS NOT NULL
    INSERT INTO AspNetUserClaims (UserId, ClaimType, ClaimValue)
    SELECT @UserId, cs.ClaimType, cs.ClaimValue
    FROM [ClaimStores] cs
    WHERE cs.ClaimValue IN (N'IndexTestimonialsPolicy', N'IndexTransferCertificatesPolicy', N'GenerateTransferCertificatesPolicy')
      AND NOT EXISTS (SELECT 1 FROM AspNetUserClaims uc WHERE uc.UserId = @UserId AND uc.ClaimType = cs.ClaimType);
