-- ============================================================================
-- Student: parents' Bangla names + NID images - POSTGRESQL
-- ----------------------------------------------------------------------------
-- Same change as Student_ParentInfo_SqlServer.sql. Run on every PostgreSQL school
-- database (e.g. each SchoolBD tenant: psql -p 5433 -d sbd_<code> -f this.sql)
-- BEFORE deploying 2.0.0. Idempotent (safe to re-run).
-- ============================================================================

ALTER TABLE "Student" ADD COLUMN IF NOT EXISTS "FatherNameBangla" character varying(150) NULL;
ALTER TABLE "Student" ADD COLUMN IF NOT EXISTS "MotherNameBangla" character varying(150) NULL;
ALTER TABLE "Student" ADD COLUMN IF NOT EXISTS "FatherNidImage"   character varying(100) NULL;
ALTER TABLE "Student" ADD COLUMN IF NOT EXISTS "MotherNidImage"   character varying(100) NULL;
