-- ============================================================================
-- Online Admission schema - SQL SERVER
-- Creates the 9 Online Admission tables and their indexes. IDEMPOTENT: every object is
-- created only if it does not exist yet, so re-running is safe and never touches data.
-- Why a script and not a migration: startup runs EnsureCreatedAsync() (which never adds tables
-- to an existing database) and the EF migration history is not usable for new migrations.
-- CI never applies schema, so run this once per instance BEFORE deploying the release.
-- Generated from the EF model (ApplicationDbContext.ConfigureOnlineAdmission), so a fresh
-- EnsureCreated database and a scripted one are identical.
-- Afterwards also run OnlineAdmission_Claims_*.sql and grant the claims to roles.
-- Single batch (no GO), so it also runs in shared-hosting SQL consoles.
-- ============================================================================

IF OBJECT_ID(N'[dbo].[AdmissionSettings]', N'U') IS NULL
BEGIN
    CREATE TABLE [AdmissionSettings] (
        [Id] int NOT NULL IDENTITY,
        [HelplinePhone] nvarchar(20) NULL,
        [WhatsAppNumber] nvarchar(20) NULL,
        [SupportHours] nvarchar(200) NULL,
        [SupportMessage] nvarchar(500) NULL,
        [ApplicationNoPrefix] nvarchar(10) NOT NULL,
        [RequireOtpVerification] bit NOT NULL,
        [SendSmsNotifications] bit NOT NULL,
        [MaxUploadSizeKb] int NOT NULL,
        [UserManualNotes] nvarchar(max) NULL,
        [CreatedBy] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [EditedBy] nvarchar(max) NULL,
        [EditedAt] datetime2 NOT NULL,
        [MACAddress] nvarchar(max) NULL,
        CONSTRAINT [PK_AdmissionSettings] PRIMARY KEY ([Id])
    );
END;

IF OBJECT_ID(N'[dbo].[AdmissionGuardianAccounts]', N'U') IS NULL
BEGIN
    CREATE TABLE [AdmissionGuardianAccounts] (
        [Id] int NOT NULL IDENTITY,
        [FullName] nvarchar(150) NOT NULL,
        [MobileNumber] nvarchar(11) NOT NULL,
        [Email] nvarchar(150) NULL,
        [PasswordHash] nvarchar(max) NOT NULL,
        [IsMobileVerified] bit NOT NULL,
        [FailedLoginCount] int NOT NULL,
        [LockoutUntil] datetime2 NULL,
        [LastLoginAt] datetime2 NULL,
        [OtpHash] nvarchar(max) NULL,
        [OtpPurpose] nvarchar(20) NULL,
        [OtpExpiresAt] datetime2 NULL,
        [OtpSentAt] datetime2 NULL,
        [OtpAttempts] int NOT NULL,
        [CreatedBy] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [EditedBy] nvarchar(max) NULL,
        [EditedAt] datetime2 NOT NULL,
        [MACAddress] nvarchar(max) NULL,
        CONSTRAINT [PK_AdmissionGuardianAccounts] PRIMARY KEY ([Id])
    );
END;

IF OBJECT_ID(N'[dbo].[AdmissionSessions]', N'U') IS NULL
BEGIN
    CREATE TABLE [AdmissionSessions] (
        [Id] int NOT NULL IDENTITY,
        [Title] nvarchar(200) NOT NULL,
        [AcademicSessionId] int NOT NULL,
        [Status] int NOT NULL,
        [PublishFrom] datetime2 NULL,
        [NoticeSummary] nvarchar(500) NULL,
        [NoticeDetails] nvarchar(max) NULL,
        [ApplicationStartDate] datetime2 NOT NULL,
        [ApplicationEndDate] datetime2 NOT NULL,
        [ClosingSoonDays] int NOT NULL,
        [ImportantDates] nvarchar(max) NULL,
        [EligibilityContent] nvarchar(max) NULL,
        [PolicyContent] nvarchar(max) NULL,
        [PolicyVersion] nvarchar(30) NULL,
        [PaymentStage] int NOT NULL,
        [AllowOnlinePayment] bit NOT NULL,
        [AllowOfflinePayment] bit NOT NULL,
        [AllowPartialPayment] bit NOT NULL,
        [OfflinePaymentInstructions] nvarchar(max) NULL,
        [OfferValidityDays] int NULL,
        [CreatedBy] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [EditedBy] nvarchar(max) NULL,
        [EditedAt] datetime2 NOT NULL,
        [MACAddress] nvarchar(max) NULL,
        CONSTRAINT [PK_AdmissionSessions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AdmissionSessions_AcademicSession_AcademicSessionId] FOREIGN KEY ([AcademicSessionId]) REFERENCES [AcademicSession] ([Id]) ON DELETE NO ACTION
    );
END;

IF OBJECT_ID(N'[dbo].[AdmissionDocumentRequirements]', N'U') IS NULL
BEGIN
    CREATE TABLE [AdmissionDocumentRequirements] (
        [Id] int NOT NULL IDENTITY,
        [AdmissionSessionId] int NOT NULL,
        [Name] nvarchar(150) NOT NULL,
        [Description] nvarchar(500) NULL,
        [IsMandatory] bit NOT NULL,
        [AllowedExtensions] nvarchar(100) NOT NULL,
        [MaxSizeKb] int NOT NULL,
        [SortOrder] int NOT NULL,
        [CreatedBy] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [EditedBy] nvarchar(max) NULL,
        [EditedAt] datetime2 NOT NULL,
        [MACAddress] nvarchar(max) NULL,
        CONSTRAINT [PK_AdmissionDocumentRequirements] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AdmissionDocumentRequirements_AdmissionSessions_AdmissionSessionId] FOREIGN KEY ([AdmissionSessionId]) REFERENCES [AdmissionSessions] ([Id]) ON DELETE NO ACTION
    );
END;

IF OBJECT_ID(N'[dbo].[AdmissionPrograms]', N'U') IS NULL
BEGIN
    CREATE TABLE [AdmissionPrograms] (
        [Id] int NOT NULL IDENTITY,
        [AdmissionSessionId] int NOT NULL,
        [AcademicClassId] int NOT NULL,
        [SeatCapacity] int NOT NULL,
        [CapacityPolicy] int NOT NULL,
        [MinAgeYears] int NULL,
        [MaxAgeYears] int NULL,
        [AgeCalculationDate] datetime2 NULL,
        [RequirePreviousResult] bit NOT NULL,
        [MinPreviousGpa] decimal(4,2) NULL,
        [EligibilityNotes] nvarchar(max) NULL,
        [AdmissionFeeAmount] decimal(18,2) NOT NULL,
        [AdmissionFeeHeadId] int NULL,
        [IsActive] bit NOT NULL,
        [CreatedBy] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [EditedBy] nvarchar(max) NULL,
        [EditedAt] datetime2 NOT NULL,
        [MACAddress] nvarchar(max) NULL,
        CONSTRAINT [PK_AdmissionPrograms] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AdmissionPrograms_AcademicClass_AcademicClassId] FOREIGN KEY ([AcademicClassId]) REFERENCES [AcademicClass] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_AdmissionPrograms_AdmissionSessions_AdmissionSessionId] FOREIGN KEY ([AdmissionSessionId]) REFERENCES [AdmissionSessions] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_AdmissionPrograms_StudentFeeHead_AdmissionFeeHeadId] FOREIGN KEY ([AdmissionFeeHeadId]) REFERENCES [StudentFeeHead] ([Id]) ON DELETE NO ACTION
    );
END;

IF OBJECT_ID(N'[dbo].[AdmissionApplications]', N'U') IS NULL
BEGIN
    CREATE TABLE [AdmissionApplications] (
        [Id] int NOT NULL IDENTITY,
        [ApplicationNo] nvarchar(30) NULL,
        [AdmissionSessionId] int NOT NULL,
        [AdmissionProgramId] int NOT NULL,
        [GuardianAccountId] int NOT NULL,
        [Status] int NOT NULL,
        [CurrentStep] int NOT NULL,
        [Name] nvarchar(150) NULL,
        [NameBangla] nvarchar(150) NULL,
        [DOB] datetime2 NULL,
        [GenderId] int NULL,
        [ReligionId] int NULL,
        [NationalityId] int NULL,
        [BloodGroupId] int NULL,
        [BirthCertificateNo] nvarchar(17) NULL,
        [PhotoFileName] nvarchar(200) NULL,
        [FatherName] nvarchar(150) NULL,
        [FatherNameBangla] nvarchar(150) NULL,
        [FatherOccupation] nvarchar(100) NULL,
        [FatherPhone] nvarchar(11) NULL,
        [FatherNID] nvarchar(17) NULL,
        [MotherName] nvarchar(150) NULL,
        [MotherNameBangla] nvarchar(150) NULL,
        [MotherOccupation] nvarchar(100) NULL,
        [MotherPhone] nvarchar(11) NULL,
        [MotherNID] nvarchar(17) NULL,
        [GuardianPhone] nvarchar(11) NULL,
        [Email] nvarchar(150) NULL,
        [PresentAddressArea] nvarchar(200) NULL,
        [PresentAddressPO] nvarchar(100) NULL,
        [PresentDivisionId] int NULL,
        [PresentDistrictId] int NULL,
        [PresentUpazilaId] int NULL,
        [PermanentAddressArea] nvarchar(200) NULL,
        [PermanentAddressPO] nvarchar(100) NULL,
        [PermanentDivisionId] int NULL,
        [PermanentDistrictId] int NULL,
        [PermanentUpazilaId] int NULL,
        [PreviousSchool] nvarchar(200) NULL,
        [PreviousClass] nvarchar(50) NULL,
        [PreviousResult] nvarchar(100) NULL,
        [PreviousGpa] decimal(4,2) NULL,
        [SubmittedAt] datetime2 NULL,
        [LastSubmittedAt] datetime2 NULL,
        [EligibilityAcknowledged] bit NOT NULL,
        [PolicyVersionAcknowledged] nvarchar(30) NULL,
        [AcknowledgedAt] datetime2 NULL,
        [EligibilityWarnings] nvarchar(2000) NULL,
        [EligibilityExceptionApproved] bit NOT NULL,
        [EligibilityExceptionReason] nvarchar(1000) NULL,
        [CorrectionReason] nvarchar(2000) NULL,
        [CorrectionDeadline] datetime2 NULL,
        [DecisionReason] nvarchar(2000) NULL,
        [DecisionBy] nvarchar(450) NULL,
        [DecisionAt] datetime2 NULL,
        [OfferExpiresAt] datetime2 NULL,
        [AmountDue] decimal(18,2) NOT NULL,
        [AmountPaid] decimal(18,2) NOT NULL,
        [StudentId] int NULL,
        [EnrolledAt] datetime2 NULL,
        [EnrolledBy] nvarchar(450) NULL,
        [EnrolmentExceptionReason] nvarchar(1000) NULL,
        [CreatedBy] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [EditedBy] nvarchar(max) NULL,
        [EditedAt] datetime2 NOT NULL,
        [MACAddress] nvarchar(max) NULL,
        CONSTRAINT [PK_AdmissionApplications] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AdmissionApplications_AdmissionGuardianAccounts_GuardianAccountId] FOREIGN KEY ([GuardianAccountId]) REFERENCES [AdmissionGuardianAccounts] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_AdmissionApplications_AdmissionPrograms_AdmissionProgramId] FOREIGN KEY ([AdmissionProgramId]) REFERENCES [AdmissionPrograms] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_AdmissionApplications_AdmissionSessions_AdmissionSessionId] FOREIGN KEY ([AdmissionSessionId]) REFERENCES [AdmissionSessions] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_AdmissionApplications_BloodGroup_BloodGroupId] FOREIGN KEY ([BloodGroupId]) REFERENCES [BloodGroup] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_AdmissionApplications_District_PermanentDistrictId] FOREIGN KEY ([PermanentDistrictId]) REFERENCES [District] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_AdmissionApplications_District_PresentDistrictId] FOREIGN KEY ([PresentDistrictId]) REFERENCES [District] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_AdmissionApplications_Division_PermanentDivisionId] FOREIGN KEY ([PermanentDivisionId]) REFERENCES [Division] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_AdmissionApplications_Division_PresentDivisionId] FOREIGN KEY ([PresentDivisionId]) REFERENCES [Division] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_AdmissionApplications_Gender_GenderId] FOREIGN KEY ([GenderId]) REFERENCES [Gender] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_AdmissionApplications_Nationality_NationalityId] FOREIGN KEY ([NationalityId]) REFERENCES [Nationality] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_AdmissionApplications_Religion_ReligionId] FOREIGN KEY ([ReligionId]) REFERENCES [Religion] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_AdmissionApplications_Student_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [Student] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_AdmissionApplications_Upazila_PermanentUpazilaId] FOREIGN KEY ([PermanentUpazilaId]) REFERENCES [Upazila] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_AdmissionApplications_Upazila_PresentUpazilaId] FOREIGN KEY ([PresentUpazilaId]) REFERENCES [Upazila] ([Id]) ON DELETE NO ACTION
    );
END;

IF OBJECT_ID(N'[dbo].[AdmissionApplicationDocuments]', N'U') IS NULL
BEGIN
    CREATE TABLE [AdmissionApplicationDocuments] (
        [Id] int NOT NULL IDENTITY,
        [AdmissionApplicationId] int NOT NULL,
        [AdmissionDocumentRequirementId] int NULL,
        [StoredFileName] nvarchar(200) NOT NULL,
        [OriginalFileName] nvarchar(255) NOT NULL,
        [ContentType] nvarchar(100) NULL,
        [SizeBytes] bigint NOT NULL,
        [VerificationStatus] int NOT NULL,
        [VerificationRemarks] nvarchar(1000) NULL,
        [VerifiedBy] nvarchar(450) NULL,
        [VerifiedAt] datetime2 NULL,
        [CreatedBy] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [EditedBy] nvarchar(max) NULL,
        [EditedAt] datetime2 NOT NULL,
        [MACAddress] nvarchar(max) NULL,
        CONSTRAINT [PK_AdmissionApplicationDocuments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AdmissionApplicationDocuments_AdmissionApplications_AdmissionApplicationId] FOREIGN KEY ([AdmissionApplicationId]) REFERENCES [AdmissionApplications] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_AdmissionApplicationDocuments_AdmissionDocumentRequirements_AdmissionDocumentRequirementId] FOREIGN KEY ([AdmissionDocumentRequirementId]) REFERENCES [AdmissionDocumentRequirements] ([Id]) ON DELETE NO ACTION
    );
END;

IF OBJECT_ID(N'[dbo].[AdmissionApplicationHistories]', N'U') IS NULL
BEGIN
    CREATE TABLE [AdmissionApplicationHistories] (
        [Id] int NOT NULL IDENTITY,
        [AdmissionApplicationId] int NOT NULL,
        [Action] nvarchar(100) NOT NULL,
        [FromStatus] int NULL,
        [ToStatus] int NULL,
        [Note] nvarchar(2000) NULL,
        [Snapshot] nvarchar(max) NULL,
        [IsInternal] bit NOT NULL,
        [ActorType] int NOT NULL,
        [ActorName] nvarchar(200) NULL,
        [CreatedBy] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [EditedBy] nvarchar(max) NULL,
        [EditedAt] datetime2 NOT NULL,
        [MACAddress] nvarchar(max) NULL,
        CONSTRAINT [PK_AdmissionApplicationHistories] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AdmissionApplicationHistories_AdmissionApplications_AdmissionApplicationId] FOREIGN KEY ([AdmissionApplicationId]) REFERENCES [AdmissionApplications] ([Id]) ON DELETE NO ACTION
    );
END;

IF OBJECT_ID(N'[dbo].[AdmissionPayments]', N'U') IS NULL
BEGIN
    CREATE TABLE [AdmissionPayments] (
        [Id] int NOT NULL IDENTITY,
        [AdmissionApplicationId] int NOT NULL,
        [Method] int NOT NULL,
        [Channel] nvarchar(100) NULL,
        [Amount] decimal(18,2) NOT NULL,
        [PaidOn] datetime2 NOT NULL,
        [TransactionReference] nvarchar(100) NULL,
        [ProofFileName] nvarchar(200) NULL,
        [Status] int NOT NULL,
        [ReceiptNo] nvarchar(30) NULL,
        [VerifiedAmount] decimal(18,2) NULL,
        [VerifiedBy] nvarchar(450) NULL,
        [VerifiedAt] datetime2 NULL,
        [ReviewRemarks] nvarchar(1000) NULL,
        [GatewayTransactionId] nvarchar(100) NULL,
        [StudentPaymentId] int NULL,
        [CreatedBy] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [EditedBy] nvarchar(max) NULL,
        [EditedAt] datetime2 NOT NULL,
        [MACAddress] nvarchar(max) NULL,
        CONSTRAINT [PK_AdmissionPayments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AdmissionPayments_AdmissionApplications_AdmissionApplicationId] FOREIGN KEY ([AdmissionApplicationId]) REFERENCES [AdmissionApplications] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AdmissionGuardianAccounts_MobileNumber' AND object_id = OBJECT_ID(N'[dbo].[AdmissionGuardianAccounts]'))
    CREATE UNIQUE INDEX [IX_AdmissionGuardianAccounts_MobileNumber] ON [AdmissionGuardianAccounts] ([MobileNumber]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AdmissionSessions_AcademicSessionId' AND object_id = OBJECT_ID(N'[dbo].[AdmissionSessions]'))
    CREATE INDEX [IX_AdmissionSessions_AcademicSessionId] ON [AdmissionSessions] ([AcademicSessionId]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AdmissionSessions_Status' AND object_id = OBJECT_ID(N'[dbo].[AdmissionSessions]'))
    CREATE INDEX [IX_AdmissionSessions_Status] ON [AdmissionSessions] ([Status]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AdmissionDocumentRequirements_AdmissionSessionId' AND object_id = OBJECT_ID(N'[dbo].[AdmissionDocumentRequirements]'))
    CREATE INDEX [IX_AdmissionDocumentRequirements_AdmissionSessionId] ON [AdmissionDocumentRequirements] ([AdmissionSessionId]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AdmissionPrograms_AcademicClassId' AND object_id = OBJECT_ID(N'[dbo].[AdmissionPrograms]'))
    CREATE INDEX [IX_AdmissionPrograms_AcademicClassId] ON [AdmissionPrograms] ([AcademicClassId]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AdmissionPrograms_AdmissionFeeHeadId' AND object_id = OBJECT_ID(N'[dbo].[AdmissionPrograms]'))
    CREATE INDEX [IX_AdmissionPrograms_AdmissionFeeHeadId] ON [AdmissionPrograms] ([AdmissionFeeHeadId]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AdmissionPrograms_AdmissionSessionId_AcademicClassId' AND object_id = OBJECT_ID(N'[dbo].[AdmissionPrograms]'))
    CREATE UNIQUE INDEX [IX_AdmissionPrograms_AdmissionSessionId_AcademicClassId] ON [AdmissionPrograms] ([AdmissionSessionId], [AcademicClassId]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AdmissionApplications_AdmissionProgramId' AND object_id = OBJECT_ID(N'[dbo].[AdmissionApplications]'))
    CREATE INDEX [IX_AdmissionApplications_AdmissionProgramId] ON [AdmissionApplications] ([AdmissionProgramId]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AdmissionApplications_AdmissionSessionId_Status' AND object_id = OBJECT_ID(N'[dbo].[AdmissionApplications]'))
    CREATE INDEX [IX_AdmissionApplications_AdmissionSessionId_Status] ON [AdmissionApplications] ([AdmissionSessionId], [Status]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AdmissionApplications_ApplicationNo' AND object_id = OBJECT_ID(N'[dbo].[AdmissionApplications]'))
    CREATE UNIQUE INDEX [IX_AdmissionApplications_ApplicationNo] ON [AdmissionApplications] ([ApplicationNo]) WHERE [ApplicationNo] IS NOT NULL;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AdmissionApplications_BirthCertificateNo' AND object_id = OBJECT_ID(N'[dbo].[AdmissionApplications]'))
    CREATE INDEX [IX_AdmissionApplications_BirthCertificateNo] ON [AdmissionApplications] ([BirthCertificateNo]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AdmissionApplications_BloodGroupId' AND object_id = OBJECT_ID(N'[dbo].[AdmissionApplications]'))
    CREATE INDEX [IX_AdmissionApplications_BloodGroupId] ON [AdmissionApplications] ([BloodGroupId]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AdmissionApplications_GenderId' AND object_id = OBJECT_ID(N'[dbo].[AdmissionApplications]'))
    CREATE INDEX [IX_AdmissionApplications_GenderId] ON [AdmissionApplications] ([GenderId]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AdmissionApplications_GuardianAccountId' AND object_id = OBJECT_ID(N'[dbo].[AdmissionApplications]'))
    CREATE INDEX [IX_AdmissionApplications_GuardianAccountId] ON [AdmissionApplications] ([GuardianAccountId]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AdmissionApplications_NationalityId' AND object_id = OBJECT_ID(N'[dbo].[AdmissionApplications]'))
    CREATE INDEX [IX_AdmissionApplications_NationalityId] ON [AdmissionApplications] ([NationalityId]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AdmissionApplications_PermanentDistrictId' AND object_id = OBJECT_ID(N'[dbo].[AdmissionApplications]'))
    CREATE INDEX [IX_AdmissionApplications_PermanentDistrictId] ON [AdmissionApplications] ([PermanentDistrictId]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AdmissionApplications_PermanentDivisionId' AND object_id = OBJECT_ID(N'[dbo].[AdmissionApplications]'))
    CREATE INDEX [IX_AdmissionApplications_PermanentDivisionId] ON [AdmissionApplications] ([PermanentDivisionId]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AdmissionApplications_PermanentUpazilaId' AND object_id = OBJECT_ID(N'[dbo].[AdmissionApplications]'))
    CREATE INDEX [IX_AdmissionApplications_PermanentUpazilaId] ON [AdmissionApplications] ([PermanentUpazilaId]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AdmissionApplications_PresentDistrictId' AND object_id = OBJECT_ID(N'[dbo].[AdmissionApplications]'))
    CREATE INDEX [IX_AdmissionApplications_PresentDistrictId] ON [AdmissionApplications] ([PresentDistrictId]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AdmissionApplications_PresentDivisionId' AND object_id = OBJECT_ID(N'[dbo].[AdmissionApplications]'))
    CREATE INDEX [IX_AdmissionApplications_PresentDivisionId] ON [AdmissionApplications] ([PresentDivisionId]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AdmissionApplications_PresentUpazilaId' AND object_id = OBJECT_ID(N'[dbo].[AdmissionApplications]'))
    CREATE INDEX [IX_AdmissionApplications_PresentUpazilaId] ON [AdmissionApplications] ([PresentUpazilaId]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AdmissionApplications_ReligionId' AND object_id = OBJECT_ID(N'[dbo].[AdmissionApplications]'))
    CREATE INDEX [IX_AdmissionApplications_ReligionId] ON [AdmissionApplications] ([ReligionId]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AdmissionApplications_StudentId' AND object_id = OBJECT_ID(N'[dbo].[AdmissionApplications]'))
    CREATE UNIQUE INDEX [IX_AdmissionApplications_StudentId] ON [AdmissionApplications] ([StudentId]) WHERE [StudentId] IS NOT NULL;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AdmissionApplicationDocuments_AdmissionApplicationId' AND object_id = OBJECT_ID(N'[dbo].[AdmissionApplicationDocuments]'))
    CREATE INDEX [IX_AdmissionApplicationDocuments_AdmissionApplicationId] ON [AdmissionApplicationDocuments] ([AdmissionApplicationId]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AdmissionApplicationDocuments_AdmissionDocumentRequirementId' AND object_id = OBJECT_ID(N'[dbo].[AdmissionApplicationDocuments]'))
    CREATE INDEX [IX_AdmissionApplicationDocuments_AdmissionDocumentRequirementId] ON [AdmissionApplicationDocuments] ([AdmissionDocumentRequirementId]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AdmissionApplicationHistories_AdmissionApplicationId' AND object_id = OBJECT_ID(N'[dbo].[AdmissionApplicationHistories]'))
    CREATE INDEX [IX_AdmissionApplicationHistories_AdmissionApplicationId] ON [AdmissionApplicationHistories] ([AdmissionApplicationId]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AdmissionPayments_AdmissionApplicationId' AND object_id = OBJECT_ID(N'[dbo].[AdmissionPayments]'))
    CREATE INDEX [IX_AdmissionPayments_AdmissionApplicationId] ON [AdmissionPayments] ([AdmissionApplicationId]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AdmissionPayments_GatewayTransactionId' AND object_id = OBJECT_ID(N'[dbo].[AdmissionPayments]'))
    CREATE UNIQUE INDEX [IX_AdmissionPayments_GatewayTransactionId] ON [AdmissionPayments] ([GatewayTransactionId]) WHERE [GatewayTransactionId] IS NOT NULL;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AdmissionPayments_ReceiptNo' AND object_id = OBJECT_ID(N'[dbo].[AdmissionPayments]'))
    CREATE UNIQUE INDEX [IX_AdmissionPayments_ReceiptNo] ON [AdmissionPayments] ([ReceiptNo]) WHERE [ReceiptNo] IS NOT NULL;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AdmissionPayments_Status' AND object_id = OBJECT_ID(N'[dbo].[AdmissionPayments]'))
    CREATE INDEX [IX_AdmissionPayments_Status] ON [AdmissionPayments] ([Status]);
