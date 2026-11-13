BEGIN TRANSACTION;
GO

CREATE TABLE [Tickets] (
    [Id] int NOT NULL IDENTITY,
    [Title] nvarchar(200) NOT NULL,
    [Description] nvarchar(max) NOT NULL,
    [RaiseDate] datetime2 NOT NULL,
    [Status] int NOT NULL,
    [TicketType] int NOT NULL,
    [RaisedByUserId] nvarchar(450) NULL,
    [AssignedToUserId] nvarchar(450) NULL,
    [ClosedAt] datetime2 NULL,
    [CreatedBy] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [EditedBy] nvarchar(max) NULL,
    [EditedAt] datetime2 NOT NULL,
    [MACAddress] nvarchar(max) NULL,
    CONSTRAINT [PK_Tickets] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [TicketAttachments] (
    [Id] int NOT NULL IDENTITY,
    [TicketId] int NOT NULL,
    [StoredFileName] nvarchar(260) NOT NULL,
    [OriginalFileName] nvarchar(260) NOT NULL,
    [ContentType] nvarchar(150) NULL,
    [FileSize] bigint NOT NULL,
    [CreatedBy] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [EditedBy] nvarchar(max) NULL,
    [EditedAt] datetime2 NOT NULL,
    [MACAddress] nvarchar(max) NULL,
    CONSTRAINT [PK_TicketAttachments] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_TicketAttachments_Tickets_TicketId] FOREIGN KEY ([TicketId]) REFERENCES [Tickets] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [TicketComments] (
    [Id] int NOT NULL IDENTITY,
    [TicketId] int NOT NULL,
    [Message] nvarchar(max) NOT NULL,
    [IsSystemNote] bit NOT NULL,
    [CreatedBy] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [EditedBy] nvarchar(max) NULL,
    [EditedAt] datetime2 NOT NULL,
    [MACAddress] nvarchar(max) NULL,
    CONSTRAINT [PK_TicketComments] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_TicketComments_Tickets_TicketId] FOREIGN KEY ([TicketId]) REFERENCES [Tickets] ([Id]) ON DELETE NO ACTION
);
GO

CREATE INDEX [IX_TicketAttachments_TicketId] ON [TicketAttachments] ([TicketId]);
GO

CREATE INDEX [IX_TicketComments_TicketId] ON [TicketComments] ([TicketId]);
GO

INSERT INTO [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260826160955_AddTicketModule', N'7.0.11');
GO

COMMIT;
GO

