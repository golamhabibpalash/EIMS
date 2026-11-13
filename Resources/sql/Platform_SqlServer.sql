-- SchoolBD_PlatformDB schema (SQL Server). Tenant registry for multi-tenant mode.
-- Generated from SMS.DB/Platform/PlatformDbContext.cs (Database.GenerateCreateScript), made idempotent.
-- Run against the EMPTY platform database (e.g. SchoolBD_PlatformDB), not a school database.
-- Single batch, no GO (shared-hosting consoles reject it). Safe to re-run.
-- Alternative: `SMS_App platform init` creates the same tables.

-- The filtered unique index (one primary database per school) requires these settings.
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;

IF OBJECT_ID(N'dbo.DatabaseServers', N'U') IS NULL
BEGIN
    CREATE TABLE [DatabaseServers] (
        [Id] int NOT NULL IDENTITY,
        [ServerName] nvarchar(100) NOT NULL,
        [Provider] int NOT NULL,
        [Host] nvarchar(253) NOT NULL,
        [Port] int NULL,
        [ConnectionOptions] nvarchar(500) NULL,
        [SecretReference] nvarchar(200) NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_DatabaseServers] PRIMARY KEY ([Id])
    );
END;

IF OBJECT_ID(N'dbo.PlatformAuditLogs', N'U') IS NULL
BEGIN
    CREATE TABLE [PlatformAuditLogs] (
        [Id] bigint NOT NULL IDENTITY,
        [TenantId] int NULL,
        [Action] nvarchar(100) NOT NULL,
        [Details] nvarchar(max) NULL,
        [Actor] nvarchar(200) NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_PlatformAuditLogs] PRIMARY KEY ([Id])
    );
END;

IF OBJECT_ID(N'dbo.PlatformSecrets', N'U') IS NULL
BEGIN
    CREATE TABLE [PlatformSecrets] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(150) NOT NULL,
        [Ciphertext] nvarchar(max) NOT NULL,
        [KeyVersion] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_PlatformSecrets] PRIMARY KEY ([Id])
    );
END;

IF OBJECT_ID(N'dbo.Tenants', N'U') IS NULL
BEGIN
    CREATE TABLE [Tenants] (
        [Id] int NOT NULL IDENTITY,
        [TenantCode] nvarchar(63) NOT NULL,
        [TenantName] nvarchar(200) NOT NULL,
        [DisplayName] nvarchar(200) NULL,
        [Status] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_Tenants] PRIMARY KEY ([Id])
    );
END;

IF OBJECT_ID(N'dbo.TenantDatabaseConnections', N'U') IS NULL
BEGIN
    CREATE TABLE [TenantDatabaseConnections] (
        [Id] int NOT NULL IDENTITY,
        [TenantId] int NOT NULL,
        [DatabaseServerId] int NOT NULL,
        [DatabaseName] nvarchar(128) NOT NULL,
        [SecretReference] nvarchar(200) NOT NULL,
        [IsPrimary] bit NOT NULL,
        [SchemaVersion] nvarchar(50) NULL,
        [Status] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_TenantDatabaseConnections] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_TenantDatabaseConnections_DatabaseServers_DatabaseServerId] FOREIGN KEY ([DatabaseServerId]) REFERENCES [DatabaseServers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_TenantDatabaseConnections_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id]) ON DELETE NO ACTION
    );
END;

IF OBJECT_ID(N'dbo.TenantDomains', N'U') IS NULL
BEGIN
    CREATE TABLE [TenantDomains] (
        [Id] int NOT NULL IDENTITY,
        [TenantId] int NOT NULL,
        [Domain] nvarchar(253) NOT NULL,
        [DomainType] int NOT NULL,
        [IsPrimary] bit NOT NULL,
        [IsVerified] bit NOT NULL,
        [IsActive] bit NOT NULL,
        [VerificationToken] nvarchar(100) NULL,
        [VerifiedAt] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_TenantDomains] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_TenantDomains_ActiveRequiresVerified] CHECK ([IsActive] = 0 OR [IsVerified] = 1),
        CONSTRAINT [FK_TenantDomains_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_DatabaseServers_ServerName' AND object_id = OBJECT_ID(N'dbo.DatabaseServers'))
    CREATE UNIQUE INDEX [IX_DatabaseServers_ServerName] ON [DatabaseServers] ([ServerName]);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_PlatformAuditLogs_TenantId_CreatedAt' AND object_id = OBJECT_ID(N'dbo.PlatformAuditLogs'))
    CREATE INDEX [IX_PlatformAuditLogs_TenantId_CreatedAt] ON [PlatformAuditLogs] ([TenantId], [CreatedAt]);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_PlatformSecrets_Name' AND object_id = OBJECT_ID(N'dbo.PlatformSecrets'))
    CREATE UNIQUE INDEX [IX_PlatformSecrets_Name] ON [PlatformSecrets] ([Name]);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_TenantDatabaseConnections_DatabaseServerId_DatabaseName' AND object_id = OBJECT_ID(N'dbo.TenantDatabaseConnections'))
    CREATE UNIQUE INDEX [IX_TenantDatabaseConnections_DatabaseServerId_DatabaseName] ON [TenantDatabaseConnections] ([DatabaseServerId], [DatabaseName]);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_TenantDatabaseConnections_OnePrimaryPerTenant' AND object_id = OBJECT_ID(N'dbo.TenantDatabaseConnections'))
    CREATE UNIQUE INDEX [IX_TenantDatabaseConnections_OnePrimaryPerTenant] ON [TenantDatabaseConnections] ([TenantId]) WHERE [IsPrimary] = 1;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_TenantDomains_Domain' AND object_id = OBJECT_ID(N'dbo.TenantDomains'))
    CREATE UNIQUE INDEX [IX_TenantDomains_Domain] ON [TenantDomains] ([Domain]);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_TenantDomains_TenantId' AND object_id = OBJECT_ID(N'dbo.TenantDomains'))
    CREATE INDEX [IX_TenantDomains_TenantId] ON [TenantDomains] ([TenantId]);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Tenants_TenantCode' AND object_id = OBJECT_ID(N'dbo.Tenants'))
    CREATE UNIQUE INDEX [IX_Tenants_TenantCode] ON [Tenants] ([TenantCode]);
