-- ============================================================================
-- Online Admission permissions - POSTGRESQL (same content as the SQL Server script)
-- IDEMPOTENT. After running: grant the claims to roles via Admin > User Profile.
-- ============================================================================
DO $$
DECLARE
    module_id integer;
    sub_id integer;
BEGIN
    IF NOT EXISTS (SELECT 1 FROM "ProjectModules" WHERE "ModuleName" = 'OnlineAdmission') THEN
        INSERT INTO "ProjectModules" ("ModuleName", "Status", "Remarks", "CreatedBy", "CreatedAt", "EditedBy", "EditedAt", "MACAddress")
        VALUES ('OnlineAdmission', true, 'Admission', 'SiteMapSeed', now() AT TIME ZONE 'utc', 'SiteMapSeed', now() AT TIME ZONE 'utc', '');
    END IF;
    SELECT "Id" INTO module_id FROM "ProjectModules" WHERE "ModuleName" = 'OnlineAdmission';

    IF NOT EXISTS (SELECT 1 FROM "ProjectSubModules" WHERE "SubModuleName" = 'AdmissionDesk' AND "ProjectModuleId" = module_id) THEN
        INSERT INTO "ProjectSubModules" ("SubModuleName", "ProjectModuleId", "Status", "Remarks", "CreatedBy", "CreatedAt", "EditedBy", "EditedAt", "MACAddress")
        VALUES ('AdmissionDesk', module_id, true, 'Applications', 'SiteMapSeed', now() AT TIME ZONE 'utc', 'SiteMapSeed', now() AT TIME ZONE 'utc', '');
    END IF;
    SELECT "Id" INTO sub_id FROM "ProjectSubModules" WHERE "SubModuleName" = 'AdmissionDesk' AND "ProjectModuleId" = module_id;
    IF NOT EXISTS (SELECT 1 FROM "ClaimStores" WHERE "ClaimValue" = 'DashboardAdmissionPolicy' AND "ClaimType" = 'View Admission Dashboard') THEN
        INSERT INTO "ClaimStores" ("ClaimType", "ClaimValue", "SubModuleId", "CreatedBy", "CreatedAt", "EditedBy", "EditedAt", "MACAddress")
        VALUES ('View Admission Dashboard', 'DashboardAdmissionPolicy', sub_id, 'SiteMapSeed', now() AT TIME ZONE 'utc', 'SiteMapSeed', now() AT TIME ZONE 'utc', '');
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "ClaimStores" WHERE "ClaimValue" = 'IndexAdmissionApplicationsPolicy' AND "ClaimType" = 'View Admission Applications') THEN
        INSERT INTO "ClaimStores" ("ClaimType", "ClaimValue", "SubModuleId", "CreatedBy", "CreatedAt", "EditedBy", "EditedAt", "MACAddress")
        VALUES ('View Admission Applications', 'IndexAdmissionApplicationsPolicy', sub_id, 'SiteMapSeed', now() AT TIME ZONE 'utc', 'SiteMapSeed', now() AT TIME ZONE 'utc', '');
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "ClaimStores" WHERE "ClaimValue" = 'ReviewAdmissionApplicationsPolicy' AND "ClaimType" = 'Review Admission Applications') THEN
        INSERT INTO "ClaimStores" ("ClaimType", "ClaimValue", "SubModuleId", "CreatedBy", "CreatedAt", "EditedBy", "EditedAt", "MACAddress")
        VALUES ('Review Admission Applications', 'ReviewAdmissionApplicationsPolicy', sub_id, 'SiteMapSeed', now() AT TIME ZONE 'utc', 'SiteMapSeed', now() AT TIME ZONE 'utc', '');
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "ClaimStores" WHERE "ClaimValue" = 'DecideAdmissionApplicationsPolicy' AND "ClaimType" = 'Approve Admission Applications') THEN
        INSERT INTO "ClaimStores" ("ClaimType", "ClaimValue", "SubModuleId", "CreatedBy", "CreatedAt", "EditedBy", "EditedAt", "MACAddress")
        VALUES ('Approve Admission Applications', 'DecideAdmissionApplicationsPolicy', sub_id, 'SiteMapSeed', now() AT TIME ZONE 'utc', 'SiteMapSeed', now() AT TIME ZONE 'utc', '');
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "ClaimStores" WHERE "ClaimValue" = 'EnrolAdmissionApplicationsPolicy' AND "ClaimType" = 'Enrol Admission Applicants') THEN
        INSERT INTO "ClaimStores" ("ClaimType", "ClaimValue", "SubModuleId", "CreatedBy", "CreatedAt", "EditedBy", "EditedAt", "MACAddress")
        VALUES ('Enrol Admission Applicants', 'EnrolAdmissionApplicationsPolicy', sub_id, 'SiteMapSeed', now() AT TIME ZONE 'utc', 'SiteMapSeed', now() AT TIME ZONE 'utc', '');
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "ClaimStores" WHERE "ClaimValue" = 'IndexAdmissionPaymentsPolicy' AND "ClaimType" = 'View Admission Payments') THEN
        INSERT INTO "ClaimStores" ("ClaimType", "ClaimValue", "SubModuleId", "CreatedBy", "CreatedAt", "EditedBy", "EditedAt", "MACAddress")
        VALUES ('View Admission Payments', 'IndexAdmissionPaymentsPolicy', sub_id, 'SiteMapSeed', now() AT TIME ZONE 'utc', 'SiteMapSeed', now() AT TIME ZONE 'utc', '');
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "ClaimStores" WHERE "ClaimValue" = 'VerifyAdmissionPaymentsPolicy' AND "ClaimType" = 'Verify Admission Payments') THEN
        INSERT INTO "ClaimStores" ("ClaimType", "ClaimValue", "SubModuleId", "CreatedBy", "CreatedAt", "EditedBy", "EditedAt", "MACAddress")
        VALUES ('Verify Admission Payments', 'VerifyAdmissionPaymentsPolicy', sub_id, 'SiteMapSeed', now() AT TIME ZONE 'utc', 'SiteMapSeed', now() AT TIME ZONE 'utc', '');
    END IF;

    IF NOT EXISTS (SELECT 1 FROM "ProjectSubModules" WHERE "SubModuleName" = 'AdmissionConfiguration' AND "ProjectModuleId" = module_id) THEN
        INSERT INTO "ProjectSubModules" ("SubModuleName", "ProjectModuleId", "Status", "Remarks", "CreatedBy", "CreatedAt", "EditedBy", "EditedAt", "MACAddress")
        VALUES ('AdmissionConfiguration', module_id, true, 'Setup', 'SiteMapSeed', now() AT TIME ZONE 'utc', 'SiteMapSeed', now() AT TIME ZONE 'utc', '');
    END IF;
    SELECT "Id" INTO sub_id FROM "ProjectSubModules" WHERE "SubModuleName" = 'AdmissionConfiguration' AND "ProjectModuleId" = module_id;
    IF NOT EXISTS (SELECT 1 FROM "ClaimStores" WHERE "ClaimValue" = 'IndexAdmissionSetupPolicy' AND "ClaimType" = 'View Admission Setup') THEN
        INSERT INTO "ClaimStores" ("ClaimType", "ClaimValue", "SubModuleId", "CreatedBy", "CreatedAt", "EditedBy", "EditedAt", "MACAddress")
        VALUES ('View Admission Setup', 'IndexAdmissionSetupPolicy', sub_id, 'SiteMapSeed', now() AT TIME ZONE 'utc', 'SiteMapSeed', now() AT TIME ZONE 'utc', '');
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "ClaimStores" WHERE "ClaimValue" = 'ManageAdmissionSetupPolicy' AND "ClaimType" = 'Manage Admission Setup') THEN
        INSERT INTO "ClaimStores" ("ClaimType", "ClaimValue", "SubModuleId", "CreatedBy", "CreatedAt", "EditedBy", "EditedAt", "MACAddress")
        VALUES ('Manage Admission Setup', 'ManageAdmissionSetupPolicy', sub_id, 'SiteMapSeed', now() AT TIME ZONE 'utc', 'SiteMapSeed', now() AT TIME ZONE 'utc', '');
    END IF;
END $$;
