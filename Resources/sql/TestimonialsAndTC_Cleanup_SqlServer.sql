-- ============================================================================
-- OPTIONAL cleanup (SQL SERVER) - run ONLY if you previously ran the earlier
-- stored-CRUD version (11 claims + the Testimonials/TransferCertificates tables).
-- Removes the 8 now-unused claims and the unused tables. Keeps the 3 live claims
-- (IndexTestimonialsPolicy, IndexTransferCertificatesPolicy, GenerateTransferCertificatesPolicy).
-- Safe/idempotent. Not required for the feature.
-- ============================================================================

DELETE FROM AspNetUserClaims
 WHERE ClaimValue IN (N'CreateTestimonialsPolicy',N'EditTestimonialsPolicy',N'DeleteTestimonialsPolicy',N'PublishTestimonialsPolicy',
                      N'CreateTransferCertificatesPolicy',N'EditTransferCertificatesPolicy',N'DeleteTransferCertificatesPolicy',N'CancelTransferCertificatesPolicy');

DELETE FROM AspNetRoleClaims
 WHERE ClaimValue IN (N'CreateTestimonialsPolicy',N'EditTestimonialsPolicy',N'DeleteTestimonialsPolicy',N'PublishTestimonialsPolicy',
                      N'CreateTransferCertificatesPolicy',N'EditTransferCertificatesPolicy',N'DeleteTransferCertificatesPolicy',N'CancelTransferCertificatesPolicy');

DELETE FROM [ClaimStores]
 WHERE [ClaimValue] IN (N'CreateTestimonialsPolicy',N'EditTestimonialsPolicy',N'DeleteTestimonialsPolicy',N'PublishTestimonialsPolicy',
                        N'CreateTransferCertificatesPolicy',N'EditTransferCertificatesPolicy',N'DeleteTransferCertificatesPolicy',N'CancelTransferCertificatesPolicy');

IF OBJECT_ID(N'[TransferCertificates]', N'U') IS NOT NULL DROP TABLE [TransferCertificates];
IF OBJECT_ID(N'[Testimonials]', N'U') IS NOT NULL DROP TABLE [Testimonials];
