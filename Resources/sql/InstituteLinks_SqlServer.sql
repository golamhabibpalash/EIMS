-- Institute external links (website, social media, other resources).
-- Idempotent: safe to run on every SqlServer instance before deploying the release.
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = N'InstituteLinks')
BEGIN
    CREATE TABLE [dbo].[InstituteLinks]
    (
        [Id]           INT             IDENTITY(1,1) NOT NULL,
        [InstituteId]  INT             NOT NULL,
        [LinkTitle]    NVARCHAR(100)   NOT NULL,
        [LinkType]     INT             NOT NULL CONSTRAINT [DF_InstituteLinks_LinkType] DEFAULT (0),
        [Url]          NVARCHAR(2048)  NOT NULL,
        [DisplayOrder] INT             NOT NULL CONSTRAINT [DF_InstituteLinks_DisplayOrder] DEFAULT (0),
        [IsActive]     BIT             NOT NULL CONSTRAINT [DF_InstituteLinks_IsActive] DEFAULT (1),
        [CreatedBy]    NVARCHAR(MAX)   NULL,
        [CreatedAt]    DATETIME2       NOT NULL,
        [EditedBy]     NVARCHAR(MAX)   NULL,
        [EditedAt]     DATETIME2       NOT NULL,
        [MACAddress]   NVARCHAR(MAX)   NULL,
        CONSTRAINT [PK_InstituteLinks] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_InstituteLinks_Institute_InstituteId] FOREIGN KEY ([InstituteId])
            REFERENCES [dbo].[Institute] ([Id]) ON DELETE CASCADE
    );
END

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_InstituteLinks_InstituteId_DisplayOrder' AND object_id = OBJECT_ID(N'[dbo].[InstituteLinks]'))
BEGIN
    CREATE INDEX [IX_InstituteLinks_InstituteId_DisplayOrder]
        ON [dbo].[InstituteLinks] ([InstituteId], [DisplayOrder]);
END
