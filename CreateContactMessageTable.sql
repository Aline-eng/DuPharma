USE dupharma_db;
GO

-- Create ContactMessages table
CREATE TABLE [ContactMessages] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [FullName] nvarchar(100) NOT NULL,
    [Email] nvarchar(100) NOT NULL,
    [Phone] nvarchar(20) NULL,
    [Subject] nvarchar(200) NOT NULL,
    [Message] nvarchar(2000) NOT NULL,
    [Reply] nvarchar(2000) NULL,
    [IsReplied] bit NOT NULL DEFAULT 0,
    [CreatedAt] datetime2 NOT NULL DEFAULT GETDATE(),
    [RepliedAt] datetime2 NULL,
    [RepliedByUserId] int NULL,
    CONSTRAINT [PK_ContactMessages] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ContactMessages_RepliedByUser] FOREIGN KEY ([RepliedByUserId]) REFERENCES [Users] ([UserId]) ON DELETE SET NULL
);

-- Create indexes for better performance
CREATE INDEX [IX_ContactMessage_CreatedAt] ON [ContactMessages] ([CreatedAt]);
CREATE INDEX [IX_ContactMessage_IsReplied] ON [ContactMessages] ([IsReplied]);
CREATE INDEX [IX_ContactMessage_Email] ON [ContactMessages] ([Email]);

-- Add contact message permissions to existing permissions
INSERT INTO [Permissions] ([Name], [Description], [Category]) VALUES
('ViewContactMessages', 'View contact messages from customers', 'Contact'),
('ReplyContactMessages', 'Reply to customer contact messages', 'Contact'),
('DeleteContactMessages', 'Delete contact messages', 'Contact');

-- Grant contact message permissions to Admin users (Role = 1)
INSERT INTO [UserPermissions] ([UserId], [PermissionId], [GrantedByUserId])
SELECT u.UserId, p.PermissionId, u.UserId
FROM [Users] u
CROSS JOIN [Permissions] p
WHERE u.Role = 1 -- Admin
AND p.Name IN ('ViewContactMessages', 'ReplyContactMessages', 'DeleteContactMessages');

-- Grant view and reply permissions to Manager users (Role = 2)
INSERT INTO [UserPermissions] ([UserId], [PermissionId], [GrantedByUserId])
SELECT u.UserId, p.PermissionId, u.UserId
FROM [Users] u
CROSS JOIN [Permissions] p
WHERE u.Role = 2 -- Manager
AND p.Name IN ('ViewContactMessages', 'ReplyContactMessages');

GO

PRINT 'ContactMessages table created successfully!';
PRINT 'Contact message permissions added and assigned to users.';
PRINT '';
PRINT 'Permission Summary:';
PRINT '- Admin: Can view, reply, and delete contact messages';
PRINT '- Manager: Can view and reply to contact messages';
PRINT '- Pharmacist: No contact message permissions (can be granted individually if needed)';