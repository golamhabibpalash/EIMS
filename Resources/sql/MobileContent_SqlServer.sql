IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = N'MobileNotices')
BEGIN
    CREATE TABLE [dbo].[MobileNotices]
    (
        [Id]            INT           IDENTITY(1,1) NOT NULL,
        [Title]         NVARCHAR(200) NOT NULL,
        [Message]       NVARCHAR(2000) NULL,
        [Url]           NVARCHAR(500) NULL,
        [IsPublished]   BIT           NOT NULL CONSTRAINT [DF_MobileNotices_IsPublished] DEFAULT (0),
        [PublishedFrom] DATETIME2     NULL,
        [PublishedTo]   DATETIME2     NULL,
        [DisplayOrder]  INT           NOT NULL CONSTRAINT [DF_MobileNotices_DisplayOrder] DEFAULT (0),
        [CreatedBy]     NVARCHAR(MAX) NULL,
        [CreatedAt]     DATETIME2     NOT NULL,
        [EditedBy]      NVARCHAR(MAX) NULL,
        [EditedAt]      DATETIME2     NOT NULL,
        [MACAddress]    NVARCHAR(MAX) NULL,
        CONSTRAINT [PK_MobileNotices] PRIMARY KEY ([Id])
    );

    CREATE INDEX [IX_MobileNotices_Published]
        ON [dbo].[MobileNotices] ([IsPublished], [DisplayOrder], [CreatedAt]);
END

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = N'MobileSliders')
BEGIN
    CREATE TABLE [dbo].[MobileSliders]
    (
        [Id]            INT           IDENTITY(1,1) NOT NULL,
        [Title]         NVARCHAR(200) NOT NULL,
        [Subtitle]      NVARCHAR(300) NULL,
        [ImagePath]     NVARCHAR(500) NULL,
        [TargetUrl]     NVARCHAR(500) NULL,
        [IsPublished]   BIT           NOT NULL CONSTRAINT [DF_MobileSliders_IsPublished] DEFAULT (0),
        [PublishedFrom] DATETIME2     NULL,
        [PublishedTo]   DATETIME2     NULL,
        [DisplayOrder]  INT           NOT NULL CONSTRAINT [DF_MobileSliders_DisplayOrder] DEFAULT (0),
        [CreatedBy]     NVARCHAR(MAX) NULL,
        [CreatedAt]     DATETIME2     NOT NULL,
        [EditedBy]      NVARCHAR(MAX) NULL,
        [EditedAt]      DATETIME2     NOT NULL,
        [MACAddress]    NVARCHAR(MAX) NULL,
        CONSTRAINT [PK_MobileSliders] PRIMARY KEY ([Id])
    );

    CREATE INDEX [IX_MobileSliders_Published]
        ON [dbo].[MobileSliders] ([IsPublished], [DisplayOrder], [CreatedAt]);
END
