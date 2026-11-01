-- ============================================================================
-- Testimonials + Transfer Certificates - SCHEMA (SQL SERVER)
-- Creates the Testimonials and TransferCertificates tables. IDEMPOTENT: guarded
-- by IF NOT EXISTS so it is safe to re-run. Single batch, no GO.
-- (Fresh databases get these tables automatically from the EF model via
--  EnsureCreated; this script is for existing databases.)
-- FK targets the singular [Student] table ([Student].[Id]).
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = N'Testimonials')
BEGIN
    CREATE TABLE [Testimonials] (
        [Id]            INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [StudentId]     INT NULL,
        [StudentName]   NVARCHAR(150) NOT NULL,
        [Designation]   NVARCHAR(150) NULL,
        [Content]       NVARCHAR(MAX) NOT NULL,
        [Photo]         NVARCHAR(MAX) NULL,
        [DisplayOrder]  INT NOT NULL DEFAULT 0,
        [Status]        BIT NOT NULL DEFAULT 1,
        [IsPublished]   BIT NOT NULL DEFAULT 0,
        [CreatedBy]     NVARCHAR(MAX) NULL,
        [CreatedAt]     DATETIME2 NOT NULL,
        [EditedBy]      NVARCHAR(MAX) NULL,
        [EditedAt]      DATETIME2 NOT NULL,
        [MACAddress]    NVARCHAR(MAX) NULL,
        CONSTRAINT [FK_Testimonials_Student] FOREIGN KEY ([StudentId])
            REFERENCES [Student]([Id]) ON DELETE SET NULL
    );
    CREATE INDEX [IX_Testimonials_StudentId] ON [Testimonials]([StudentId]);
END

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = N'TransferCertificates')
BEGIN
    CREATE TABLE [TransferCertificates] (
        [Id]                   INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [StudentId]            INT NOT NULL,
        [CertificateNo]        NVARCHAR(50) NOT NULL,
        [IssueDate]            DATETIME2 NOT NULL,
        [LeavingDate]          DATETIME2 NULL,
        [Reason]               NVARCHAR(500) NULL,
        [Conduct]              NVARCHAR(500) NULL,
        [Remarks]              NVARCHAR(1000) NULL,
        [Status]               INT NOT NULL DEFAULT 0,
        [StudentNameSnapshot]  NVARCHAR(MAX) NULL,
        [UniqueIdSnapshot]     NVARCHAR(MAX) NULL,
        [ClassNameSnapshot]    NVARCHAR(MAX) NULL,
        [ClassRollSnapshot]    INT NOT NULL DEFAULT 0,
        [SessionNameSnapshot]  NVARCHAR(MAX) NULL,
        [AdmissionDateSnapshot] DATETIME2 NULL,
        [CreatedBy]            NVARCHAR(MAX) NULL,
        [CreatedAt]            DATETIME2 NOT NULL,
        [EditedBy]             NVARCHAR(MAX) NULL,
        [EditedAt]             DATETIME2 NOT NULL,
        [MACAddress]           NVARCHAR(MAX) NULL,
        CONSTRAINT [FK_TransferCertificates_Student] FOREIGN KEY ([StudentId])
            REFERENCES [Student]([Id]) ON DELETE NO ACTION
    );
    CREATE UNIQUE INDEX [IX_TransferCertificates_CertificateNo] ON [TransferCertificates]([CertificateNo]);
    CREATE INDEX [IX_TransferCertificates_StudentId] ON [TransferCertificates]([StudentId]);
END
