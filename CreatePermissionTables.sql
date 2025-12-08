USE dupharma_db;
GO

-- Create Permissions table
CREATE TABLE [Permissions] (
    [PermissionId] int IDENTITY(1,1) NOT NULL,
    [Name] nvarchar(100) NOT NULL,
    [Description] nvarchar(200) NOT NULL,
    [Category] nvarchar(50) NOT NULL,
    [IsActive] bit NOT NULL DEFAULT 1,
    CONSTRAINT [PK_Permissions] PRIMARY KEY ([PermissionId])
);

-- Create UserPermissions table
CREATE TABLE [UserPermissions] (
    [UserPermissionId] int IDENTITY(1,1) NOT NULL,
    [UserId] int NOT NULL,
    [PermissionId] int NOT NULL,
    [GrantedAt] datetime2 NOT NULL DEFAULT GETDATE(),
    [GrantedByUserId] int NULL,
    CONSTRAINT [PK_UserPermissions] PRIMARY KEY ([UserPermissionId]),
    CONSTRAINT [FK_UserPermissions_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId]) ON DELETE CASCADE,
    CONSTRAINT [FK_UserPermissions_Permissions_PermissionId] FOREIGN KEY ([PermissionId]) REFERENCES [Permissions] ([PermissionId]) ON DELETE CASCADE,
    CONSTRAINT [FK_UserPermissions_GrantedByUsers_GrantedByUserId] FOREIGN KEY ([GrantedByUserId]) REFERENCES [Users] ([UserId]) ON DELETE SET NULL
);

-- Create indexes
CREATE UNIQUE INDEX [IX_Permissions_Name] ON [Permissions] ([Name]);
CREATE INDEX [IX_UserPermissions_UserId] ON [UserPermissions] ([UserId]);
CREATE INDEX [IX_UserPermissions_PermissionId] ON [UserPermissions] ([PermissionId]);
CREATE UNIQUE INDEX [IX_UserPermissions_User_Permission] ON [UserPermissions] ([UserId], [PermissionId]);

-- Insert default permissions
INSERT INTO [Permissions] ([Name], [Description], [Category]) VALUES
-- Dashboard
('ViewDashboard', 'View dashboard and statistics', 'Dashboard'),

-- Medicines
('ViewMedicines', 'View medicines list', 'Medicines'),
('CreateMedicines', 'Create new medicines', 'Medicines'),
('EditMedicines', 'Edit existing medicines', 'Medicines'),
('DeleteMedicines', 'Delete medicines', 'Medicines'),
('ViewMedicineBatches', 'View medicine batches', 'Medicines'),

-- Sales
('ViewSales', 'View sales records', 'Sales'),
('CreateSales', 'Create new sales', 'Sales'),
('ViewAllBranchSales', 'View sales from all branches', 'Sales'),

-- Orders
('ViewOrders', 'View customer orders', 'Orders'),
('ApproveOrders', 'Approve/reject orders', 'Orders'),
('ViewAllBranchOrders', 'View orders from all branches', 'Orders'),

-- Customers
('ViewCustomers', 'View customers list', 'Customers'),
('CreateCustomers', 'Create new customers', 'Customers'),
('EditCustomers', 'Edit customer information', 'Customers'),

-- Users
('ViewUsers', 'View users list', 'Users'),
('CreateUsers', 'Create new users', 'Users'),
('EditUsers', 'Edit user information', 'Users'),
('DeleteUsers', 'Delete users', 'Users'),

-- Reports
('ViewReports', 'View reports', 'Reports'),
('ExportReports', 'Export reports', 'Reports'),

-- Batches
('ViewBatches', 'View medicine batches', 'Batches'),
('CreateBatches', 'Create new batches', 'Batches'),
('EditBatches', 'Edit batch information', 'Batches'),

-- Administration
('ManagePermissions', 'Manage user permissions', 'Administration'),
('ViewAuditLogs', 'View audit logs', 'Administration');

-- Assign permissions to existing users based on their roles

-- Admin permissions (Role = 1)
INSERT INTO [UserPermissions] ([UserId], [PermissionId], [GrantedByUserId])
SELECT u.UserId, p.PermissionId, u.UserId
FROM [Users] u
CROSS JOIN [Permissions] p
WHERE u.Role = 1; -- Admin gets all permissions

-- Manager permissions (Role = 2)
INSERT INTO [UserPermissions] ([UserId], [PermissionId], [GrantedByUserId])
SELECT u.UserId, p.PermissionId, u.UserId
FROM [Users] u
CROSS JOIN [Permissions] p
WHERE u.Role = 2 -- Manager
AND p.Name IN (
    'ViewDashboard',
    'ViewMedicines', 'CreateMedicines', 'EditMedicines', 'ViewMedicineBatches',
    'ViewSales', 'CreateSales',
    'ViewOrders', 'ApproveOrders',
    'ViewCustomers', 'CreateCustomers', 'EditCustomers',
    'ViewReports',
    'ViewBatches', 'CreateBatches', 'EditBatches'
);

-- Pharmacist permissions (Role = 3)
INSERT INTO [UserPermissions] ([UserId], [PermissionId], [GrantedByUserId])
SELECT u.UserId, p.PermissionId, u.UserId
FROM [Users] u
CROSS JOIN [Permissions] p
WHERE u.Role = 3 -- Pharmacist
AND p.Name IN (
    'ViewDashboard',
    'ViewMedicines', 'ViewMedicineBatches',
    'ViewSales', 'CreateSales',
    'ViewOrders',
    'ViewCustomers', 'CreateCustomers',
    'ViewBatches'
);

GO

PRINT 'Permission tables created and default permissions assigned successfully!';
PRINT '';
PRINT 'Permission Summary:';
PRINT '- Admin: All permissions';
PRINT '- Manager: Most permissions except user management and system admin';
PRINT '- Pharmacist: Basic permissions for daily operations';