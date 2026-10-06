-- ============================================================================
-- Seed ProjectModules / ProjectSubModules / ClaimStores from SiteMap.Config
-- Generated from SMS_App/SiteMap.Config + AuthorizationPolicies.cs. IDEMPOTENT:
-- every INSERT is guarded by IF NOT EXISTS, so re-running is safe and rows
-- already created (modules, submodules, permissions) are skipped untouched.
-- Existing rows are NEVER updated (Status flags you changed by hand are kept).
-- New modules/submodules are created with Status = 1 (visible).
-- After running: assign the claims to roles/users via Admin > User Profile.
-- Target: SQL Server. Tables: ProjectModules, ProjectSubModules, ClaimStores.
-- Single batch, no GO separators: GO is an SSMS/sqlcmd client keyword, not
-- T-SQL, and shared-hosting SQL consoles reject it. All DECLAREd variables are
-- uniquely named per module/submodule, so this runs as one batch anywhere.
-- ============================================================================

-- NOTE: <item StudentRegistration Claim=CreateStudentsPolicy is a direct child of module StudentManagement and is ignored by SiteMapLoader (never renders); claim row attached to StudentRecords.
-- NOTE: <item StudentProfile Claim=IndexStudentsPolicy is a direct child of module StudentManagement and is ignored by SiteMapLoader (never renders); claim row attached to StudentRecords.
-- WARNING: SiteMap Claim ViewStudentFeeAllocationsPolicy has NO matching policy in AuthorizationPolicies.cs (page will 403 even with the claim).

-- Module: Administration (Admin)
IF NOT EXISTS (SELECT 1 FROM [ProjectModules] WHERE [ModuleName] = N'Administration')
BEGIN
    INSERT INTO [ProjectModules] ([ModuleName], [Status], [Remarks], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Administration', 1, N'Admin', N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
END
DECLARE @Module_Administration INT = (SELECT [Id] FROM [ProjectModules] WHERE [ModuleName] = N'Administration');
-- Submodule: InstituteSetup (Institute)
IF NOT EXISTS (SELECT 1 FROM [ProjectSubModules] WHERE [SubModuleName] = N'InstituteSetup' AND [ProjectModuleId] = @Module_Administration)
    INSERT INTO [ProjectSubModules] ([SubModuleName], [ProjectModuleId], [Status], [Remarks], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'InstituteSetup', @Module_Administration, 1, N'Institute', N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
DECLARE @Sub_InstituteSetup_Administration INT = (SELECT [Id] FROM [ProjectSubModules] WHERE [SubModuleName] = N'InstituteSetup' AND [ProjectModuleId] = @Module_Administration);
-- Permission: IndexInstitutesPolicy (from SiteMap.Config menu item)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'IndexInstitutesPolicy' AND [ClaimType] = N'View Institutes Info')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'View Institutes Info', N'IndexInstitutesPolicy', @Sub_InstituteSetup_Administration, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: IndexOffDayTypesPolicy (from SiteMap.Config menu item)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'IndexOffDayTypesPolicy' AND [ClaimType] = N'View Off Day Types')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'View Off Day Types', N'IndexOffDayTypesPolicy', @Sub_InstituteSetup_Administration, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: IndexOffDaysPolicy (from SiteMap.Config menu item)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'IndexOffDaysPolicy' AND [ClaimType] = N'View Off Days')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'View Off Days', N'IndexOffDaysPolicy', @Sub_InstituteSetup_Administration, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: IndexParamBusConfigPolicy (from SiteMap.Config menu item)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'IndexParamBusConfigPolicy' AND [ClaimType] = N'View Config Data')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'View Config Data', N'IndexParamBusConfigPolicy', @Sub_InstituteSetup_Administration, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: IndexBloodGroupsPolicy (generic lookup table; regroup in Claim List UI if desired)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'IndexBloodGroupsPolicy' AND [ClaimType] = N'View Blood Groups')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'View Blood Groups', N'IndexBloodGroupsPolicy', @Sub_InstituteSetup_Administration, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: DetailsBloodGroupsPolicy (generic lookup table; regroup in Claim List UI if desired)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'DetailsBloodGroupsPolicy' AND [ClaimType] = N'Details Blood Groups')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Details Blood Groups', N'DetailsBloodGroupsPolicy', @Sub_InstituteSetup_Administration, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: CreateBloodGroupsPolicy (generic lookup table; regroup in Claim List UI if desired)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'CreateBloodGroupsPolicy' AND [ClaimType] = N'Create  Blood Groups')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Create  Blood Groups', N'CreateBloodGroupsPolicy', @Sub_InstituteSetup_Administration, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: EditBloodGroupsPolicy (generic lookup table; regroup in Claim List UI if desired)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'EditBloodGroupsPolicy' AND [ClaimType] = N'Edit Blood Groups')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Edit Blood Groups', N'EditBloodGroupsPolicy', @Sub_InstituteSetup_Administration, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: DeleteBloodGroupsPolicy (generic lookup table; regroup in Claim List UI if desired)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'DeleteBloodGroupsPolicy' AND [ClaimType] = N'Delete Blood Groups')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Delete Blood Groups', N'DeleteBloodGroupsPolicy', @Sub_InstituteSetup_Administration, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: IndexDistrictsPolicy (generic lookup table; regroup in Claim List UI if desired)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'IndexDistrictsPolicy' AND [ClaimType] = N'View Districts')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'View Districts', N'IndexDistrictsPolicy', @Sub_InstituteSetup_Administration, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: DetailsDistrictsPolicy (generic lookup table; regroup in Claim List UI if desired)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'DetailsDistrictsPolicy' AND [ClaimType] = N'Details Districts')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Details Districts', N'DetailsDistrictsPolicy', @Sub_InstituteSetup_Administration, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: CreateDistrictsPolicy (generic lookup table; regroup in Claim List UI if desired)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'CreateDistrictsPolicy' AND [ClaimType] = N'Create Districts')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Create Districts', N'CreateDistrictsPolicy', @Sub_InstituteSetup_Administration, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: EditDistrictsPolicy (generic lookup table; regroup in Claim List UI if desired)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'EditDistrictsPolicy' AND [ClaimType] = N'Edit Districts')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Edit Districts', N'EditDistrictsPolicy', @Sub_InstituteSetup_Administration, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: DeleteDistrictsPolicy (generic lookup table; regroup in Claim List UI if desired)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'DeleteDistrictsPolicy' AND [ClaimType] = N'Delete Districts')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Delete Districts', N'DeleteDistrictsPolicy', @Sub_InstituteSetup_Administration, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: IndexDivisionsPolicy (generic lookup table; regroup in Claim List UI if desired)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'IndexDivisionsPolicy' AND [ClaimType] = N'View Divisions')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'View Divisions', N'IndexDivisionsPolicy', @Sub_InstituteSetup_Administration, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: DetailsDivisionsPolicy (generic lookup table; regroup in Claim List UI if desired)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'DetailsDivisionsPolicy' AND [ClaimType] = N'Details Divisions')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Details Divisions', N'DetailsDivisionsPolicy', @Sub_InstituteSetup_Administration, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: CreateDivisionsPolicy (generic lookup table; regroup in Claim List UI if desired)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'CreateDivisionsPolicy' AND [ClaimType] = N'Create Divisions')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Create Divisions', N'CreateDivisionsPolicy', @Sub_InstituteSetup_Administration, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: EditDivisionsPolicy (generic lookup table; regroup in Claim List UI if desired)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'EditDivisionsPolicy' AND [ClaimType] = N'Edit Divisions')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Edit Divisions', N'EditDivisionsPolicy', @Sub_InstituteSetup_Administration, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: DeleteDivisionsPolicy (generic lookup table; regroup in Claim List UI if desired)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'DeleteDivisionsPolicy' AND [ClaimType] = N'Delete Divisions')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Delete Divisions', N'DeleteDivisionsPolicy', @Sub_InstituteSetup_Administration, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: IndexGendersPolicy (generic lookup table; regroup in Claim List UI if desired)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'IndexGendersPolicy' AND [ClaimType] = N'View Genders')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'View Genders', N'IndexGendersPolicy', @Sub_InstituteSetup_Administration, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: DetailsGendersPolicy (generic lookup table; regroup in Claim List UI if desired)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'DetailsGendersPolicy' AND [ClaimType] = N'Details Genders')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Details Genders', N'DetailsGendersPolicy', @Sub_InstituteSetup_Administration, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: CreateGendersPolicy (generic lookup table; regroup in Claim List UI if desired)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'CreateGendersPolicy' AND [ClaimType] = N'Create Genders')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Create Genders', N'CreateGendersPolicy', @Sub_InstituteSetup_Administration, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: EditGendersPolicy (generic lookup table; regroup in Claim List UI if desired)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'EditGendersPolicy' AND [ClaimType] = N'Edit Genders')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Edit Genders', N'EditGendersPolicy', @Sub_InstituteSetup_Administration, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: DeleteGendersPolicy (generic lookup table; regroup in Claim List UI if desired)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'DeleteGendersPolicy' AND [ClaimType] = N'Delete Genders')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Delete Genders', N'DeleteGendersPolicy', @Sub_InstituteSetup_Administration, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: CreateInstitutesPolicy (page action for the Institutes screens)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'CreateInstitutesPolicy' AND [ClaimType] = N'Create Institutes Info')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Create Institutes Info', N'CreateInstitutesPolicy', @Sub_InstituteSetup_Administration, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: EditInstitutesPolicy (page action for the Institutes screens)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'EditInstitutesPolicy' AND [ClaimType] = N'Edit Institutes Info')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Edit Institutes Info', N'EditInstitutesPolicy', @Sub_InstituteSetup_Administration, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: SchoolTimeTableInstitutesPolicy (school time setting page)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'SchoolTimeTableInstitutesPolicy' AND [ClaimType] = N'Edit School Time Table')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Edit School Time Table', N'SchoolTimeTableInstitutesPolicy', @Sub_InstituteSetup_Administration, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: IndexNationalitiesPolicy (generic lookup table; regroup in Claim List UI if desired)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'IndexNationalitiesPolicy' AND [ClaimType] = N'View Nationalities')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'View Nationalities', N'IndexNationalitiesPolicy', @Sub_InstituteSetup_Administration, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: DetailsNationalitiesPolicy (generic lookup table; regroup in Claim List UI if desired)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'DetailsNationalitiesPolicy' AND [ClaimType] = N'Details Nationalities')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Details Nationalities', N'DetailsNationalitiesPolicy', @Sub_InstituteSetup_Administration, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: CreateNationalitiesPolicy (generic lookup table; regroup in Claim List UI if desired)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'CreateNationalitiesPolicy' AND [ClaimType] = N'Create Nationalities')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Create Nationalities', N'CreateNationalitiesPolicy', @Sub_InstituteSetup_Administration, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: EditNationalitiesPolicy (generic lookup table; regroup in Claim List UI if desired)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'EditNationalitiesPolicy' AND [ClaimType] = N'Edit Nationalities')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Edit Nationalities', N'EditNationalitiesPolicy', @Sub_InstituteSetup_Administration, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: DeleteNationalitiesPolicy (generic lookup table; regroup in Claim List UI if desired)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'DeleteNationalitiesPolicy' AND [ClaimType] = N'Delete Nationalities')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Delete Nationalities', N'DeleteNationalitiesPolicy', @Sub_InstituteSetup_Administration, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: DetailsOffDaysPolicy (page action for the OffDays screens)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'DetailsOffDaysPolicy' AND [ClaimType] = N'Details Off Days')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Details Off Days', N'DetailsOffDaysPolicy', @Sub_InstituteSetup_Administration, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: CreateOffDaysPolicy (page action for the OffDays screens)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'CreateOffDaysPolicy' AND [ClaimType] = N'Create Off Days')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Create Off Days', N'CreateOffDaysPolicy', @Sub_InstituteSetup_Administration, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: EditOffDaysPolicy (page action for the OffDays screens)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'EditOffDaysPolicy' AND [ClaimType] = N'Edit Off Days')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Edit Off Days', N'EditOffDaysPolicy', @Sub_InstituteSetup_Administration, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: DeleteOffDaysPolicy (page action for the OffDays screens)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'DeleteOffDaysPolicy' AND [ClaimType] = N'Delete Off Days')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Delete Off Days', N'DeleteOffDaysPolicy', @Sub_InstituteSetup_Administration, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: DetailsOffDayTypesPolicy (page action for the OffDayTypes screens)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'DetailsOffDayTypesPolicy' AND [ClaimType] = N'Details Off Day Types')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Details Off Day Types', N'DetailsOffDayTypesPolicy', @Sub_InstituteSetup_Administration, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: CreateOffDayTypesPolicy (page action for the OffDayTypes screens)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'CreateOffDayTypesPolicy' AND [ClaimType] = N'Create Off Day Types')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Create Off Day Types', N'CreateOffDayTypesPolicy', @Sub_InstituteSetup_Administration, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: EditOffDayTypesPolicy (page action for the OffDayTypes screens)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'EditOffDayTypesPolicy' AND [ClaimType] = N'Edit Off Day Types')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Edit Off Day Types', N'EditOffDayTypesPolicy', @Sub_InstituteSetup_Administration, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: DeleteOffDayTypesPolicy (page action for the OffDayTypes screens)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'DeleteOffDayTypesPolicy' AND [ClaimType] = N'Delete Off Day Types')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Delete Off Day Types', N'DeleteOffDayTypesPolicy', @Sub_InstituteSetup_Administration, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: UpSertParamBusConfigPolicy (page action for the ParamBusConfig screens)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'UpSertParamBusConfigPolicy' AND [ClaimType] = N'Create/Update Config Data')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Create/Update Config Data', N'UpSertParamBusConfigPolicy', @Sub_InstituteSetup_Administration, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: IndexReligionsPolicy (generic lookup table; regroup in Claim List UI if desired)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'IndexReligionsPolicy' AND [ClaimType] = N'View Religions')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'View Religions', N'IndexReligionsPolicy', @Sub_InstituteSetup_Administration, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: DetailsReligionsPolicy (generic lookup table; regroup in Claim List UI if desired)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'DetailsReligionsPolicy' AND [ClaimType] = N'Details Religions')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Details Religions', N'DetailsReligionsPolicy', @Sub_InstituteSetup_Administration, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: CreateReligionsPolicy (generic lookup table; regroup in Claim List UI if desired)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'CreateReligionsPolicy' AND [ClaimType] = N'Create Religions')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Create Religions', N'CreateReligionsPolicy', @Sub_InstituteSetup_Administration, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: EditReligionsPolicy (generic lookup table; regroup in Claim List UI if desired)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'EditReligionsPolicy' AND [ClaimType] = N'Edit Religions')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Edit Religions', N'EditReligionsPolicy', @Sub_InstituteSetup_Administration, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: DeleteReligionsPolicy (generic lookup table; regroup in Claim List UI if desired)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'DeleteReligionsPolicy' AND [ClaimType] = N'Delete Religions')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Delete Religions', N'DeleteReligionsPolicy', @Sub_InstituteSetup_Administration, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: IndexUpazilasPolicy (generic lookup table; regroup in Claim List UI if desired)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'IndexUpazilasPolicy' AND [ClaimType] = N'View Upazilas')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'View Upazilas', N'IndexUpazilasPolicy', @Sub_InstituteSetup_Administration, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: DetailsUpazilasPolicy (generic lookup table; regroup in Claim List UI if desired)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'DetailsUpazilasPolicy' AND [ClaimType] = N'View Details Upazila')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'View Details Upazila', N'DetailsUpazilasPolicy', @Sub_InstituteSetup_Administration, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: CreateUpazilasPolicy (generic lookup table; regroup in Claim List UI if desired)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'CreateUpazilasPolicy' AND [ClaimType] = N'Create Upazila')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Create Upazila', N'CreateUpazilasPolicy', @Sub_InstituteSetup_Administration, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: EditUpazilasPolicy (generic lookup table; regroup in Claim List UI if desired)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'EditUpazilasPolicy' AND [ClaimType] = N'Edit Upazila')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Edit Upazila', N'EditUpazilasPolicy', @Sub_InstituteSetup_Administration, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: DeleteUpazilasPolicy (generic lookup table; regroup in Claim List UI if desired)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'DeleteUpazilasPolicy' AND [ClaimType] = N'Delete Upazila')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Delete Upazila', N'DeleteUpazilasPolicy', @Sub_InstituteSetup_Administration, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Submodule: AcademicSetup (Academic)
IF NOT EXISTS (SELECT 1 FROM [ProjectSubModules] WHERE [SubModuleName] = N'AcademicSetup' AND [ProjectModuleId] = @Module_Administration)
    INSERT INTO [ProjectSubModules] ([SubModuleName], [ProjectModuleId], [Status], [Remarks], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'AcademicSetup', @Module_Administration, 1, N'Academic', N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
DECLARE @Sub_AcademicSetup_Administration INT = (SELECT [Id] FROM [ProjectSubModules] WHERE [SubModuleName] = N'AcademicSetup' AND [ProjectModuleId] = @Module_Administration);
-- Permission: IndexAcademicSessionPolicy (from SiteMap.Config menu item)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'IndexAcademicSessionPolicy' AND [ClaimType] = N'View Academic Session')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'View Academic Session', N'IndexAcademicSessionPolicy', @Sub_AcademicSetup_Administration, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: IndexAcademicClassesPolicy (from SiteMap.Config menu item)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'IndexAcademicClassesPolicy' AND [ClaimType] = N'View Academic Class')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'View Academic Class', N'IndexAcademicClassesPolicy', @Sub_AcademicSetup_Administration, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: IndexAcademicSectionPolicy (from SiteMap.Config menu item)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'IndexAcademicSectionPolicy' AND [ClaimType] = N'View Academic Section')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'View Academic Section', N'IndexAcademicSectionPolicy', @Sub_AcademicSetup_Administration, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: IndexAcademicSubjectPolicy (from SiteMap.Config menu item)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'IndexAcademicSubjectPolicy' AND [ClaimType] = N'View Academic Subject')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'View Academic Subject', N'IndexAcademicSubjectPolicy', @Sub_AcademicSetup_Administration, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: IndexGradingTablePolicy (from SiteMap.Config menu item)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'IndexGradingTablePolicy' AND [ClaimType] = N'View Grading Table')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'View Grading Table', N'IndexGradingTablePolicy', @Sub_AcademicSetup_Administration, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: DetailsAcademicClassesPolicy (page action for the AcademicClasses screens)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'DetailsAcademicClassesPolicy' AND [ClaimType] = N'Details View Academic Class')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Details View Academic Class', N'DetailsAcademicClassesPolicy', @Sub_AcademicSetup_Administration, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: CreateAcademicClassesPolicy (page action for the AcademicClasses screens)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'CreateAcademicClassesPolicy' AND [ClaimType] = N'Create Academic Class')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Create Academic Class', N'CreateAcademicClassesPolicy', @Sub_AcademicSetup_Administration, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: EditAcademicClassesPolicy (page action for the AcademicClasses screens)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'EditAcademicClassesPolicy' AND [ClaimType] = N'Edit Academic Class')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Edit Academic Class', N'EditAcademicClassesPolicy', @Sub_AcademicSetup_Administration, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: DetailsAcademicSectionPolicy (page action for the AcademicSection screens)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'DetailsAcademicSectionPolicy' AND [ClaimType] = N'Details Academic Section')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Details Academic Section', N'DetailsAcademicSectionPolicy', @Sub_AcademicSetup_Administration, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: CreateAcademicSectionPolicy (page action for the AcademicSection screens)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'CreateAcademicSectionPolicy' AND [ClaimType] = N'Create Academic Section')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Create Academic Section', N'CreateAcademicSectionPolicy', @Sub_AcademicSetup_Administration, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: EditAcademicSectionPolicy (page action for the AcademicSection screens)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'EditAcademicSectionPolicy' AND [ClaimType] = N'Edit Academic Section')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Edit Academic Section', N'EditAcademicSectionPolicy', @Sub_AcademicSetup_Administration, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: DeleteAcademicSectionPolicy (page action for the AcademicSection screens)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'DeleteAcademicSectionPolicy' AND [ClaimType] = N'Delete Academic Section')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Delete Academic Section', N'DeleteAcademicSectionPolicy', @Sub_AcademicSetup_Administration, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: DetailsAcademicSessionPolicy (page action for the AcademicSession screens)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'DetailsAcademicSessionPolicy' AND [ClaimType] = N'Details Academic Session')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Details Academic Session', N'DetailsAcademicSessionPolicy', @Sub_AcademicSetup_Administration, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: CreateAcademicSessionPolicy (page action for the AcademicSession screens)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'CreateAcademicSessionPolicy' AND [ClaimType] = N'Create Academic Session')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Create Academic Session', N'CreateAcademicSessionPolicy', @Sub_AcademicSetup_Administration, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: EditAcademicSessionPolicy (page action for the AcademicSession screens)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'EditAcademicSessionPolicy' AND [ClaimType] = N'Edit Academic Session')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Edit Academic Session', N'EditAcademicSessionPolicy', @Sub_AcademicSetup_Administration, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: DeleteAcademicSessionPolicy (page action for the AcademicSession screens)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'DeleteAcademicSessionPolicy' AND [ClaimType] = N'Delete Academic Session')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Delete Academic Session', N'DeleteAcademicSessionPolicy', @Sub_AcademicSetup_Administration, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: DetailsAcademicSubjectPolicy (page action for the AcademicSubject screens)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'DetailsAcademicSubjectPolicy' AND [ClaimType] = N'Details Academic Subject')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Details Academic Subject', N'DetailsAcademicSubjectPolicy', @Sub_AcademicSetup_Administration, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: CreateAcademicSubjectPolicy (page action for the AcademicSubject screens)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'CreateAcademicSubjectPolicy' AND [ClaimType] = N'Create Academic Subject')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Create Academic Subject', N'CreateAcademicSubjectPolicy', @Sub_AcademicSetup_Administration, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: EditAcademicSubjectPolicy (page action for the AcademicSubject screens)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'EditAcademicSubjectPolicy' AND [ClaimType] = N'Edit Academic Subject')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Edit Academic Subject', N'EditAcademicSubjectPolicy', @Sub_AcademicSetup_Administration, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: DeleteAcademicSubjectPolicy (page action for the AcademicSubject screens)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'DeleteAcademicSubjectPolicy' AND [ClaimType] = N'Delete Academic Subject')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Delete Academic Subject', N'DeleteAcademicSubjectPolicy', @Sub_AcademicSetup_Administration, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: ViewClassWiseSubjectAllocationAcademicSubjectPolicy (class-wise allocation page)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'ViewClassWiseSubjectAllocationAcademicSubjectPolicy' AND [ClaimType] = N'View Class-wise Subject Allocation')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'View Class-wise Subject Allocation', N'ViewClassWiseSubjectAllocationAcademicSubjectPolicy', @Sub_AcademicSetup_Administration, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: CreateClassWiseSubjectAllocationAcademicSubjectPolicy (class-wise allocation page)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'CreateClassWiseSubjectAllocationAcademicSubjectPolicy' AND [ClaimType] = N'Create Class-wise Subject Allocation')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Create Class-wise Subject Allocation', N'CreateClassWiseSubjectAllocationAcademicSubjectPolicy', @Sub_AcademicSetup_Administration, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: DeleteClassWiseSubjectAllocationAcademicSubjectPolicy (class-wise allocation page)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'DeleteClassWiseSubjectAllocationAcademicSubjectPolicy' AND [ClaimType] = N'Delete Class-wise Subject Allocation')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Delete Class-wise Subject Allocation', N'DeleteClassWiseSubjectAllocationAcademicSubjectPolicy', @Sub_AcademicSetup_Administration, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: IndexAcademicSubjectTypesPolicy (subject type setup)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'IndexAcademicSubjectTypesPolicy' AND [ClaimType] = N'View Subject Type')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'View Subject Type', N'IndexAcademicSubjectTypesPolicy', @Sub_AcademicSetup_Administration, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: DetailsAcademicSubjectTypesPolicy (subject type setup)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'DetailsAcademicSubjectTypesPolicy' AND [ClaimType] = N'Details Subject Type')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Details Subject Type', N'DetailsAcademicSubjectTypesPolicy', @Sub_AcademicSetup_Administration, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: CreateAcademicSubjectTypesPolicy (subject type setup)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'CreateAcademicSubjectTypesPolicy' AND [ClaimType] = N'Create  Subject Type')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Create  Subject Type', N'CreateAcademicSubjectTypesPolicy', @Sub_AcademicSetup_Administration, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: EditAcademicSubjectTypesPolicy (subject type setup)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'EditAcademicSubjectTypesPolicy' AND [ClaimType] = N'Edit Subject Type')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Edit Subject Type', N'EditAcademicSubjectTypesPolicy', @Sub_AcademicSetup_Administration, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: DeleteAcademicSubjectTypesPolicy (subject type setup)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'DeleteAcademicSubjectTypesPolicy' AND [ClaimType] = N'Delete Subject Type')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Delete Subject Type', N'DeleteAcademicSubjectTypesPolicy', @Sub_AcademicSetup_Administration, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: ViewChaptersPolicy (no chapters section in sitemap)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'ViewChaptersPolicy' AND [ClaimType] = N'View Chapter')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'View Chapter', N'ViewChaptersPolicy', @Sub_AcademicSetup_Administration, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: CreateChaptersPolicy (no chapters section in sitemap)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'CreateChaptersPolicy' AND [ClaimType] = N'Edit Chapter')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Edit Chapter', N'CreateChaptersPolicy', @Sub_AcademicSetup_Administration, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: DetailsGradingTablePolicy (page action for the GradingTable screens)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'DetailsGradingTablePolicy' AND [ClaimType] = N'Details Grading Table')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Details Grading Table', N'DetailsGradingTablePolicy', @Sub_AcademicSetup_Administration, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: CreateGradingTablePolicy (page action for the GradingTable screens)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'CreateGradingTablePolicy' AND [ClaimType] = N'Create Grading Table')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Create Grading Table', N'CreateGradingTablePolicy', @Sub_AcademicSetup_Administration, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: EditGradingTablePolicy (page action for the GradingTable screens)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'EditGradingTablePolicy' AND [ClaimType] = N'Edit Grading Table')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Edit Grading Table', N'EditGradingTablePolicy', @Sub_AcademicSetup_Administration, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: DeleteGradingTablePolicy (page action for the GradingTable screens)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'DeleteGradingTablePolicy' AND [ClaimType] = N'Delete Grading Table')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Delete Grading Table', N'DeleteGradingTablePolicy', @Sub_AcademicSetup_Administration, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: SubjectEnrollSubjectEnrollmentPolicy (no enrollment section in sitemap)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'SubjectEnrollSubjectEnrollmentPolicy' AND [ClaimType] = N'View Enrollment List')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'View Enrollment List', N'SubjectEnrollSubjectEnrollmentPolicy', @Sub_AcademicSetup_Administration, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: SetOptionalSubjectSubjectEnrollmentPolicy (no enrollment section in sitemap)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'SetOptionalSubjectSubjectEnrollmentPolicy' AND [ClaimType] = N'Update Optional Subject')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Update Optional Subject', N'SetOptionalSubjectSubjectEnrollmentPolicy', @Sub_AcademicSetup_Administration, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Submodule: SecuritySetup (Security)
IF NOT EXISTS (SELECT 1 FROM [ProjectSubModules] WHERE [SubModuleName] = N'SecuritySetup' AND [ProjectModuleId] = @Module_Administration)
    INSERT INTO [ProjectSubModules] ([SubModuleName], [ProjectModuleId], [Status], [Remarks], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'SecuritySetup', @Module_Administration, 1, N'Security', N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
DECLARE @Sub_SecuritySetup_Administration INT = (SELECT [Id] FROM [ProjectSubModules] WHERE [SubModuleName] = N'SecuritySetup' AND [ProjectModuleId] = @Module_Administration);
-- Permission: UserListAccountsPolicy (from SiteMap.Config menu item)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'UserListAccountsPolicy' AND [ClaimType] = N'User List')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'User List', N'UserListAccountsPolicy', @Sub_SecuritySetup_Administration, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: ViewUserProfileAdministrationsPolicy (from SiteMap.Config menu item)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'ViewUserProfileAdministrationsPolicy' AND [ClaimType] = N'View User Profile')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'View User Profile', N'ViewUserProfileAdministrationsPolicy', @Sub_SecuritySetup_Administration, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: ViewRolesAccountsPolicy (from SiteMap.Config menu item)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'ViewRolesAccountsPolicy' AND [ClaimType] = N'View User Role')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'View User Role', N'ViewRolesAccountsPolicy', @Sub_SecuritySetup_Administration, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: RegisterAccountsPolicy (page action for the Accounts screens)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'RegisterAccountsPolicy' AND [ClaimType] = N'Register New User')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Register New User', N'RegisterAccountsPolicy', @Sub_SecuritySetup_Administration, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: EditUserAccountsPolicy (user screens live under Security)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'EditUserAccountsPolicy' AND [ClaimType] = N'Edit User Accounts')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Edit User Accounts', N'EditUserAccountsPolicy', @Sub_SecuritySetup_Administration, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: DeleteUserAccountsPolicy (user screens live under Security)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'DeleteUserAccountsPolicy' AND [ClaimType] = N'Delete User Account')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Delete User Account', N'DeleteUserAccountsPolicy', @Sub_SecuritySetup_Administration, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: CreateRoleAccountsPolicy (role screens live under Security)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'CreateRoleAccountsPolicy' AND [ClaimType] = N'Create User Role')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Create User Role', N'CreateRoleAccountsPolicy', @Sub_SecuritySetup_Administration, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: EditRoleAccountsPolicy (role screens live under Security)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'EditRoleAccountsPolicy' AND [ClaimType] = N'Edit User Role')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Edit User Role', N'EditRoleAccountsPolicy', @Sub_SecuritySetup_Administration, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: AddOrRemoveUserAccountsPolicy (user screens live under Security)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'AddOrRemoveUserAccountsPolicy' AND [ClaimType] = N'Add or Remove User From Role')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Add or Remove User From Role', N'AddOrRemoveUserAccountsPolicy', @Sub_SecuritySetup_Administration, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: EditUserProfileAdministrationsPolicy (page action for the Administrations screens)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'EditUserProfileAdministrationsPolicy' AND [ClaimType] = N'Edit User Profile')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Edit User Profile', N'EditUserProfileAdministrationsPolicy', @Sub_SecuritySetup_Administration, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: IndexRolesPolicy (no Roles section in sitemap; grouped with Security)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'IndexRolesPolicy' AND [ClaimType] = N'View Roles')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'View Roles', N'IndexRolesPolicy', @Sub_SecuritySetup_Administration, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: CreateRolesPolicy (no Roles section in sitemap; grouped with Security)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'CreateRolesPolicy' AND [ClaimType] = N'Create Roles')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Create Roles', N'CreateRolesPolicy', @Sub_SecuritySetup_Administration, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: AssignRolesPolicy (no Roles section in sitemap; grouped with Security)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'AssignRolesPolicy' AND [ClaimType] = N'Assign Roles')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Assign Roles', N'AssignRolesPolicy', @Sub_SecuritySetup_Administration, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Submodule: Integrations (Integration)
IF NOT EXISTS (SELECT 1 FROM [ProjectSubModules] WHERE [SubModuleName] = N'Integrations' AND [ProjectModuleId] = @Module_Administration)
    INSERT INTO [ProjectSubModules] ([SubModuleName], [ProjectModuleId], [Status], [Remarks], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Integrations', @Module_Administration, 1, N'Integration', N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
DECLARE @Sub_Integrations_Administration INT = (SELECT [Id] FROM [ProjectSubModules] WHERE [SubModuleName] = N'Integrations' AND [ProjectModuleId] = @Module_Administration);
-- Submodule: UserRoles (Users Roles)
IF NOT EXISTS (SELECT 1 FROM [ProjectSubModules] WHERE [SubModuleName] = N'UserRoles' AND [ProjectModuleId] = @Module_Administration)
    INSERT INTO [ProjectSubModules] ([SubModuleName], [ProjectModuleId], [Status], [Remarks], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'UserRoles', @Module_Administration, 1, N'Users Roles', N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
DECLARE @Sub_UserRoles_Administration INT = (SELECT [Id] FROM [ProjectSubModules] WHERE [SubModuleName] = N'UserRoles' AND [ProjectModuleId] = @Module_Administration);
-- Submodule: AdminReports (Reports)
IF NOT EXISTS (SELECT 1 FROM [ProjectSubModules] WHERE [SubModuleName] = N'AdminReports' AND [ProjectModuleId] = @Module_Administration)
    INSERT INTO [ProjectSubModules] ([SubModuleName], [ProjectModuleId], [Status], [Remarks], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'AdminReports', @Module_Administration, 1, N'Reports', N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
DECLARE @Sub_AdminReports_Administration INT = (SELECT [Id] FROM [ProjectSubModules] WHERE [SubModuleName] = N'AdminReports' AND [ProjectModuleId] = @Module_Administration);

-- Module: StudentManagement (Students)
IF NOT EXISTS (SELECT 1 FROM [ProjectModules] WHERE [ModuleName] = N'StudentManagement')
BEGIN
    INSERT INTO [ProjectModules] ([ModuleName], [Status], [Remarks], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'StudentManagement', 1, N'Students', N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
END
DECLARE @Module_StudentManagement INT = (SELECT [Id] FROM [ProjectModules] WHERE [ModuleName] = N'StudentManagement');
-- Submodule: StudentRecords (Records)
IF NOT EXISTS (SELECT 1 FROM [ProjectSubModules] WHERE [SubModuleName] = N'StudentRecords' AND [ProjectModuleId] = @Module_StudentManagement)
    INSERT INTO [ProjectSubModules] ([SubModuleName], [ProjectModuleId], [Status], [Remarks], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'StudentRecords', @Module_StudentManagement, 1, N'Records', N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
DECLARE @Sub_StudentRecords_StudentManagement INT = (SELECT [Id] FROM [ProjectSubModules] WHERE [SubModuleName] = N'StudentRecords' AND [ProjectModuleId] = @Module_StudentManagement);
-- Permission: IndexStudentsPolicy (from SiteMap.Config menu item)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'IndexStudentsPolicy' AND [ClaimType] = N'View Student List')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'View Student List', N'IndexStudentsPolicy', @Sub_StudentRecords_StudentManagement, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: CreateStudentsPolicy (from SiteMap.Config menu item)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'CreateStudentsPolicy' AND [ClaimType] = N'Create Student')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Create Student', N'CreateStudentsPolicy', @Sub_StudentRecords_StudentManagement, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: DetailsStudentsPolicy (page action for the Students screens)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'DetailsStudentsPolicy' AND [ClaimType] = N'Details Student')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Details Student', N'DetailsStudentsPolicy', @Sub_StudentRecords_StudentManagement, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: BulkUploadStudentsPolicy (page action for the Students screens)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'BulkUploadStudentsPolicy' AND [ClaimType] = N'Bulk Upload Students')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Bulk Upload Students', N'BulkUploadStudentsPolicy', @Sub_StudentRecords_StudentManagement, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: EditStudentsPolicy (page action for the Students screens)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'EditStudentsPolicy' AND [ClaimType] = N'Edit Student')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Edit Student', N'EditStudentsPolicy', @Sub_StudentRecords_StudentManagement, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: DeleteStudentsPolicy (page action for the Students screens)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'DeleteStudentsPolicy' AND [ClaimType] = N'Delete Student')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Delete Student', N'DeleteStudentsPolicy', @Sub_StudentRecords_StudentManagement, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: ProfileStudentsPolicy (page action for the Students screens)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'ProfileStudentsPolicy' AND [ClaimType] = N'View Student Profile')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'View Student Profile', N'ProfileStudentsPolicy', @Sub_StudentRecords_StudentManagement, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Submodule: StudentReports (Reports)
IF NOT EXISTS (SELECT 1 FROM [ProjectSubModules] WHERE [SubModuleName] = N'StudentReports' AND [ProjectModuleId] = @Module_StudentManagement)
    INSERT INTO [ProjectSubModules] ([SubModuleName], [ProjectModuleId], [Status], [Remarks], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'StudentReports', @Module_StudentManagement, 1, N'Reports', N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
DECLARE @Sub_StudentReports_StudentManagement INT = (SELECT [Id] FROM [ProjectSubModules] WHERE [SubModuleName] = N'StudentReports' AND [ProjectModuleId] = @Module_StudentManagement);
-- Permission: StudentsReportsPolicy (student list report)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'StudentsReportsPolicy' AND [ClaimType] = N'View Student List Report')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'View Student List Report', N'StudentsReportsPolicy', @Sub_StudentReports_StudentManagement, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');

-- Module: HR (HR)
IF NOT EXISTS (SELECT 1 FROM [ProjectModules] WHERE [ModuleName] = N'HR')
BEGIN
    INSERT INTO [ProjectModules] ([ModuleName], [Status], [Remarks], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'HR', 1, N'HR', N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
END
DECLARE @Module_HR INT = (SELECT [Id] FROM [ProjectModules] WHERE [ModuleName] = N'HR');
-- Submodule: EmployeeManagement (Employees)
IF NOT EXISTS (SELECT 1 FROM [ProjectSubModules] WHERE [SubModuleName] = N'EmployeeManagement' AND [ProjectModuleId] = @Module_HR)
    INSERT INTO [ProjectSubModules] ([SubModuleName], [ProjectModuleId], [Status], [Remarks], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'EmployeeManagement', @Module_HR, 1, N'Employees', N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
DECLARE @Sub_EmployeeManagement_HR INT = (SELECT [Id] FROM [ProjectSubModules] WHERE [SubModuleName] = N'EmployeeManagement' AND [ProjectModuleId] = @Module_HR);
-- Permission: IndexEmployeesPolicy (from SiteMap.Config menu item)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'IndexEmployeesPolicy' AND [ClaimType] = N'View Employee')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'View Employee', N'IndexEmployeesPolicy', @Sub_EmployeeManagement_HR, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: DetailsEmployeesPolicy (page action for the Employees screens)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'DetailsEmployeesPolicy' AND [ClaimType] = N'Details Employee')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Details Employee', N'DetailsEmployeesPolicy', @Sub_EmployeeManagement_HR, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: CreateEmployeesPolicy (page action for the Employees screens)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'CreateEmployeesPolicy' AND [ClaimType] = N'Create Employee')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Create Employee', N'CreateEmployeesPolicy', @Sub_EmployeeManagement_HR, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: EditEmployeesPolicy (page action for the Employees screens)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'EditEmployeesPolicy' AND [ClaimType] = N'Edit Employee')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Edit Employee', N'EditEmployeesPolicy', @Sub_EmployeeManagement_HR, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: DeleteEmployeesPolicy (page action for the Employees screens)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'DeleteEmployeesPolicy' AND [ClaimType] = N'Delete Employee')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Delete Employee', N'DeleteEmployeesPolicy', @Sub_EmployeeManagement_HR, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Submodule: Setup (HR Setup)
IF NOT EXISTS (SELECT 1 FROM [ProjectSubModules] WHERE [SubModuleName] = N'Setup' AND [ProjectModuleId] = @Module_HR)
    INSERT INTO [ProjectSubModules] ([SubModuleName], [ProjectModuleId], [Status], [Remarks], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Setup', @Module_HR, 1, N'HR Setup', N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
DECLARE @Sub_Setup_HR INT = (SELECT [Id] FROM [ProjectSubModules] WHERE [SubModuleName] = N'Setup' AND [ProjectModuleId] = @Module_HR);
-- Permission: IndexDesignationsPolicy (from SiteMap.Config menu item)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'IndexDesignationsPolicy' AND [ClaimType] = N'View Designations')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'View Designations', N'IndexDesignationsPolicy', @Sub_Setup_HR, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: DetailsDesignationsPolicy (page action for the Designations screens)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'DetailsDesignationsPolicy' AND [ClaimType] = N'View Details Designations')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'View Details Designations', N'DetailsDesignationsPolicy', @Sub_Setup_HR, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: CreateDesignationsPolicy (page action for the Designations screens)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'CreateDesignationsPolicy' AND [ClaimType] = N'Create Designations')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Create Designations', N'CreateDesignationsPolicy', @Sub_Setup_HR, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: EditDesignationsPolicy (page action for the Designations screens)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'EditDesignationsPolicy' AND [ClaimType] = N'Edit Designations')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Edit Designations', N'EditDesignationsPolicy', @Sub_Setup_HR, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: DeleteDesignationsPolicy (page action for the Designations screens)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'DeleteDesignationsPolicy' AND [ClaimType] = N'Delete Designations')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Delete Designations', N'DeleteDesignationsPolicy', @Sub_Setup_HR, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: IndexDesignationTypesPolicy (HR lookup table)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'IndexDesignationTypesPolicy' AND [ClaimType] = N'View Designation Types')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'View Designation Types', N'IndexDesignationTypesPolicy', @Sub_Setup_HR, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: DetailsDesignationTypesPolicy (HR lookup table)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'DetailsDesignationTypesPolicy' AND [ClaimType] = N'Details Designation Types')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Details Designation Types', N'DetailsDesignationTypesPolicy', @Sub_Setup_HR, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: CreateDesignationTypesPolicy (HR lookup table)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'CreateDesignationTypesPolicy' AND [ClaimType] = N'Create Designation Types')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Create Designation Types', N'CreateDesignationTypesPolicy', @Sub_Setup_HR, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: EditDesignationTypesPolicy (HR lookup table)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'EditDesignationTypesPolicy' AND [ClaimType] = N'Edit Designation Types')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Edit Designation Types', N'EditDesignationTypesPolicy', @Sub_Setup_HR, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: DeleteDesignationTypesPolicy (HR lookup table)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'DeleteDesignationTypesPolicy' AND [ClaimType] = N'Delete Designation Types')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Delete Designation Types', N'DeleteDesignationTypesPolicy', @Sub_Setup_HR, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: IndexEmpTypesPolicy (HR lookup table)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'IndexEmpTypesPolicy' AND [ClaimType] = N'View Employee Types')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'View Employee Types', N'IndexEmpTypesPolicy', @Sub_Setup_HR, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: DetailsEmpTypesPolicy (HR lookup table)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'DetailsEmpTypesPolicy' AND [ClaimType] = N'Details Employee Types')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Details Employee Types', N'DetailsEmpTypesPolicy', @Sub_Setup_HR, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: CreateEmpTypesPolicy (HR lookup table)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'CreateEmpTypesPolicy' AND [ClaimType] = N'Create Employee Types')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Create Employee Types', N'CreateEmpTypesPolicy', @Sub_Setup_HR, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: EditEmpTypesPolicy (HR lookup table)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'EditEmpTypesPolicy' AND [ClaimType] = N'Edit Employee Types')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Edit Employee Types', N'EditEmpTypesPolicy', @Sub_Setup_HR, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: DeleteEmpTypesPolicy (HR lookup table)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'DeleteEmpTypesPolicy' AND [ClaimType] = N'Delete Employee Types')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Delete Employee Types', N'DeleteEmpTypesPolicy', @Sub_Setup_HR, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Submodule: HRReports (Reports)
IF NOT EXISTS (SELECT 1 FROM [ProjectSubModules] WHERE [SubModuleName] = N'HRReports' AND [ProjectModuleId] = @Module_HR)
    INSERT INTO [ProjectSubModules] ([SubModuleName], [ProjectModuleId], [Status], [Remarks], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'HRReports', @Module_HR, 1, N'Reports', N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
DECLARE @Sub_HRReports_HR INT = (SELECT [Id] FROM [ProjectSubModules] WHERE [SubModuleName] = N'HRReports' AND [ProjectModuleId] = @Module_HR);

-- Module: Academics (Academics)
IF NOT EXISTS (SELECT 1 FROM [ProjectModules] WHERE [ModuleName] = N'Academics')
BEGIN
    INSERT INTO [ProjectModules] ([ModuleName], [Status], [Remarks], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Academics', 1, N'Academics', N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
END
DECLARE @Module_Academics INT = (SELECT [Id] FROM [ProjectModules] WHERE [ModuleName] = N'Academics');
-- Submodule: Classroom (Classes)
IF NOT EXISTS (SELECT 1 FROM [ProjectSubModules] WHERE [SubModuleName] = N'Classroom' AND [ProjectModuleId] = @Module_Academics)
    INSERT INTO [ProjectSubModules] ([SubModuleName], [ProjectModuleId], [Status], [Remarks], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Classroom', @Module_Academics, 1, N'Classes', N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
DECLARE @Sub_Classroom_Academics INT = (SELECT [Id] FROM [ProjectSubModules] WHERE [SubModuleName] = N'Classroom' AND [ProjectModuleId] = @Module_Academics);
-- Submodule: AcademicReports (Reports)
IF NOT EXISTS (SELECT 1 FROM [ProjectSubModules] WHERE [SubModuleName] = N'AcademicReports' AND [ProjectModuleId] = @Module_Academics)
    INSERT INTO [ProjectSubModules] ([SubModuleName], [ProjectModuleId], [Status], [Remarks], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'AcademicReports', @Module_Academics, 1, N'Reports', N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
DECLARE @Sub_AcademicReports_Academics INT = (SELECT [Id] FROM [ProjectSubModules] WHERE [SubModuleName] = N'AcademicReports' AND [ProjectModuleId] = @Module_Academics);

-- Module: Attendance (Attendance)
IF NOT EXISTS (SELECT 1 FROM [ProjectModules] WHERE [ModuleName] = N'Attendance')
BEGIN
    INSERT INTO [ProjectModules] ([ModuleName], [Status], [Remarks], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Attendance', 1, N'Attendance', N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
END
DECLARE @Module_Attendance INT = (SELECT [Id] FROM [ProjectModules] WHERE [ModuleName] = N'Attendance');
-- Submodule: AttendanceManagement (Manage)
IF NOT EXISTS (SELECT 1 FROM [ProjectSubModules] WHERE [SubModuleName] = N'AttendanceManagement' AND [ProjectModuleId] = @Module_Attendance)
    INSERT INTO [ProjectSubModules] ([SubModuleName], [ProjectModuleId], [Status], [Remarks], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'AttendanceManagement', @Module_Attendance, 1, N'Manage', N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
DECLARE @Sub_AttendanceManagement_Attendance INT = (SELECT [Id] FROM [ProjectSubModules] WHERE [SubModuleName] = N'AttendanceManagement' AND [ProjectModuleId] = @Module_Attendance);
-- Permission: CreateAttendanceMachinesPolicy (from SiteMap.Config menu item)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'CreateAttendanceMachinesPolicy' AND [ClaimType] = N'Create Attendance')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Create Attendance', N'CreateAttendanceMachinesPolicy', @Sub_AttendanceManagement_Attendance, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: DetailsAttendanceMachinesPolicy (page action for the AttendanceMachines screens)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'DetailsAttendanceMachinesPolicy' AND [ClaimType] = N'View Details Attendance')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'View Details Attendance', N'DetailsAttendanceMachinesPolicy', @Sub_AttendanceManagement_Attendance, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: EditAttendanceMachinesPolicy (page action for the AttendanceMachines screens)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'EditAttendanceMachinesPolicy' AND [ClaimType] = N'Edit Attendance')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Edit Attendance', N'EditAttendanceMachinesPolicy', @Sub_AttendanceManagement_Attendance, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: DeleteAttendanceMachinesPolicy (page action for the AttendanceMachines screens)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'DeleteAttendanceMachinesPolicy' AND [ClaimType] = N'Delete Attendance')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Delete Attendance', N'DeleteAttendanceMachinesPolicy', @Sub_AttendanceManagement_Attendance, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Submodule: AttendanceReports (Reports)
IF NOT EXISTS (SELECT 1 FROM [ProjectSubModules] WHERE [SubModuleName] = N'AttendanceReports' AND [ProjectModuleId] = @Module_Attendance)
    INSERT INTO [ProjectSubModules] ([SubModuleName], [ProjectModuleId], [Status], [Remarks], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'AttendanceReports', @Module_Attendance, 1, N'Reports', N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
DECLARE @Sub_AttendanceReports_Attendance INT = (SELECT [Id] FROM [ProjectSubModules] WHERE [SubModuleName] = N'AttendanceReports' AND [ProjectModuleId] = @Module_Attendance);
-- Permission: IndexAttendanceMachinesPolicy (from SiteMap.Config menu item)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'IndexAttendanceMachinesPolicy' AND [ClaimType] = N'View Attendances')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'View Attendances', N'IndexAttendanceMachinesPolicy', @Sub_AttendanceReports_Attendance, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: DailyAttendanceReportsPolicy (from SiteMap.Config menu item)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'DailyAttendanceReportsPolicy' AND [ClaimType] = N'View Daily Attendance Report')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'View Daily Attendance Report', N'DailyAttendanceReportsPolicy', @Sub_AttendanceReports_Attendance, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: AttendanceReportsPolicy (from SiteMap.Config menu item)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'AttendanceReportsPolicy' AND [ClaimType] = N'View Attendance Report')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'View Attendance Report', N'AttendanceReportsPolicy', @Sub_AttendanceReports_Attendance, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Submodule: AttendanceMachinesSetup (Fingerprint Machines)
IF NOT EXISTS (SELECT 1 FROM [ProjectSubModules] WHERE [SubModuleName] = N'AttendanceMachinesSetup' AND [ProjectModuleId] = @Module_Attendance)
    INSERT INTO [ProjectSubModules] ([SubModuleName], [ProjectModuleId], [Status], [Remarks], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'AttendanceMachinesSetup', @Module_Attendance, 1, N'Fingerprint Machines', N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
DECLARE @Sub_AttendanceMachinesSetup_Attendance INT = (SELECT [Id] FROM [ProjectSubModules] WHERE [SubModuleName] = N'AttendanceMachinesSetup' AND [ProjectModuleId] = @Module_Attendance);
-- Permission: IndexAttendanceMachineDevicesPolicy (from SiteMap.Config menu item)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'IndexAttendanceMachineDevicesPolicy' AND [ClaimType] = N'View Attendance Machines')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'View Attendance Machines', N'IndexAttendanceMachineDevicesPolicy', @Sub_AttendanceMachinesSetup_Attendance, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: ManageMachineUsersPolicy (from SiteMap.Config menu item)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'ManageMachineUsersPolicy' AND [ClaimType] = N'Manage Machine Users')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Manage Machine Users', N'ManageMachineUsersPolicy', @Sub_AttendanceMachinesSetup_Attendance, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: DetailsAttendanceMachineDevicesPolicy (page action for the AttendanceMachineDevices screens)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'DetailsAttendanceMachineDevicesPolicy' AND [ClaimType] = N'View Details Attendance Machines')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'View Details Attendance Machines', N'DetailsAttendanceMachineDevicesPolicy', @Sub_AttendanceMachinesSetup_Attendance, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: CreateAttendanceMachineDevicesPolicy (page action for the AttendanceMachineDevices screens)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'CreateAttendanceMachineDevicesPolicy' AND [ClaimType] = N'Create Attendance Machines')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Create Attendance Machines', N'CreateAttendanceMachineDevicesPolicy', @Sub_AttendanceMachinesSetup_Attendance, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: EditAttendanceMachineDevicesPolicy (page action for the AttendanceMachineDevices screens)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'EditAttendanceMachineDevicesPolicy' AND [ClaimType] = N'Edit Attendance Machines')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Edit Attendance Machines', N'EditAttendanceMachineDevicesPolicy', @Sub_AttendanceMachinesSetup_Attendance, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: DeleteAttendanceMachineDevicesPolicy (page action for the AttendanceMachineDevices screens)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'DeleteAttendanceMachineDevicesPolicy' AND [ClaimType] = N'Delete Attendance Machines')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Delete Attendance Machines', N'DeleteAttendanceMachineDevicesPolicy', @Sub_AttendanceMachinesSetup_Attendance, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');

-- Module: Exam_Result (Exams)
IF NOT EXISTS (SELECT 1 FROM [ProjectModules] WHERE [ModuleName] = N'Exam_Result')
BEGIN
    INSERT INTO [ProjectModules] ([ModuleName], [Status], [Remarks], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Exam_Result', 1, N'Exams', N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
END
DECLARE @Module_Exam_Result INT = (SELECT [Id] FROM [ProjectModules] WHERE [ModuleName] = N'Exam_Result');
-- Submodule: ExamSetup (Setup)
IF NOT EXISTS (SELECT 1 FROM [ProjectSubModules] WHERE [SubModuleName] = N'ExamSetup' AND [ProjectModuleId] = @Module_Exam_Result)
    INSERT INTO [ProjectSubModules] ([SubModuleName], [ProjectModuleId], [Status], [Remarks], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'ExamSetup', @Module_Exam_Result, 1, N'Setup', N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
DECLARE @Sub_ExamSetup_Exam_Result INT = (SELECT [Id] FROM [ProjectSubModules] WHERE [SubModuleName] = N'ExamSetup' AND [ProjectModuleId] = @Module_Exam_Result);
-- Permission: IndexAcademicExamGroupPolicy (from SiteMap.Config menu item)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'IndexAcademicExamGroupPolicy' AND [ClaimType] = N'View Academic Exam Group')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'View Academic Exam Group', N'IndexAcademicExamGroupPolicy', @Sub_ExamSetup_Exam_Result, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: IndexAcademicExamTypePolicy (from SiteMap.Config menu item)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'IndexAcademicExamTypePolicy' AND [ClaimType] = N'View Academic Exam Type')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'View Academic Exam Type', N'IndexAcademicExamTypePolicy', @Sub_ExamSetup_Exam_Result, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: IndexAcademicExamPolicy (from SiteMap.Config menu item)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'IndexAcademicExamPolicy' AND [ClaimType] = N'View Academic Exam')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'View Academic Exam', N'IndexAcademicExamPolicy', @Sub_ExamSetup_Exam_Result, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: DetailsAcademicExamGroupPolicy (page action for the AcademicExamGroup screens)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'DetailsAcademicExamGroupPolicy' AND [ClaimType] = N'Details Academic Exam Group')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Details Academic Exam Group', N'DetailsAcademicExamGroupPolicy', @Sub_ExamSetup_Exam_Result, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: CreateAcademicExamGroupPolicy (page action for the AcademicExamGroup screens)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'CreateAcademicExamGroupPolicy' AND [ClaimType] = N'Create Academic Exam Group')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Create Academic Exam Group', N'CreateAcademicExamGroupPolicy', @Sub_ExamSetup_Exam_Result, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: EditAcademicExamGroupPolicy (page action for the AcademicExamGroup screens)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'EditAcademicExamGroupPolicy' AND [ClaimType] = N'Edit Academic Exam Group')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Edit Academic Exam Group', N'EditAcademicExamGroupPolicy', @Sub_ExamSetup_Exam_Result, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: DeleteAcademicExamGroupPolicy (page action for the AcademicExamGroup screens)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'DeleteAcademicExamGroupPolicy' AND [ClaimType] = N'Delete Academic Exam Group')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Delete Academic Exam Group', N'DeleteAcademicExamGroupPolicy', @Sub_ExamSetup_Exam_Result, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: DetailsAcademicExamPolicy (page action for the AcademicExam screens)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'DetailsAcademicExamPolicy' AND [ClaimType] = N'Details Academic Exam')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Details Academic Exam', N'DetailsAcademicExamPolicy', @Sub_ExamSetup_Exam_Result, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: CreateAcademicExamPolicy (page action for the AcademicExam screens)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'CreateAcademicExamPolicy' AND [ClaimType] = N'Create Academic Exam')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Create Academic Exam', N'CreateAcademicExamPolicy', @Sub_ExamSetup_Exam_Result, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: EditAcademicExamPolicy (page action for the AcademicExam screens)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'EditAcademicExamPolicy' AND [ClaimType] = N'Edit Academic Exam')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Edit Academic Exam', N'EditAcademicExamPolicy', @Sub_ExamSetup_Exam_Result, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: DeleteAcademicExamPolicy (page action for the AcademicExam screens)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'DeleteAcademicExamPolicy' AND [ClaimType] = N'Delete Academic Exam Group')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Delete Academic Exam Group', N'DeleteAcademicExamPolicy', @Sub_ExamSetup_Exam_Result, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: ExamMarkSubmitAcademicExamPolicy (page action for the AcademicExam screens)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'ExamMarkSubmitAcademicExamPolicy' AND [ClaimType] = N'Submit Academic Exam Marks')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Submit Academic Exam Marks', N'ExamMarkSubmitAcademicExamPolicy', @Sub_ExamSetup_Exam_Result, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: AdmitCardAcademicExamPolicy (page action for the AcademicExam screens)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'AdmitCardAcademicExamPolicy' AND [ClaimType] = N'Report Admit Card')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Report Admit Card', N'AdmitCardAcademicExamPolicy', @Sub_ExamSetup_Exam_Result, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: LockAcademicExamPolicy (page action for the AcademicExam screens)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'LockAcademicExamPolicy' AND [ClaimType] = N'Lock Academic Exam')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Lock Academic Exam', N'LockAcademicExamPolicy', @Sub_ExamSetup_Exam_Result, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: DetailsAcademicExamTypePolicy (page action for the AcademicExamType screens)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'DetailsAcademicExamTypePolicy' AND [ClaimType] = N'Details Academic Exam Type')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Details Academic Exam Type', N'DetailsAcademicExamTypePolicy', @Sub_ExamSetup_Exam_Result, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: CraeteAcademicExamTypePolicy (page action for the AcademicExamType screens)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'CraeteAcademicExamTypePolicy' AND [ClaimType] = N'Create Academic Exam Type')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Create Academic Exam Type', N'CraeteAcademicExamTypePolicy', @Sub_ExamSetup_Exam_Result, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: EditAcademicExamTypePolicy (page action for the AcademicExamType screens)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'EditAcademicExamTypePolicy' AND [ClaimType] = N'Edit Academic Exam Type')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Edit Academic Exam Type', N'EditAcademicExamTypePolicy', @Sub_ExamSetup_Exam_Result, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: DeleteAcademicExamTypePolicy (page action for the AcademicExamType screens)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'DeleteAcademicExamTypePolicy' AND [ClaimType] = N'Delete Academic Exam Type')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Delete Academic Exam Type', N'DeleteAcademicExamTypePolicy', @Sub_ExamSetup_Exam_Result, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: IndexQuestionBanksPolicy (no question-bank section in sitemap)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'IndexQuestionBanksPolicy' AND [ClaimType] = N'View Question Banks')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'View Question Banks', N'IndexQuestionBanksPolicy', @Sub_ExamSetup_Exam_Result, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: AllQuestionQuestionBanksPolicy (no question-bank section in sitemap)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'AllQuestionQuestionBanksPolicy' AND [ClaimType] = N'View All Question Bank')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'View All Question Bank', N'AllQuestionQuestionBanksPolicy', @Sub_ExamSetup_Exam_Result, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: CreateQuestionBanksPolicy (no question-bank section in sitemap)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'CreateQuestionBanksPolicy' AND [ClaimType] = N'Create Question Bank')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Create Question Bank', N'CreateQuestionBanksPolicy', @Sub_ExamSetup_Exam_Result, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: EditQuestionBanksPolicy (no question-bank section in sitemap)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'EditQuestionBanksPolicy' AND [ClaimType] = N'Edit Question Bank')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Edit Question Bank', N'EditQuestionBanksPolicy', @Sub_ExamSetup_Exam_Result, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: IndexQuestionFormationPolicy (no question formation section in sitemap)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'IndexQuestionFormationPolicy' AND [ClaimType] = N'View Question Formation')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'View Question Formation', N'IndexQuestionFormationPolicy', @Sub_ExamSetup_Exam_Result, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: CreateQuestionFormationPolicy (no question formation section in sitemap)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'CreateQuestionFormationPolicy' AND [ClaimType] = N'Create Question Formation')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Create Question Formation', N'CreateQuestionFormationPolicy', @Sub_ExamSetup_Exam_Result, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: EditQuestionFormationPolicy (no question formation section in sitemap)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'EditQuestionFormationPolicy' AND [ClaimType] = N'Edit Question Formation')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Edit Question Formation', N'EditQuestionFormationPolicy', @Sub_ExamSetup_Exam_Result, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Submodule: ExamResults (Results)
IF NOT EXISTS (SELECT 1 FROM [ProjectSubModules] WHERE [SubModuleName] = N'ExamResults' AND [ProjectModuleId] = @Module_Exam_Result)
    INSERT INTO [ProjectSubModules] ([SubModuleName], [ProjectModuleId], [Status], [Remarks], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'ExamResults', @Module_Exam_Result, 1, N'Results', N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
DECLARE @Sub_ExamResults_Exam_Result INT = (SELECT [Id] FROM [ProjectSubModules] WHERE [SubModuleName] = N'ExamResults' AND [ProjectModuleId] = @Module_Exam_Result);
-- Permission: LiveResultExamResultsPolicy (from SiteMap.Config menu item)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'LiveResultExamResultsPolicy' AND [ClaimType] = N'Live Results')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Live Results', N'LiveResultExamResultsPolicy', @Sub_ExamResults_Exam_Result, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: ClassWiseResultExamResultsPolicy (from SiteMap.Config menu item)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'ClassWiseResultExamResultsPolicy' AND [ClaimType] = N'View Class-Wise Current Result')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'View Class-Wise Current Result', N'ClassWiseResultExamResultsPolicy', @Sub_ExamResults_Exam_Result, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: IndexExamResultsPolicy (from SiteMap.Config menu item)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'IndexExamResultsPolicy' AND [ClaimType] = N'View Student-Wise Exam Result')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'View Student-Wise Exam Result', N'IndexExamResultsPolicy', @Sub_ExamResults_Exam_Result, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: SubjectWiseResultExamResultsPolicy (page action for the ExamResults screens)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'SubjectWiseResultExamResultsPolicy' AND [ClaimType] = N'View Subject-Wise Current Result')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'View Subject-Wise Current Result', N'SubjectWiseResultExamResultsPolicy', @Sub_ExamResults_Exam_Result, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: StudentWiseResultAfterProcessExamResultsPolicy (page action for the ExamResults screens)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'StudentWiseResultAfterProcessExamResultsPolicy' AND [ClaimType] = N'View Student-Wise Processed Result')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'View Student-Wise Processed Result', N'StudentWiseResultAfterProcessExamResultsPolicy', @Sub_ExamResults_Exam_Result, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: DetailsExamResultsPolicy (page action for the ExamResults screens)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'DetailsExamResultsPolicy' AND [ClaimType] = N'Delete Employee Types')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Delete Employee Types', N'DetailsExamResultsPolicy', @Sub_ExamResults_Exam_Result, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: CreateExamResultsPolicy (page action for the ExamResults screens)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'CreateExamResultsPolicy' AND [ClaimType] = N'Create Exam Result')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Create Exam Result', N'CreateExamResultsPolicy', @Sub_ExamResults_Exam_Result, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: EditExamResultsPolicy (page action for the ExamResults screens)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'EditExamResultsPolicy' AND [ClaimType] = N'Edit Exam Result')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Edit Exam Result', N'EditExamResultsPolicy', @Sub_ExamResults_Exam_Result, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: ProcessResultExamResultsPolicy (page action for the ExamResults screens)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'ProcessResultExamResultsPolicy' AND [ClaimType] = N'Process Result')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Process Result', N'ProcessResultExamResultsPolicy', @Sub_ExamResults_Exam_Result, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: UpdateRankingExamResultsPolicy (page action for the ExamResults screens)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'UpdateRankingExamResultsPolicy' AND [ClaimType] = N'Update Ranking')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Update Ranking', N'UpdateRankingExamResultsPolicy', @Sub_ExamResults_Exam_Result, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: DeleteExamResultsPolicy (page action for the ExamResults screens)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'DeleteExamResultsPolicy' AND [ClaimType] = N'Delete Exam')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Delete Exam', N'DeleteExamResultsPolicy', @Sub_ExamResults_Exam_Result, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: DeleteResultExamResultsPolicy (page action for the ExamResults screens)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'DeleteResultExamResultsPolicy' AND [ClaimType] = N'Delete Results')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Delete Results', N'DeleteResultExamResultsPolicy', @Sub_ExamResults_Exam_Result, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Submodule: ExamReports (Reports)
IF NOT EXISTS (SELECT 1 FROM [ProjectSubModules] WHERE [SubModuleName] = N'ExamReports' AND [ProjectModuleId] = @Module_Exam_Result)
    INSERT INTO [ProjectSubModules] ([SubModuleName], [ProjectModuleId], [Status], [Remarks], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'ExamReports', @Module_Exam_Result, 1, N'Reports', N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
DECLARE @Sub_ExamReports_Exam_Result INT = (SELECT [Id] FROM [ProjectSubModules] WHERE [SubModuleName] = N'ExamReports' AND [ProjectModuleId] = @Module_Exam_Result);
-- Permission: ClassWiseResultAfterProcessExamResultsPolicy (from SiteMap.Config menu item)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'ClassWiseResultAfterProcessExamResultsPolicy' AND [ClaimType] = N'View Class-wise Processed Result')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'View Class-wise Processed Result', N'ClassWiseResultAfterProcessExamResultsPolicy', @Sub_ExamReports_Exam_Result, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: SubjectWiseResultAfterProcessExamResultsPolicy (from SiteMap.Config menu item)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'SubjectWiseResultAfterProcessExamResultsPolicy' AND [ClaimType] = N'View Subject-wise Processed Result')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'View Subject-wise Processed Result', N'SubjectWiseResultAfterProcessExamResultsPolicy', @Sub_ExamReports_Exam_Result, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: StudentWiseMarkSheetReportsPolicy (from SiteMap.Config menu item)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'StudentWiseMarkSheetReportsPolicy' AND [ClaimType] = N'View Studetn-Wise Marksheet Report')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'View Studetn-Wise Marksheet Report', N'StudentWiseMarkSheetReportsPolicy', @Sub_ExamReports_Exam_Result, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: SubjectWiseMarkSheetReportsPolicy (subject-wise marksheet report)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'SubjectWiseMarkSheetReportsPolicy' AND [ClaimType] = N'View Subject-Wise Marksheet Report')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'View Subject-Wise Marksheet Report', N'SubjectWiseMarkSheetReportsPolicy', @Sub_ExamReports_Exam_Result, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: AdmitCardReportsPolicy (admit card report)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'AdmitCardReportsPolicy' AND [ClaimType] = N'View Admit card Report')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'View Admit card Report', N'AdmitCardReportsPolicy', @Sub_ExamReports_Exam_Result, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');

-- Module: Finance (Finance)
IF NOT EXISTS (SELECT 1 FROM [ProjectModules] WHERE [ModuleName] = N'Finance')
BEGIN
    INSERT INTO [ProjectModules] ([ModuleName], [Status], [Remarks], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Finance', 1, N'Finance', N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
END
DECLARE @Module_Finance INT = (SELECT [Id] FROM [ProjectModules] WHERE [ModuleName] = N'Finance');
-- Submodule: Accounts (Accounts)
IF NOT EXISTS (SELECT 1 FROM [ProjectSubModules] WHERE [SubModuleName] = N'Accounts' AND [ProjectModuleId] = @Module_Finance)
    INSERT INTO [ProjectSubModules] ([SubModuleName], [ProjectModuleId], [Status], [Remarks], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Accounts', @Module_Finance, 1, N'Accounts', N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
DECLARE @Sub_Accounts_Finance INT = (SELECT [Id] FROM [ProjectSubModules] WHERE [SubModuleName] = N'Accounts' AND [ProjectModuleId] = @Module_Finance);
-- Permission: IndexExpensTypesPolicy (expense lookup table)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'IndexExpensTypesPolicy' AND [ClaimType] = N'View Expense Types')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'View Expense Types', N'IndexExpensTypesPolicy', @Sub_Accounts_Finance, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: DetailsExpensTypesPolicy (expense lookup table)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'DetailsExpensTypesPolicy' AND [ClaimType] = N'Details Expense Types')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Details Expense Types', N'DetailsExpensTypesPolicy', @Sub_Accounts_Finance, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: CreateExpensTypesPolicy (expense lookup table)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'CreateExpensTypesPolicy' AND [ClaimType] = N'Create Expense Types')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Create Expense Types', N'CreateExpensTypesPolicy', @Sub_Accounts_Finance, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: EditExpensTypesPolicy (expense lookup table)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'EditExpensTypesPolicy' AND [ClaimType] = N'Edit Expense Types')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Edit Expense Types', N'EditExpensTypesPolicy', @Sub_Accounts_Finance, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: DeleteExpensTypesPolicy (expense lookup table)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'DeleteExpensTypesPolicy' AND [ClaimType] = N'Delete Expense Types')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Delete Expense Types', N'DeleteExpensTypesPolicy', @Sub_Accounts_Finance, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Submodule: StudentFinance (Student Fees)
IF NOT EXISTS (SELECT 1 FROM [ProjectSubModules] WHERE [SubModuleName] = N'StudentFinance' AND [ProjectModuleId] = @Module_Finance)
    INSERT INTO [ProjectSubModules] ([SubModuleName], [ProjectModuleId], [Status], [Remarks], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'StudentFinance', @Module_Finance, 1, N'Student Fees', N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
DECLARE @Sub_StudentFinance_Finance INT = (SELECT [Id] FROM [ProjectSubModules] WHERE [SubModuleName] = N'StudentFinance' AND [ProjectModuleId] = @Module_Finance);
-- Permission: PaymentStudentPaymentsPolicy (from SiteMap.Config menu item)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'PaymentStudentPaymentsPolicy' AND [ClaimType] = N'Details Student Payment')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Details Student Payment', N'PaymentStudentPaymentsPolicy', @Sub_StudentFinance_Finance, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: IndexClassFeeListsPolicy (from SiteMap.Config menu item)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'IndexClassFeeListsPolicy' AND [ClaimType] = N'View Class Fee Lists')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'View Class Fee Lists', N'IndexClassFeeListsPolicy', @Sub_StudentFinance_Finance, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: IndexStudentFeeHeadsPolicy (from SiteMap.Config menu item)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'IndexStudentFeeHeadsPolicy' AND [ClaimType] = N'View Student Fee Heads')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'View Student Fee Heads', N'IndexStudentFeeHeadsPolicy', @Sub_StudentFinance_Finance, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: ViewStudentFeeAllocationsPolicy (from SiteMap.Config menu item)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'ViewStudentFeeAllocationsPolicy' AND [ClaimType] = N'View Student Fee Allocations')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'View Student Fee Allocations', N'ViewStudentFeeAllocationsPolicy', @Sub_StudentFinance_Finance, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: GroupStudentFeeAllocationsPolicy (from SiteMap.Config menu item)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'GroupStudentFeeAllocationsPolicy' AND [ClaimType] = N'Add Student Fee Allocations Group')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Add Student Fee Allocations Group', N'GroupStudentFeeAllocationsPolicy', @Sub_StudentFinance_Finance, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: DetailsClassFeeListsPolicy (page action for the ClassFeeLists screens)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'DetailsClassFeeListsPolicy' AND [ClaimType] = N'Details Class Fee Lists')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Details Class Fee Lists', N'DetailsClassFeeListsPolicy', @Sub_StudentFinance_Finance, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: CreateClassFeeListsPolicy (page action for the ClassFeeLists screens)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'CreateClassFeeListsPolicy' AND [ClaimType] = N'Create Class Fee Lists')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Create Class Fee Lists', N'CreateClassFeeListsPolicy', @Sub_StudentFinance_Finance, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: EditClassFeeListsPolicy (page action for the ClassFeeLists screens)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'EditClassFeeListsPolicy' AND [ClaimType] = N'Edit Class Fee Lists')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Edit Class Fee Lists', N'EditClassFeeListsPolicy', @Sub_StudentFinance_Finance, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: DeleteClassFeeListsPolicy (page action for the ClassFeeLists screens)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'DeleteClassFeeListsPolicy' AND [ClaimType] = N'Delete Class Fee Lists')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Delete Class Fee Lists', N'DeleteClassFeeListsPolicy', @Sub_StudentFinance_Finance, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: IndexStudentFeeAllocationsPolicy (page action for the StudentFeeAllocations screens)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'IndexStudentFeeAllocationsPolicy' AND [ClaimType] = N'View Student Fee Allocations')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'View Student Fee Allocations', N'IndexStudentFeeAllocationsPolicy', @Sub_StudentFinance_Finance, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: DetailsStudentFeeAllocationsPolicy (page action for the StudentFeeAllocations screens)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'DetailsStudentFeeAllocationsPolicy' AND [ClaimType] = N'Details Student Fee Allocations')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Details Student Fee Allocations', N'DetailsStudentFeeAllocationsPolicy', @Sub_StudentFinance_Finance, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: CreateStudentFeeAllocationsPolicy (page action for the StudentFeeAllocations screens)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'CreateStudentFeeAllocationsPolicy' AND [ClaimType] = N'Create Student Fee Allocations')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Create Student Fee Allocations', N'CreateStudentFeeAllocationsPolicy', @Sub_StudentFinance_Finance, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: EditStudentFeeAllocationsPolicy (page action for the StudentFeeAllocations screens)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'EditStudentFeeAllocationsPolicy' AND [ClaimType] = N'Edit Student Fee Allocations')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Edit Student Fee Allocations', N'EditStudentFeeAllocationsPolicy', @Sub_StudentFinance_Finance, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: DeleteStudentFeeAllocationsPolicy (page action for the StudentFeeAllocations screens)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'DeleteStudentFeeAllocationsPolicy' AND [ClaimType] = N'Delete Student Fee Allocations')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Delete Student Fee Allocations', N'DeleteStudentFeeAllocationsPolicy', @Sub_StudentFinance_Finance, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: DetailsStudentFeeHeadsPolicy (page action for the StudentFeeHeads screens)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'DetailsStudentFeeHeadsPolicy' AND [ClaimType] = N'Details Student Fee Head')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Details Student Fee Head', N'DetailsStudentFeeHeadsPolicy', @Sub_StudentFinance_Finance, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: CreateStudentFeeHeadsPolicy (page action for the StudentFeeHeads screens)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'CreateStudentFeeHeadsPolicy' AND [ClaimType] = N'Create Student Fee Head')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Create Student Fee Head', N'CreateStudentFeeHeadsPolicy', @Sub_StudentFinance_Finance, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: EditStudentFeeHeadsPolicy (page action for the StudentFeeHeads screens)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'EditStudentFeeHeadsPolicy' AND [ClaimType] = N'Edit Student Fee Head')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Edit Student Fee Head', N'EditStudentFeeHeadsPolicy', @Sub_StudentFinance_Finance, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: DeleteStudentFeeHeadsPolicy (page action for the StudentFeeHeads screens)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'DeleteStudentFeeHeadsPolicy' AND [ClaimType] = N'Delete Student Fee Head')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Delete Student Fee Head', N'DeleteStudentFeeHeadsPolicy', @Sub_StudentFinance_Finance, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: IndexStudentPaymentDetailsPolicy (payment details live with student fees)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'IndexStudentPaymentDetailsPolicy' AND [ClaimType] = N'View Student Payment Details')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'View Student Payment Details', N'IndexStudentPaymentDetailsPolicy', @Sub_StudentFinance_Finance, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: DetailsStudentPaymentDetailsPolicy (payment details live with student fees)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'DetailsStudentPaymentDetailsPolicy' AND [ClaimType] = N'Details Student Payment Details')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Details Student Payment Details', N'DetailsStudentPaymentDetailsPolicy', @Sub_StudentFinance_Finance, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: CreateStudentPaymentDetailsPolicy (payment details live with student fees)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'CreateStudentPaymentDetailsPolicy' AND [ClaimType] = N'Create Student Payment Details')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Create Student Payment Details', N'CreateStudentPaymentDetailsPolicy', @Sub_StudentFinance_Finance, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: EditStudentPaymentDetailsPolicy (payment details live with student fees)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'EditStudentPaymentDetailsPolicy' AND [ClaimType] = N'Edit Student Payment Details')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Edit Student Payment Details', N'EditStudentPaymentDetailsPolicy', @Sub_StudentFinance_Finance, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: DeleteStudentPaymentDetailsPolicy (payment details live with student fees)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'DeleteStudentPaymentDetailsPolicy' AND [ClaimType] = N'Delete Student Payment Details')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Delete Student Payment Details', N'DeleteStudentPaymentDetailsPolicy', @Sub_StudentFinance_Finance, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: IndexStudentPaymentsPolicy (page action for the StudentPayments screens)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'IndexStudentPaymentsPolicy' AND [ClaimType] = N'View Student Payment')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'View Student Payment', N'IndexStudentPaymentsPolicy', @Sub_StudentFinance_Finance, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: CreateStudentPaymentsPolicy (page action for the StudentPayments screens)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'CreateStudentPaymentsPolicy' AND [ClaimType] = N'Create Student Payment')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Create Student Payment', N'CreateStudentPaymentsPolicy', @Sub_StudentFinance_Finance, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: EditStudentPaymentsPolicy (page action for the StudentPayments screens)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'EditStudentPaymentsPolicy' AND [ClaimType] = N'Edit Student Payment')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Edit Student Payment', N'EditStudentPaymentsPolicy', @Sub_StudentFinance_Finance, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: DeleteStudentPaymentsPolicy (page action for the StudentPayments screens)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'DeleteStudentPaymentsPolicy' AND [ClaimType] = N'Delete Student Payment')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Delete Student Payment', N'DeleteStudentPaymentsPolicy', @Sub_StudentFinance_Finance, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Submodule: DueAmount (Deu (Fees))
IF NOT EXISTS (SELECT 1 FROM [ProjectSubModules] WHERE [SubModuleName] = N'DueAmount' AND [ProjectModuleId] = @Module_Finance)
    INSERT INTO [ProjectSubModules] ([SubModuleName], [ProjectModuleId], [Status], [Remarks], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'DueAmount', @Module_Finance, 1, N'Deu (Fees)', N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
DECLARE @Sub_DueAmount_Finance INT = (SELECT [Id] FROM [ProjectSubModules] WHERE [SubModuleName] = N'DueAmount' AND [ProjectModuleId] = @Module_Finance);
-- Permission: DuePaymentStudentPaymentsPolicy (from SiteMap.Config menu item)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'DuePaymentStudentPaymentsPolicy' AND [ClaimType] = N'Student Due Payment')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Student Due Payment', N'DuePaymentStudentPaymentsPolicy', @Sub_DueAmount_Finance, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: PreviousDuePaymentStudentPaymentsPolicy (from SiteMap.Config menu item)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'PreviousDuePaymentStudentPaymentsPolicy' AND [ClaimType] = N'View Previous Due Amount')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'View Previous Due Amount', N'PreviousDuePaymentStudentPaymentsPolicy', @Sub_DueAmount_Finance, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: DueAmountStudentsPolicy (page shows due totals; kept with due payments)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'DueAmountStudentsPolicy' AND [ClaimType] = N'View Due Amount')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'View Due Amount', N'DueAmountStudentsPolicy', @Sub_DueAmount_Finance, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Submodule: StaffFinance (Payroll)
IF NOT EXISTS (SELECT 1 FROM [ProjectSubModules] WHERE [SubModuleName] = N'StaffFinance' AND [ProjectModuleId] = @Module_Finance)
    INSERT INTO [ProjectSubModules] ([SubModuleName], [ProjectModuleId], [Status], [Remarks], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'StaffFinance', @Module_Finance, 1, N'Payroll', N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
DECLARE @Sub_StaffFinance_Finance INT = (SELECT [Id] FROM [ProjectSubModules] WHERE [SubModuleName] = N'StaffFinance' AND [ProjectModuleId] = @Module_Finance);
-- Submodule: FinanceReports (Reports)
IF NOT EXISTS (SELECT 1 FROM [ProjectSubModules] WHERE [SubModuleName] = N'FinanceReports' AND [ProjectModuleId] = @Module_Finance)
    INSERT INTO [ProjectSubModules] ([SubModuleName], [ProjectModuleId], [Status], [Remarks], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'FinanceReports', @Module_Finance, 1, N'Reports', N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
DECLARE @Sub_FinanceReports_Finance INT = (SELECT [Id] FROM [ProjectSubModules] WHERE [SubModuleName] = N'FinanceReports' AND [ProjectModuleId] = @Module_Finance);
-- Permission: StudentPaymentInfoReportsPolicy (from SiteMap.Config menu item)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'StudentPaymentInfoReportsPolicy' AND [ClaimType] = N'View Student Payment Details Report')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'View Student Payment Details Report', N'StudentPaymentInfoReportsPolicy', @Sub_FinanceReports_Finance, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: StudentPaymentReportsPolicy (from SiteMap.Config menu item)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'StudentPaymentReportsPolicy' AND [ClaimType] = N'View Student payment Report')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'View Student payment Report', N'StudentPaymentReportsPolicy', @Sub_FinanceReports_Finance, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: ReceiptPaymentReportsPolicy (payment receipt report)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'ReceiptPaymentReportsPolicy' AND [ClaimType] = N'View Payment Receipt Report')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'View Payment Receipt Report', N'ReceiptPaymentReportsPolicy', @Sub_FinanceReports_Finance, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');

-- Module: Library (Library)
IF NOT EXISTS (SELECT 1 FROM [ProjectModules] WHERE [ModuleName] = N'Library')
BEGIN
    INSERT INTO [ProjectModules] ([ModuleName], [Status], [Remarks], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Library', 1, N'Library', N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
END
DECLARE @Module_Library INT = (SELECT [Id] FROM [ProjectModules] WHERE [ModuleName] = N'Library');
-- Submodule: LibrarySetup (Setup)
IF NOT EXISTS (SELECT 1 FROM [ProjectSubModules] WHERE [SubModuleName] = N'LibrarySetup' AND [ProjectModuleId] = @Module_Library)
    INSERT INTO [ProjectSubModules] ([SubModuleName], [ProjectModuleId], [Status], [Remarks], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'LibrarySetup', @Module_Library, 1, N'Setup', N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
DECLARE @Sub_LibrarySetup_Library INT = (SELECT [Id] FROM [ProjectSubModules] WHERE [SubModuleName] = N'LibrarySetup' AND [ProjectModuleId] = @Module_Library);
-- Submodule: LibraryOperations (Operations)
IF NOT EXISTS (SELECT 1 FROM [ProjectSubModules] WHERE [SubModuleName] = N'LibraryOperations' AND [ProjectModuleId] = @Module_Library)
    INSERT INTO [ProjectSubModules] ([SubModuleName], [ProjectModuleId], [Status], [Remarks], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'LibraryOperations', @Module_Library, 1, N'Operations', N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
DECLARE @Sub_LibraryOperations_Library INT = (SELECT [Id] FROM [ProjectSubModules] WHERE [SubModuleName] = N'LibraryOperations' AND [ProjectModuleId] = @Module_Library);
-- Submodule: LibraryReports (Reports)
IF NOT EXISTS (SELECT 1 FROM [ProjectSubModules] WHERE [SubModuleName] = N'LibraryReports' AND [ProjectModuleId] = @Module_Library)
    INSERT INTO [ProjectSubModules] ([SubModuleName], [ProjectModuleId], [Status], [Remarks], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'LibraryReports', @Module_Library, 1, N'Reports', N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
DECLARE @Sub_LibraryReports_Library INT = (SELECT [Id] FROM [ProjectSubModules] WHERE [SubModuleName] = N'LibraryReports' AND [ProjectModuleId] = @Module_Library);

-- Module: Transport (Transport)
IF NOT EXISTS (SELECT 1 FROM [ProjectModules] WHERE [ModuleName] = N'Transport')
BEGIN
    INSERT INTO [ProjectModules] ([ModuleName], [Status], [Remarks], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Transport', 1, N'Transport', N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
END
DECLARE @Module_Transport INT = (SELECT [Id] FROM [ProjectModules] WHERE [ModuleName] = N'Transport');
-- Submodule: TransportSetup (Setup)
IF NOT EXISTS (SELECT 1 FROM [ProjectSubModules] WHERE [SubModuleName] = N'TransportSetup' AND [ProjectModuleId] = @Module_Transport)
    INSERT INTO [ProjectSubModules] ([SubModuleName], [ProjectModuleId], [Status], [Remarks], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'TransportSetup', @Module_Transport, 1, N'Setup', N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
DECLARE @Sub_TransportSetup_Transport INT = (SELECT [Id] FROM [ProjectSubModules] WHERE [SubModuleName] = N'TransportSetup' AND [ProjectModuleId] = @Module_Transport);
-- Submodule: TransportOperations (Operations)
IF NOT EXISTS (SELECT 1 FROM [ProjectSubModules] WHERE [SubModuleName] = N'TransportOperations' AND [ProjectModuleId] = @Module_Transport)
    INSERT INTO [ProjectSubModules] ([SubModuleName], [ProjectModuleId], [Status], [Remarks], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'TransportOperations', @Module_Transport, 1, N'Operations', N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
DECLARE @Sub_TransportOperations_Transport INT = (SELECT [Id] FROM [ProjectSubModules] WHERE [SubModuleName] = N'TransportOperations' AND [ProjectModuleId] = @Module_Transport);
-- Submodule: TransportReports (Reports)
IF NOT EXISTS (SELECT 1 FROM [ProjectSubModules] WHERE [SubModuleName] = N'TransportReports' AND [ProjectModuleId] = @Module_Transport)
    INSERT INTO [ProjectSubModules] ([SubModuleName], [ProjectModuleId], [Status], [Remarks], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'TransportReports', @Module_Transport, 1, N'Reports', N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
DECLARE @Sub_TransportReports_Transport INT = (SELECT [Id] FROM [ProjectSubModules] WHERE [SubModuleName] = N'TransportReports' AND [ProjectModuleId] = @Module_Transport);

-- Module: Communication (Communications)
IF NOT EXISTS (SELECT 1 FROM [ProjectModules] WHERE [ModuleName] = N'Communication')
BEGIN
    INSERT INTO [ProjectModules] ([ModuleName], [Status], [Remarks], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Communication', 1, N'Communications', N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
END
DECLARE @Module_Communication INT = (SELECT [Id] FROM [ProjectModules] WHERE [ModuleName] = N'Communication');
-- Submodule: Messaging (SMS Panel)
IF NOT EXISTS (SELECT 1 FROM [ProjectSubModules] WHERE [SubModuleName] = N'Messaging' AND [ProjectModuleId] = @Module_Communication)
    INSERT INTO [ProjectSubModules] ([SubModuleName], [ProjectModuleId], [Status], [Remarks], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Messaging', @Module_Communication, 1, N'SMS Panel', N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
DECLARE @Sub_Messaging_Communication INT = (SELECT [Id] FROM [ProjectSubModules] WHERE [SubModuleName] = N'Messaging' AND [ProjectModuleId] = @Module_Communication);
-- Permission: IndexPhoneSMSPolicy (from SiteMap.Config menu item)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'IndexPhoneSMSPolicy' AND [ClaimType] = N'View Phone SMS')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'View Phone SMS', N'IndexPhoneSMSPolicy', @Sub_Messaging_Communication, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: GetAPIDataPhoneSMSPolicy (from SiteMap.Config menu item)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'GetAPIDataPhoneSMSPolicy' AND [ClaimType] = N'View SMS API Data')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'View SMS API Data', N'GetAPIDataPhoneSMSPolicy', @Sub_Messaging_Communication, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: CreatePhoneSMSPolicy (page action for the PhoneSMS screens)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'CreatePhoneSMSPolicy' AND [ClaimType] = N'Create Phone SMS')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Create Phone SMS', N'CreatePhoneSMSPolicy', @Sub_Messaging_Communication, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Submodule: Setup (Notification Setup)
IF NOT EXISTS (SELECT 1 FROM [ProjectSubModules] WHERE [SubModuleName] = N'Setup' AND [ProjectModuleId] = @Module_Communication)
    INSERT INTO [ProjectSubModules] ([SubModuleName], [ProjectModuleId], [Status], [Remarks], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Setup', @Module_Communication, 1, N'Notification Setup', N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
DECLARE @Sub_Setup_Communication INT = (SELECT [Id] FROM [ProjectSubModules] WHERE [SubModuleName] = N'Setup' AND [ProjectModuleId] = @Module_Communication);
-- Permission: SMSControlSetupPolicy (from SiteMap.Config menu item)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'SMSControlSetupPolicy' AND [ClaimType] = N'SMS Control')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'SMS Control', N'SMSControlSetupPolicy', @Sub_Setup_Communication, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: StudentWiseSMSServiceSetupPolicy (page action for the Setup screens)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'StudentWiseSMSServiceSetupPolicy' AND [ClaimType] = N'Student-Wise SMS Service Setup')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Student-Wise SMS Service Setup', N'StudentWiseSMSServiceSetupPolicy', @Sub_Setup_Communication, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Submodule: CommunicationReports (Reports)
IF NOT EXISTS (SELECT 1 FROM [ProjectSubModules] WHERE [SubModuleName] = N'CommunicationReports' AND [ProjectModuleId] = @Module_Communication)
    INSERT INTO [ProjectSubModules] ([SubModuleName], [ProjectModuleId], [Status], [Remarks], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'CommunicationReports', @Module_Communication, 1, N'Reports', N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
DECLARE @Sub_CommunicationReports_Communication INT = (SELECT [Id] FROM [ProjectSubModules] WHERE [SubModuleName] = N'CommunicationReports' AND [ProjectModuleId] = @Module_Communication);

-- Module: Ticket (Tickets)
IF NOT EXISTS (SELECT 1 FROM [ProjectModules] WHERE [ModuleName] = N'Ticket')
BEGIN
    INSERT INTO [ProjectModules] ([ModuleName], [Status], [Remarks], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Ticket', 1, N'Tickets', N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
END
DECLARE @Module_Ticket INT = (SELECT [Id] FROM [ProjectModules] WHERE [ModuleName] = N'Ticket');
-- Submodule: TicketDesk (Ticket Desk)
IF NOT EXISTS (SELECT 1 FROM [ProjectSubModules] WHERE [SubModuleName] = N'TicketDesk' AND [ProjectModuleId] = @Module_Ticket)
    INSERT INTO [ProjectSubModules] ([SubModuleName], [ProjectModuleId], [Status], [Remarks], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'TicketDesk', @Module_Ticket, 1, N'Ticket Desk', N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
DECLARE @Sub_TicketDesk_Ticket INT = (SELECT [Id] FROM [ProjectSubModules] WHERE [SubModuleName] = N'TicketDesk' AND [ProjectModuleId] = @Module_Ticket);
-- Permission: IndexTicketsPolicy (from SiteMap.Config menu item)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'IndexTicketsPolicy' AND [ClaimType] = N'View Tickets')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'View Tickets', N'IndexTicketsPolicy', @Sub_TicketDesk_Ticket, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: CreateTicketsPolicy (from SiteMap.Config menu item)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'CreateTicketsPolicy' AND [ClaimType] = N'Create Ticket')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Create Ticket', N'CreateTicketsPolicy', @Sub_TicketDesk_Ticket, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: DetailsTicketsPolicy (page action for the Tickets screens)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'DetailsTicketsPolicy' AND [ClaimType] = N'View Details Ticket')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'View Details Ticket', N'DetailsTicketsPolicy', @Sub_TicketDesk_Ticket, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: EditTicketsPolicy (page action for the Tickets screens)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'EditTicketsPolicy' AND [ClaimType] = N'Edit Ticket')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Edit Ticket', N'EditTicketsPolicy', @Sub_TicketDesk_Ticket, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: DeleteTicketsPolicy (page action for the Tickets screens)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'DeleteTicketsPolicy' AND [ClaimType] = N'Delete Ticket')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Delete Ticket', N'DeleteTicketsPolicy', @Sub_TicketDesk_Ticket, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: ChangeStatusTicketsPolicy (page action for the Tickets screens)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'ChangeStatusTicketsPolicy' AND [ClaimType] = N'Change Ticket Status')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Change Ticket Status', N'ChangeStatusTicketsPolicy', @Sub_TicketDesk_Ticket, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: CommentTicketsPolicy (page action for the Tickets screens)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'CommentTicketsPolicy' AND [ClaimType] = N'Comment On Ticket')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Comment On Ticket', N'CommentTicketsPolicy', @Sub_TicketDesk_Ticket, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');

-- Module: ApplicationSetup (Application Setup)
IF NOT EXISTS (SELECT 1 FROM [ProjectModules] WHERE [ModuleName] = N'ApplicationSetup')
BEGIN
    INSERT INTO [ProjectModules] ([ModuleName], [Status], [Remarks], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'ApplicationSetup', 1, N'Application Setup', N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
END
DECLARE @Module_ApplicationSetup INT = (SELECT [Id] FROM [ProjectModules] WHERE [ModuleName] = N'ApplicationSetup');
-- Submodule: ModuleHierarchy (Module Hierarchy)
IF NOT EXISTS (SELECT 1 FROM [ProjectSubModules] WHERE [SubModuleName] = N'ModuleHierarchy' AND [ProjectModuleId] = @Module_ApplicationSetup)
    INSERT INTO [ProjectSubModules] ([SubModuleName], [ProjectModuleId], [Status], [Remarks], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'ModuleHierarchy', @Module_ApplicationSetup, 1, N'Module Hierarchy', N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
DECLARE @Sub_ModuleHierarchy_ApplicationSetup INT = (SELECT [Id] FROM [ProjectSubModules] WHERE [SubModuleName] = N'ModuleHierarchy' AND [ProjectModuleId] = @Module_ApplicationSetup);
-- Permission: IndexProjectModulesPolicy (from SiteMap.Config menu item)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'IndexProjectModulesPolicy' AND [ClaimType] = N'View Project Modules')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'View Project Modules', N'IndexProjectModulesPolicy', @Sub_ModuleHierarchy_ApplicationSetup, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: IndexClaimStoresPolicy (from SiteMap.Config menu item)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'IndexClaimStoresPolicy' AND [ClaimType] = N'View Claim Store')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'View Claim Store', N'IndexClaimStoresPolicy', @Sub_ModuleHierarchy_ApplicationSetup, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: CreateClaimStoresPolicy (page action for the ClaimStores screens)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'CreateClaimStoresPolicy' AND [ClaimType] = N'Create Claim Store')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Create Claim Store', N'CreateClaimStoresPolicy', @Sub_ModuleHierarchy_ApplicationSetup, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: EditClaimStoresPolicy (page action for the ClaimStores screens)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'EditClaimStoresPolicy' AND [ClaimType] = N'Edit Claim Store')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Edit Claim Store', N'EditClaimStoresPolicy', @Sub_ModuleHierarchy_ApplicationSetup, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: DeleteClaimStoresPolicy (page action for the ClaimStores screens)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'DeleteClaimStoresPolicy' AND [ClaimType] = N'Delete Claim Store')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Delete Claim Store', N'DeleteClaimStoresPolicy', @Sub_ModuleHierarchy_ApplicationSetup, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: DetailsProjectModulesPolicy (page action for the ProjectModules screens)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'DetailsProjectModulesPolicy' AND [ClaimType] = N'Details Project Modules')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Details Project Modules', N'DetailsProjectModulesPolicy', @Sub_ModuleHierarchy_ApplicationSetup, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: CreateProjectModulesPolicy (page action for the ProjectModules screens)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'CreateProjectModulesPolicy' AND [ClaimType] = N'Create Project Modules')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Create Project Modules', N'CreateProjectModulesPolicy', @Sub_ModuleHierarchy_ApplicationSetup, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: EditProjectModulesPolicy (page action for the ProjectModules screens)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'EditProjectModulesPolicy' AND [ClaimType] = N'Edit Project Modules')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Edit Project Modules', N'EditProjectModulesPolicy', @Sub_ModuleHierarchy_ApplicationSetup, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: DeleteProjectModulesPolicy (page action for the ProjectModules screens)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'DeleteProjectModulesPolicy' AND [ClaimType] = N'Delete Project Modules')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Delete Project Modules', N'DeleteProjectModulesPolicy', @Sub_ModuleHierarchy_ApplicationSetup, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: IndexProjectSubModulesPolicy (Sub-Modules row lives under Module Hierarchy)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'IndexProjectSubModulesPolicy' AND [ClaimType] = N'View Project Sub Modules')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'View Project Sub Modules', N'IndexProjectSubModulesPolicy', @Sub_ModuleHierarchy_ApplicationSetup, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: DetailsProjectSubModulesPolicy (Sub-Modules row lives under Module Hierarchy)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'DetailsProjectSubModulesPolicy' AND [ClaimType] = N'Details Project Sub Modules')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Details Project Sub Modules', N'DetailsProjectSubModulesPolicy', @Sub_ModuleHierarchy_ApplicationSetup, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: CreateProjectSubModulesPolicy (Sub-Modules row lives under Module Hierarchy)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'CreateProjectSubModulesPolicy' AND [ClaimType] = N'Create Project Sub Modules')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Create Project Sub Modules', N'CreateProjectSubModulesPolicy', @Sub_ModuleHierarchy_ApplicationSetup, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: EditProjectSubModulesPolicy (Sub-Modules row lives under Module Hierarchy)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'EditProjectSubModulesPolicy' AND [ClaimType] = N'Edit Project Sub Modules')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Edit Project Sub Modules', N'EditProjectSubModulesPolicy', @Sub_ModuleHierarchy_ApplicationSetup, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
-- Permission: DeleteProjectSubModulesPolicy (Sub-Modules row lives under Module Hierarchy)
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'DeleteProjectSubModulesPolicy' AND [ClaimType] = N'Delete Project Sub Modules')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Delete Project Sub Modules', N'DeleteProjectSubModulesPolicy', @Sub_ModuleHierarchy_ApplicationSetup, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
