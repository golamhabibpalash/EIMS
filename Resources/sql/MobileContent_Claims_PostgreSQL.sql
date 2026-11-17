-- ============================================================================
-- Mobile Notice + Mobile Slider permissions - POSTGRESQL
-- Adds 8 permissions (view/create/edit/delete for notices and for the slider) used by
-- Communications > SMS Panel > Notice / Student Slider (MobileContentController), so they
-- can be given to users in Administrations > User Profile, and grants all of them to the
-- SuperAdmin and Admin roles. Run MobileContent_PostgreSQL.sql first (it creates the tables).
-- IDEMPOTENT: safe to re-run.
-- Run per school database, e.g. psql -p 5433 -d sbd_<code> -f this.sql
-- Users must LOG OUT and back in (or wait up to 15 minutes) for new permissions to apply.
-- ============================================================================
DO $$
DECLARE
    module_id integer;
    sub_id integer;
BEGIN
    IF NOT EXISTS (SELECT 1 FROM "ProjectModules" WHERE "ModuleName" = 'Communication') THEN
        INSERT INTO "ProjectModules" ("ModuleName", "Status", "Remarks", "CreatedBy", "CreatedAt", "EditedBy", "EditedAt", "MACAddress")
        VALUES ('Communication', true, 'Communications', 'SiteMapSeed', now() AT TIME ZONE 'utc', 'SiteMapSeed', now() AT TIME ZONE 'utc', '');
    END IF;
    SELECT "Id" INTO module_id FROM "ProjectModules" WHERE "ModuleName" = 'Communication';

    IF NOT EXISTS (SELECT 1 FROM "ProjectSubModules" WHERE "SubModuleName" = 'Messaging' AND "ProjectModuleId" = module_id) THEN
        INSERT INTO "ProjectSubModules" ("SubModuleName", "ProjectModuleId", "Status", "Remarks", "CreatedBy", "CreatedAt", "EditedBy", "EditedAt", "MACAddress")
        VALUES ('Messaging', module_id, true, 'SMS Panel', 'SiteMapSeed', now() AT TIME ZONE 'utc', 'SiteMapSeed', now() AT TIME ZONE 'utc', '');
    END IF;
    SELECT "Id" INTO sub_id FROM "ProjectSubModules" WHERE "SubModuleName" = 'Messaging' AND "ProjectModuleId" = module_id;

    DROP TABLE IF EXISTS pg_temp.perm;
    CREATE TEMP TABLE perm (claim_type text, claim_value text) ON COMMIT DROP;
    INSERT INTO perm VALUES
        ('View Mobile Notices', 'IndexMobileNoticesPolicy'),
        ('Create Mobile Notice', 'CreateMobileNoticesPolicy'),
        ('Edit Mobile Notice', 'EditMobileNoticesPolicy'),
        ('Delete Mobile Notice', 'DeleteMobileNoticesPolicy'),
        ('View Mobile Slider', 'IndexMobileSlidersPolicy'),
        ('Create Mobile Slider', 'CreateMobileSlidersPolicy'),
        ('Edit Mobile Slider', 'EditMobileSlidersPolicy'),
        ('Delete Mobile Slider', 'DeleteMobileSlidersPolicy');

    INSERT INTO "ClaimStores" ("ClaimType", "ClaimValue", "SubModuleId", "CreatedBy", "CreatedAt", "EditedBy", "EditedAt", "MACAddress")
    SELECT p.claim_type, p.claim_value, sub_id, 'SiteMapSeed', now() AT TIME ZONE 'utc', 'SiteMapSeed', now() AT TIME ZONE 'utc', ''
    FROM perm p
    WHERE NOT EXISTS (SELECT 1 FROM "ClaimStores" c WHERE c."ClaimValue" = p.claim_value AND c."ClaimType" = p.claim_type);

    -- Grant all of them to the SuperAdmin and Admin ROLES (role claims), so every current and
    -- future SuperAdmin/Admin has them. Other users get them one by one in User Profile.
    INSERT INTO "AspNetRoleClaims" ("RoleId", "ClaimType", "ClaimValue")
    SELECT r."Id", p.claim_type, p.claim_value
    FROM "AspNetRoles" r
    CROSS JOIN perm p
    WHERE r."NormalizedName" IN ('SUPERADMIN', 'ADMIN')
      AND NOT EXISTS (SELECT 1 FROM "AspNetRoleClaims" c
                      WHERE c."RoleId" = r."Id" AND c."ClaimType" = p.claim_type AND c."ClaimValue" = p.claim_value);
END $$;
