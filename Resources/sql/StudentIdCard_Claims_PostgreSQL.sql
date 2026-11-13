-- ============================================================================
-- Student ID Card permission - POSTGRESQL (same content as the SQL Server script)
-- Adds the "View Student ID Card" permission (policy IdCardStudentsPolicy) used by
-- the Students > ID Cards screen, under the StudentManagement > StudentRecords menu.
-- IDEMPOTENT. After running, assign the claim to roles/users via Admin > User Profile
-- (or edit target_email below to grant one account immediately). The user must LOG
-- OUT and back in for the new claim to take effect.
-- ============================================================================
DO $$
DECLARE
    module_id integer;
    sub_id integer;
    target_email text := 'admin@eims.com';   -- <<< change to the account that needs access
    user_id text;
BEGIN
    -- module + submodule
    IF NOT EXISTS (SELECT 1 FROM "ProjectModules" WHERE "ModuleName" = 'StudentManagement') THEN
        INSERT INTO "ProjectModules" ("ModuleName", "Status", "Remarks", "CreatedBy", "CreatedAt", "EditedBy", "EditedAt", "MACAddress")
        VALUES ('StudentManagement', true, 'Students', 'SiteMapSeed', now() AT TIME ZONE 'utc', 'SiteMapSeed', now() AT TIME ZONE 'utc', '');
    END IF;
    SELECT "Id" INTO module_id FROM "ProjectModules" WHERE "ModuleName" = 'StudentManagement';

    IF NOT EXISTS (SELECT 1 FROM "ProjectSubModules" WHERE "SubModuleName" = 'StudentRecords' AND "ProjectModuleId" = module_id) THEN
        INSERT INTO "ProjectSubModules" ("SubModuleName", "ProjectModuleId", "Status", "Remarks", "CreatedBy", "CreatedAt", "EditedBy", "EditedAt", "MACAddress")
        VALUES ('StudentRecords', module_id, true, 'Records', 'SiteMapSeed', now() AT TIME ZONE 'utc', 'SiteMapSeed', now() AT TIME ZONE 'utc', '');
    END IF;
    SELECT "Id" INTO sub_id FROM "ProjectSubModules" WHERE "SubModuleName" = 'StudentRecords' AND "ProjectModuleId" = module_id;

    -- the permission
    IF NOT EXISTS (SELECT 1 FROM "ClaimStores" WHERE "ClaimValue" = 'IdCardStudentsPolicy' AND "ClaimType" = 'View Student ID Card') THEN
        INSERT INTO "ClaimStores" ("ClaimType", "ClaimValue", "SubModuleId", "CreatedBy", "CreatedAt", "EditedBy", "EditedAt", "MACAddress")
        VALUES ('View Student ID Card', 'IdCardStudentsPolicy', sub_id, 'SiteMapSeed', now() AT TIME ZONE 'utc', 'SiteMapSeed', now() AT TIME ZONE 'utc', '');
    END IF;

    -- optional: grant to one account for immediate access
    SELECT "Id" INTO user_id FROM "AspNetUsers" WHERE "Email" = target_email;
    IF user_id IS NOT NULL AND NOT EXISTS (
        SELECT 1 FROM "AspNetUserClaims" WHERE "UserId" = user_id AND "ClaimType" = 'View Student ID Card') THEN
        INSERT INTO "AspNetUserClaims" ("UserId", "ClaimType", "ClaimValue")
        VALUES (user_id, 'View Student ID Card', 'IdCardStudentsPolicy');
    END IF;
END $$;
