-- ============================================================================
-- Student Certificates (Testimonial + Transfer Certificate) - SCHEMA (SQL SERVER)
-- One table holds both certificate types as stored records with a student snapshot.
-- Drops the earlier per-type tables (only if present AND empty) and creates StudentCertificates.
-- IDEMPOTENT. Single batch, no GO. FK -> singular [Student]([Id]).
-- (Fresh DBs get this table from the EF model automatically; this is for existing DBs.)
-- ============================================================================

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;

-- The earlier per-type tables are dropped ONLY when empty, so certificates already
-- issued on an instance that ran the older version are never lost.
IF OBJECT_ID(N'[TransferCertificates]', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.partitions
                   WHERE object_id = OBJECT_ID(N'[TransferCertificates]') AND index_id IN (0, 1) AND rows > 0)
    DROP TABLE [TransferCertificates];
IF OBJECT_ID(N'[Testimonials]', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.partitions
                   WHERE object_id = OBJECT_ID(N'[Testimonials]') AND index_id IN (0, 1) AND rows > 0)
    DROP TABLE [Testimonials];

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = N'StudentCertificates')
BEGIN
    CREATE TABLE [StudentCertificates] (
        [Id]                   INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [Type]                 INT NOT NULL,
        [StudentId]            INT NOT NULL,
        [IssueDate]            DATETIME2 NOT NULL,
        [DeliveryDate]         DATETIME2 NULL,
        [LeavingDate]          DATETIME2 NULL,
        [Reason]               NVARCHAR(500) NULL,
        [ExamName]             NVARCHAR(100) NULL,
        [TotalMarks]           NVARCHAR(50) NULL,
        [Gpa]                  NVARCHAR(50) NULL,
        [Grade]                NVARCHAR(50) NULL,
        [MeritRank]            NVARCHAR(50) NULL,
        [SignatoryName]        NVARCHAR(150) NULL,
        [SignatoryDesignation] NVARCHAR(100) NULL,
        [StudentNameSnapshot]  NVARCHAR(MAX) NULL,
        [FatherNameSnapshot]   NVARCHAR(MAX) NULL,
        [MotherNameSnapshot]   NVARCHAR(MAX) NULL,
        [DobSnapshot]          DATETIME2 NOT NULL,
        [ClassNameSnapshot]    NVARCHAR(MAX) NULL,
        [SectionNameSnapshot]  NVARCHAR(MAX) NULL,
        [ClassRollSnapshot]    INT NOT NULL DEFAULT 0,
        [SessionNameSnapshot]  NVARCHAR(MAX) NULL,
        [UniqueIdSnapshot]     NVARCHAR(MAX) NULL,
        [GenderNameSnapshot]   NVARCHAR(MAX) NULL,
        [CreatedBy]            NVARCHAR(MAX) NULL,
        [CreatedAt]            DATETIME2 NOT NULL,
        [EditedBy]             NVARCHAR(MAX) NULL,
        [EditedAt]             DATETIME2 NOT NULL,
        [MACAddress]           NVARCHAR(MAX) NULL,
        CONSTRAINT [FK_StudentCertificates_Student] FOREIGN KEY ([StudentId])
            REFERENCES [Student]([Id]) ON DELETE NO ACTION
    );
    CREATE INDEX [IX_StudentCertificates_StudentId] ON [StudentCertificates]([StudentId]);
    CREATE INDEX [IX_StudentCertificates_Type] ON [StudentCertificates]([Type]);
END
