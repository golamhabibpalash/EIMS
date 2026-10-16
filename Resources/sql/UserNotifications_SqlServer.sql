-- ============================================================================
-- UserNotifications table (in-app notification centre) - SQL SERVER
-- ----------------------------------------------------------------------------
-- Why a script and not an EF migration:
--   * At runtime the app calls EnsureCreatedAsync(), not Migrate(), so an EXISTING
--     database never gains new tables automatically - they must be created by hand.
--   * The solution's migration differ is in a pre-existing broken state (the history
--     was squashed), so "dotnet ef migrations add" cannot scaffold at the moment.
-- Run this once against each SQL Server instance's database before deploying the
-- notification feature. It is idempotent (safe to run more than once).
--
-- Column types mirror what EF Core's model would create for the UserNotification
-- entity (CommonProps + the entity's own properties) so a fresh EnsureCreated DB and
-- a hand-provisioned one stay identical.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = N'UserNotifications')
BEGIN
    CREATE TABLE [dbo].[UserNotifications]
    (
        [Id]              INT            IDENTITY(1,1) NOT NULL,
        [RecipientUserId] NVARCHAR(450)  NOT NULL,
        [Title]           NVARCHAR(200)  NOT NULL,
        [Message]         NVARCHAR(1000) NOT NULL,
        [Url]             NVARCHAR(500)  NULL,
        [Category]        INT            NOT NULL CONSTRAINT [DF_UserNotifications_Category] DEFAULT (0),
        [IsRead]          BIT            NOT NULL CONSTRAINT [DF_UserNotifications_IsRead]   DEFAULT (0),
        [ReadAt]          DATETIME2      NULL,
        [CreatedBy]       NVARCHAR(MAX)  NULL,
        [CreatedAt]       DATETIME2      NOT NULL,
        [EditedBy]        NVARCHAR(MAX)  NULL,
        [EditedAt]        DATETIME2      NOT NULL,
        [MACAddress]      NVARCHAR(MAX)  NULL,
        CONSTRAINT [PK_UserNotifications] PRIMARY KEY ([Id])
    );

    -- Supports the badge's unread-count query and the per-user list.
    CREATE INDEX [IX_UserNotifications_RecipientUserId_IsRead]
        ON [dbo].[UserNotifications] ([RecipientUserId], [IsRead]);
END
GO
