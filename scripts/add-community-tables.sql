-- AddCommunityTables Migration SQL
-- Run this in Visual Studio Package Manager Console or SQL Server Object Explorer
-- This creates CommunityPosts, CommunityComments, CommunityLikes tables

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'CommunityPosts')
BEGIN
    CREATE TABLE [CommunityPosts] (
        [Id] int NOT NULL IDENTITY,
        [UserId] nvarchar(450) NOT NULL,
        [Title] nvarchar(200) NOT NULL,
        [Content] nvarchar(5000) NOT NULL,
        [Category] int NOT NULL,
        [FeedbackWanted] bit NOT NULL,
        [CreatedDate] datetime2 NOT NULL,
        [ModifiedDate] datetime2 NULL,
        CONSTRAINT [PK_CommunityPosts] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CommunityPosts_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers]([Id]) ON DELETE CASCADE
    );

    CREATE INDEX [IX_CommunityPosts_UserId] ON [CommunityPosts] ([UserId]);
    CREATE INDEX [IX_CommunityPosts_Category] ON [CommunityPosts] ([Category]);
    CREATE INDEX [IX_CommunityPosts_CreatedDate] ON [CommunityPosts] ([CreatedDate]);
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'CommunityLikes')
BEGIN
    CREATE TABLE [CommunityLikes] (
        [Id] int NOT NULL IDENTITY,
        [PostId] int NOT NULL,
        [UserId] nvarchar(450) NOT NULL,
        [CreatedDate] datetime2 NOT NULL,
        CONSTRAINT [PK_CommunityLikes] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CommunityLikes_CommunityPosts_PostId] FOREIGN KEY ([PostId]) REFERENCES [CommunityPosts]([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_CommunityLikes_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers]([Id]) ON DELETE CASCADE
    );

    CREATE INDEX [IX_CommunityLikes_PostId] ON [CommunityLikes] ([PostId]);
    CREATE INDEX [IX_CommunityLikes_UserId] ON [CommunityLikes] ([UserId]);
    CREATE UNIQUE INDEX [IX_CommunityLikes_UserId_PostId] ON [CommunityLikes] ([UserId], [PostId]);
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'CommunityComments')
BEGIN
    CREATE TABLE [CommunityComments] (
        [Id] int NOT NULL IDENTITY,
        [PostId] int NOT NULL,
        [UserId] nvarchar(450) NOT NULL,
        [Content] nvarchar(2000) NOT NULL,
        [ParentCommentId] int NULL,
        [CreatedDate] datetime2 NOT NULL,
        CONSTRAINT [PK_CommunityComments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CommunityComments_CommunityPosts_PostId] FOREIGN KEY ([PostId]) REFERENCES [CommunityPosts]([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_CommunityComments_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers]([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_CommunityComments_CommunityComments_ParentCommentId] FOREIGN KEY ([ParentCommentId]) REFERENCES [CommunityComments]([Id]) ON DELETE RESTRICT
    );

    CREATE INDEX [IX_CommunityComments_PostId] ON [CommunityComments] ([PostId]);
    CREATE INDEX [IX_CommunityComments_UserId] ON [CommunityComments] ([UserId]);
    CREATE INDEX [IX_CommunityComments_ParentCommentId] ON [CommunityComments] ([ParentCommentId]);
END

-- Record the migration in EF history table
IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20260821122844_AddCommunityTables')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES ('20260821122844_AddCommunityTables', '8.0.29');
END

PRINT 'Community tables created successfully!';
