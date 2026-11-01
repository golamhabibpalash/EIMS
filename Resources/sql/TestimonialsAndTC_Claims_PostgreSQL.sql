-- ============================================================================
-- Testimonials + Transfer Certificates - PERMISSIONS (POSTGRESQL)
-- Same content as the SQL Server script. IDEMPOTENT. Seeds ClaimStores under
-- StudentManagement > StudentDocuments and optionally grants all permissions to
-- one account (edit target_email). Log out/in afterwards.
-- ============================================================================
DO $$
DECLARE
    module_id integer;
    sub_id integer;
    target_email text := 'admin@eims.com';   -- <<< change to the account that needs access
    user_id text;
    claims text[][] := ARRAY[
        ['View Testimonials','IndexTestimonialsPolicy'],
        ['Create Testimonial','CreateTestimonialsPolicy'],
        ['Edit Testimonial','EditTestimonialsPolicy'],
        ['Delete Testimonial','DeleteTestimonialsPolicy'],
        ['Publish Testimonial','PublishTestimonialsPolicy'],
        ['View Transfer Certificates','IndexTransferCertificatesPolicy'],
        ['Create Transfer Certificate','CreateTransferCertificatesPolicy'],
        ['Edit Transfer Certificate','EditTransferCertificatesPolicy'],
        ['Delete Transfer Certificate','DeleteTransferCertificatesPolicy'],
        ['Cancel Transfer Certificate','CancelTransferCertificatesPolicy'],
        ['Generate Transfer Certificate','GenerateTransferCertificatesPolicy']
    ];
    i integer;
    ctype text;
    cvalue text;
BEGIN
    IF NOT EXISTS (SELECT 1 FROM "ProjectModules" WHERE "ModuleName" = 'StudentManagement') THEN
        INSERT INTO "ProjectModules" ("ModuleName", "Status", "Remarks", "CreatedBy", "CreatedAt", "EditedBy", "EditedAt", "MACAddress")
        VALUES ('StudentManagement', true, 'Students', 'SiteMapSeed', now() AT TIME ZONE 'utc', 'SiteMapSeed', now() AT TIME ZONE 'utc', '');
    END IF;
    SELECT "Id" INTO module_id FROM "ProjectModules" WHERE "ModuleName" = 'StudentManagement';

    IF NOT EXISTS (SELECT 1 FROM "ProjectSubModules" WHERE "SubModuleName" = 'StudentDocuments' AND "ProjectModuleId" = module_id) THEN
        INSERT INTO "ProjectSubModules" ("SubModuleName", "ProjectModuleId", "Status", "Remarks", "CreatedBy", "CreatedAt", "EditedBy", "EditedAt", "MACAddress")
        VALUES ('StudentDocuments', module_id, true, 'Certificates & Testimonials', 'SiteMapSeed', now() AT TIME ZONE 'utc', 'SiteMapSeed', now() AT TIME ZONE 'utc', '');
    END IF;
    SELECT "Id" INTO sub_id FROM "ProjectSubModules" WHERE "SubModuleName" = 'StudentDocuments' AND "ProjectModuleId" = module_id;

    FOR i IN 1 .. array_length(claims, 1) LOOP
        ctype := claims[i][1];
        cvalue := claims[i][2];
        IF NOT EXISTS (SELECT 1 FROM "ClaimStores" WHERE "ClaimValue" = cvalue AND "ClaimType" = ctype) THEN
            INSERT INTO "ClaimStores" ("ClaimType", "ClaimValue", "SubModuleId", "CreatedBy", "CreatedAt", "EditedBy", "EditedAt", "MACAddress")
            VALUES (ctype, cvalue, sub_id, 'SiteMapSeed', now() AT TIME ZONE 'utc', 'SiteMapSeed', now() AT TIME ZONE 'utc', '');
        END IF;
    END LOOP;

    SELECT "Id" INTO user_id FROM "AspNetUsers" WHERE "Email" = target_email;
    IF user_id IS NOT NULL THEN
        INSERT INTO "AspNetUserClaims" ("UserId", "ClaimType", "ClaimValue")
        SELECT user_id, cs."ClaimType", cs."ClaimValue"
        FROM "ClaimStores" cs
        WHERE cs."SubModuleId" = sub_id
          AND NOT EXISTS (SELECT 1 FROM "AspNetUserClaims" uc WHERE uc."UserId" = user_id AND uc."ClaimType" = cs."ClaimType");
    END IF;
END $$;
