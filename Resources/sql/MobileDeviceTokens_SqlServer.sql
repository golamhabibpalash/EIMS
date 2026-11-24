IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = N'MobileDeviceTokens')
BEGIN
    CREATE TABLE [dbo].[MobileDeviceTokens]
    (
        [Id]          INT           IDENTITY(1,1) NOT NULL,
        [UserId]      NVARCHAR(450) NOT NULL,
        [Token]       NVARCHAR(450) NOT NULL,
        [Platform]    NVARCHAR(20)  NULL,
        [LastSeenAt]  DATETIME2     NOT NULL,
        [IsActive]    BIT           NOT NULL CONSTRAINT [DF_MobileDeviceTokens_IsActive] DEFAULT (1),
        [CreatedBy]   NVARCHAR(MAX) NULL,
        [CreatedAt]   DATETIME2     NOT NULL,
        [EditedBy]    NVARCHAR(MAX) NULL,
        [EditedAt]    DATETIME2     NOT NULL,
        [MACAddress]  NVARCHAR(MAX) NULL,
        CONSTRAINT [PK_MobileDeviceTokens] PRIMARY KEY ([Id])
    );
END

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_MobileDeviceTokens_Token' AND object_id = OBJECT_ID(N'[dbo].[MobileDeviceTokens]'))
BEGIN
    CREATE UNIQUE INDEX [IX_MobileDeviceTokens_Token]
        ON [dbo].[MobileDeviceTokens] ([Token]);
END

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_MobileDeviceTokens_UserId_IsActive' AND object_id = OBJECT_ID(N'[dbo].[MobileDeviceTokens]'))
BEGIN
    CREATE INDEX [IX_MobileDeviceTokens_UserId_IsActive]
        ON [dbo].[MobileDeviceTokens] ([UserId], [IsActive]);
END
