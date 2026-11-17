-- ============================================================================
-- "Reset User Password" permission - POSTGRESQL (same content as the SQL Server script)
-- Adds the permission (policy ResetUserPasswordPolicy) used by Admin > Security >
-- Reset Password and grants it to every SuperAdmin account. IDEMPOTENT.
-- Run per school database, e.g. psql -p 5433 -d sbd_<code> -f this.sql
-- (new schools created with `platform tenant-init` get it automatically).
-- ============================================================================
DO $$
DECLARE
    module_id integer;
    sub_id integer;
BEGIN
    IF NOT EXISTS (SELECT 1 FROM "ProjectModules" WHERE "ModuleName" = 'Administration') THEN
        INSERT INTO "ProjectModules" ("ModuleName", "Status", "Remarks", "CreatedBy", "CreatedAt", "EditedBy", "EditedAt", "MACAddress")
        VALUES ('Administration', true, 'Admin', 'SiteMapSeed', now() AT TIME ZONE 'utc', 'SiteMapSeed', now() AT TIME ZONE 'utc', '');
    END IF;
    SELECT "Id" INTO module_id FROM "ProjectModules" WHERE "ModuleName" = 'Administration';

    IF NOT EXISTS (SELECT 1 FROM "ProjectSubModules" WHERE "SubModuleName" = 'SecuritySetup' AND "ProjectModuleId" = module_id) THEN
        INSERT INTO "ProjectSubModules" ("SubModuleName", "ProjectModuleId", "Status", "Remarks", "CreatedBy", "CreatedAt", "EditedBy", "EditedAt", "MACAddress")
        VALUES ('SecuritySetup', module_id, true, 'Security', 'SiteMapSeed', now() AT TIME ZONE 'utc', 'SiteMapSeed', now() AT TIME ZONE 'utc', '');
    END IF;
    SELECT "Id" INTO sub_id FROM "ProjectSubModules" WHERE "SubModuleName" = 'SecuritySetup' AND "ProjectModuleId" = module_id;

    IF NOT EXISTS (SELECT 1 FROM "ClaimStores" WHERE "ClaimValue" = 'ResetUserPasswordPolicy' AND "ClaimType" = 'Reset User Password') THEN
        INSERT INTO "ClaimStores" ("ClaimType", "ClaimValue", "SubModuleId", "CreatedBy", "CreatedAt", "EditedBy", "EditedAt", "MACAddress")
        VALUES ('Reset User Password', 'ResetUserPasswordPolicy', sub_id, 'SiteMapSeed', now() AT TIME ZONE 'utc', 'SiteMapSeed', now() AT TIME ZONE 'utc', '');
    END IF;

    -- Grant to every SuperAdmin.
    INSERT INTO "AspNetUserClaims" ("UserId", "ClaimType", "ClaimValue")
    SELECT ur."UserId", 'Reset User Password', 'ResetUserPasswordPolicy'
    FROM "AspNetUserRoles" ur
    JOIN "AspNetRoles" r ON r."Id" = ur."RoleId" AND r."NormalizedName" = 'SUPERADMIN'
    WHERE NOT EXISTS (SELECT 1 FROM "AspNetUserClaims" c
                      WHERE c."UserId" = ur."UserId" AND c."ClaimType" = 'Reset User Password');
END $$;
