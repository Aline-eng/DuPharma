USE dupharma_db;
GO

-- Add new columns to Medicines table
ALTER TABLE [Medicines] ADD [Description] nvarchar(500) NULL;
ALTER TABLE [Medicines] ADD [ImageUrl] nvarchar(200) NULL DEFAULT '/images/medicine-default.png';
ALTER TABLE [Medicines] ADD [RequiresPrescription] bit NOT NULL DEFAULT 0;
GO

-- Create Orders table
CREATE TABLE [Orders] (
    [OrderId] int IDENTITY(1,1) NOT NULL,
    [OrderNumber] nvarchar(20) NOT NULL,
    [CustomerId] int NULL,
    [CustomerName] nvarchar(100) NOT NULL,
    [CustomerEmail] nvarchar(100) NOT NULL,
    [CustomerPhone] nvarchar(20) NOT NULL,
    [DeliveryAddress] nvarchar(200) NOT NULL,
    [OrderDate] datetime2 NOT NULL,
    [TotalAmount] decimal(10,2) NOT NULL,
    [Status] nvarchar(20) NOT NULL DEFAULT 'Pending',
    [BranchId] int NOT NULL,
    [ApprovedByUserId] int NULL,
    [Notes] nvarchar(500) NOT NULL DEFAULT '',
    CONSTRAINT [PK_Orders] PRIMARY KEY ([OrderId]),
    CONSTRAINT [FK_Orders_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([CustomerId]) ON DELETE SET NULL,
    CONSTRAINT [FK_Orders_Branches_BranchId] FOREIGN KEY ([BranchId]) REFERENCES [Branches] ([BranchId]),
    CONSTRAINT [FK_Orders_Users_ApprovedByUserId] FOREIGN KEY ([ApprovedByUserId]) REFERENCES [Users] ([UserId]) ON DELETE SET NULL
);
GO

-- Create OrderItems table
CREATE TABLE [OrderItems] (
    [OrderItemId] int IDENTITY(1,1) NOT NULL,
    [OrderId] int NOT NULL,
    [MedicineId] int NOT NULL,
    [Quantity] int NOT NULL,
    [UnitPrice] decimal(10,2) NOT NULL,
    [SubTotal] decimal(10,2) NOT NULL,
    [PrescriptionImageUrl] nvarchar(200) NOT NULL DEFAULT '',
    CONSTRAINT [PK_OrderItems] PRIMARY KEY ([OrderItemId]),
    CONSTRAINT [FK_OrderItems_Orders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [Orders] ([OrderId]) ON DELETE CASCADE,
    CONSTRAINT [FK_OrderItems_Medicines_MedicineId] FOREIGN KEY ([MedicineId]) REFERENCES [Medicines] ([MedicineId])
);
GO

-- Create indexes
CREATE INDEX [IX_Orders_OrderDate] ON [Orders] ([OrderDate]);
CREATE INDEX [IX_Orders_Status] ON [Orders] ([Status]);
GO

-- Update existing medicines with sample data
UPDATE [Medicines] SET 
    [Description] = 'Effective pain relief and fever reducer',
    [RequiresPrescription] = 0
WHERE [GenericName] = 'Paracetamol';

UPDATE [Medicines] SET 
    [Description] = 'Anti-inflammatory pain reliever',
    [RequiresPrescription] = 0
WHERE [GenericName] = 'Ibuprofen';

UPDATE [Medicines] SET 
    [Description] = 'Antibiotic for bacterial infections',
    [RequiresPrescription] = 1
WHERE [GenericName] = 'Amoxicillin';

UPDATE [Medicines] SET 
    [Description] = 'Proton pump inhibitor for acid reflux',
    [RequiresPrescription] = 1
WHERE [GenericName] = 'Omeprazole';

UPDATE [Medicines] SET 
    [Description] = 'Blood thinner and pain reliever',
    [RequiresPrescription] = 0
WHERE [GenericName] = 'Aspirin';

UPDATE [Medicines] SET 
    [Description] = 'Diabetes medication for blood sugar control',
    [RequiresPrescription] = 1
WHERE [GenericName] = 'Metformin';

UPDATE [Medicines] SET 
    [Description] = 'ACE inhibitor for high blood pressure',
    [RequiresPrescription] = 1
WHERE [GenericName] = 'Lisinopril';

UPDATE [Medicines] SET 
    [Description] = 'Statin for cholesterol management',
    [RequiresPrescription] = 1
WHERE [GenericName] = 'Atorvastatin';
GO

PRINT 'Shop features added successfully!';
