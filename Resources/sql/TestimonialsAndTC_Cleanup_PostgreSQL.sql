-- ============================================================================
-- OPTIONAL cleanup (POSTGRESQL) - run ONLY if you previously ran the earlier
-- stored-CRUD version (11 claims + the Testimonials/TransferCertificates tables).
-- The certificates are now on-demand generators, so those tables and 8 of the
-- claims are unused. This removes them. The 3 live claims are kept:
--   IndexTestimonialsPolicy, IndexTransferCertificatesPolicy, GenerateTransferCertificatesPolicy
-- Safe/idempotent. Nothing here is required for the feature to work.
-- ============================================================================

-- 1) Remove the 8 now-unused permissions (and any grants of them).
DELETE FROM "AspNetUserClaims"
 WHERE "ClaimValue" IN ('CreateTestimonialsPolicy','EditTestimonialsPolicy','DeleteTestimonialsPolicy','PublishTestimonialsPolicy',
                        'CreateTransferCertificatesPolicy','EditTransferCertificatesPolicy','DeleteTransferCertificatesPolicy','CancelTransferCertificatesPolicy');

DELETE FROM "AspNetRoleClaims"
 WHERE "ClaimValue" IN ('CreateTestimonialsPolicy','EditTestimonialsPolicy','DeleteTestimonialsPolicy','PublishTestimonialsPolicy',
                        'CreateTransferCertificatesPolicy','EditTransferCertificatesPolicy','DeleteTransferCertificatesPolicy','CancelTransferCertificatesPolicy');

DELETE FROM "ClaimStores"
 WHERE "ClaimValue" IN ('CreateTestimonialsPolicy','EditTestimonialsPolicy','DeleteTestimonialsPolicy','PublishTestimonialsPolicy',
                        'CreateTransferCertificatesPolicy','EditTransferCertificatesPolicy','DeleteTransferCertificatesPolicy','CancelTransferCertificatesPolicy');

-- 2) Drop the unused tables (generators store nothing).
DROP TABLE IF EXISTS "TransferCertificates";
DROP TABLE IF EXISTS "Testimonials";
