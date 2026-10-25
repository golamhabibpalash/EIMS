

IF NOT EXISTS (SELECT 1 FROM [ProjectModules] WHERE [ModuleName] = N'OnlineAdmission')
    INSERT INTO [ProjectModules] ([ModuleName], [Status], [Remarks], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'OnlineAdmission', 1, N'Admission', N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
DECLARE @Module_OnlineAdmission INT = (SELECT [Id] FROM [ProjectModules] WHERE [ModuleName] = N'OnlineAdmission');
IF NOT EXISTS (SELECT 1 FROM [ProjectSubModules] WHERE [SubModuleName] = N'AdmissionDesk' AND [ProjectModuleId] = @Module_OnlineAdmission)
    INSERT INTO [ProjectSubModules] ([SubModuleName], [ProjectModuleId], [Status], [Remarks], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'AdmissionDesk', @Module_OnlineAdmission, 1, N'Applications', N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
DECLARE @Sub_AdmissionDesk INT = (SELECT [Id] FROM [ProjectSubModules] WHERE [SubModuleName] = N'AdmissionDesk' AND [ProjectModuleId] = @Module_OnlineAdmission);
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'DashboardAdmissionPolicy' AND [ClaimType] = N'View Admission Dashboard')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'View Admission Dashboard', N'DashboardAdmissionPolicy', @Sub_AdmissionDesk, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'IndexAdmissionApplicationsPolicy' AND [ClaimType] = N'View Admission Applications')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'View Admission Applications', N'IndexAdmissionApplicationsPolicy', @Sub_AdmissionDesk, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'ReviewAdmissionApplicationsPolicy' AND [ClaimType] = N'Review Admission Applications')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Review Admission Applications', N'ReviewAdmissionApplicationsPolicy', @Sub_AdmissionDesk, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'DecideAdmissionApplicationsPolicy' AND [ClaimType] = N'Approve Admission Applications')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Approve Admission Applications', N'DecideAdmissionApplicationsPolicy', @Sub_AdmissionDesk, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'EnrolAdmissionApplicationsPolicy' AND [ClaimType] = N'Enrol Admission Applicants')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Enrol Admission Applicants', N'EnrolAdmissionApplicationsPolicy', @Sub_AdmissionDesk, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'IndexAdmissionPaymentsPolicy' AND [ClaimType] = N'View Admission Payments')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'View Admission Payments', N'IndexAdmissionPaymentsPolicy', @Sub_AdmissionDesk, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'VerifyAdmissionPaymentsPolicy' AND [ClaimType] = N'Verify Admission Payments')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Verify Admission Payments', N'VerifyAdmissionPaymentsPolicy', @Sub_AdmissionDesk, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
IF NOT EXISTS (SELECT 1 FROM [ProjectSubModules] WHERE [SubModuleName] = N'AdmissionConfiguration' AND [ProjectModuleId] = @Module_OnlineAdmission)
    INSERT INTO [ProjectSubModules] ([SubModuleName], [ProjectModuleId], [Status], [Remarks], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'AdmissionConfiguration', @Module_OnlineAdmission, 1, N'Setup', N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
DECLARE @Sub_AdmissionConfiguration INT = (SELECT [Id] FROM [ProjectSubModules] WHERE [SubModuleName] = N'AdmissionConfiguration' AND [ProjectModuleId] = @Module_OnlineAdmission);
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'IndexAdmissionSetupPolicy' AND [ClaimType] = N'View Admission Setup')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'View Admission Setup', N'IndexAdmissionSetupPolicy', @Sub_AdmissionConfiguration, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
IF NOT EXISTS (SELECT 1 FROM [ClaimStores] WHERE [ClaimValue] = N'ManageAdmissionSetupPolicy' AND [ClaimType] = N'Manage Admission Setup')
    INSERT INTO [ClaimStores] ([ClaimType], [ClaimValue], [SubModuleId], [CreatedBy], [CreatedAt], [EditedBy], [EditedAt], [MACAddress])
    VALUES (N'Manage Admission Setup', N'ManageAdmissionSetupPolicy', @Sub_AdmissionConfiguration, N'SiteMapSeed', SYSUTCDATETIME(), N'SiteMapSeed', SYSUTCDATETIME(), N'');
