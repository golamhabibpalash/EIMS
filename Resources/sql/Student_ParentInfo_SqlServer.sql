-- ============================================================================
-- Student: parents' Bangla names + NID images - SQL SERVER
-- ----------------------------------------------------------------------------
-- Adds four optional columns to [Student] used by Students Create/Edit/Details/Profile:
--   FatherNameBangla, MotherNameBangla  nvarchar(150)  Father's / Mother's Name (Bangla)
--   FatherNidImage,   MotherNidImage    nvarchar(100)  stored file name of the NID scan
-- Run on every SQL Server instance BEFORE deploying 2.0.0: the app reads these columns
-- on every student query. Idempotent (safe to re-run). Single batch, no GO.
-- ============================================================================

IF COL_LENGTH(N'dbo.Student', N'FatherNameBangla') IS NULL
    ALTER TABLE [dbo].[Student] ADD [FatherNameBangla] nvarchar(150) NULL;
IF COL_LENGTH(N'dbo.Student', N'MotherNameBangla') IS NULL
    ALTER TABLE [dbo].[Student] ADD [MotherNameBangla] nvarchar(150) NULL;
IF COL_LENGTH(N'dbo.Student', N'FatherNidImage') IS NULL
    ALTER TABLE [dbo].[Student] ADD [FatherNidImage] nvarchar(100) NULL;
IF COL_LENGTH(N'dbo.Student', N'MotherNidImage') IS NULL
    ALTER TABLE [dbo].[Student] ADD [MotherNidImage] nvarchar(100) NULL;
