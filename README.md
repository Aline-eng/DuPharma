# DuPharma - Pharmacy Management System

A comprehensive ASP.NET Core 8 MVC pharmacy management system with role-based access control, inventory management, and sales tracking.

## Features

- **Role-based Authentication**: Admin, Manager and Pharmacist roles.
- **Medicine Management**: CRUD operations with batch tracking.
- **Sales Management**: POS system with FEFO allocation.
- **Inventory Tracking**: Real-time stock levels and expiry alerts.
- **Dashboard**: Low stock alerts, expiring medicines, top-selling items.
- **Reports**: Sales reports and analytics.

## Prerequisites

- .NET 8.0 SDK
- SQL Server or SQL Server LocalDB
- Visual Studio 2022 or VS Code

## Setup Instructions

### 1. Clone and Restore Packages

```bash
git clone <repository-url>
cd DuPharma
dotnet restore
```

### 2. Database Setup Using SQL Server

Update the connection string in `appsettings.json`:
```json
"DefaultConnection": "Data Source=.;Initial Catalog=dupharma_db;Integrated Security=True;TrustServerCertificate=True"
```

### 3. Create Database and Run Migrations

```bash
# Restore packages
dotnet restore

# Build the project
dotnet build

# Create and apply migrations (EF tools are included in project)
dotnet ef migrations add Initial
dotnet ef database update
```

**Note**: The database `dupharma_db` will be automatically created in SQL Server LocalDB. The migration will create all necessary tables and seed initial data including:
- Admin, Manager and Pharmacist roles
- Default admin user (admin@dupharma.local / ChangeMe123!)
- Sample branch, suppliers, medicines and batches
- Sample customers for testing

### 4. Run the Application

```bash
dotnet run
```

The application will be available at `https://localhost:5001` or `http://localhost:5000`

## Default Login Credentials

- **Email**: admin@dupharma.local
- **Password**: ChangeMe123!
- **Role**: Admin

## Database Schema

The system automatically creates the `dupharma_db` database with the following tables:

### Core Tables
- `Users` (Extended with FullName, BranchId) - User accounts with Identity
- `Roles` - Identity role management
- `Branches` - Pharmacy branch information
- `Suppliers` - Medicine suppliers with contact details
- `Medicines` - Medicine master data (generic name, brand, strength, form)
- `Batches` - Medicine batches with expiry dates, pricing, and stock levels
- `Customers` - Customer information and contact details

### Transaction Tables
- `Sales` - Sale transactions with invoice numbers and totals
- `SaleItems` - Individual sale line items with batch allocation
- `Prescriptions` - Customer prescriptions from doctors
- `PrescriptionItems` - Prescription line items with dosage information
- `StockMovements` - Inventory movement tracking (IN/OUT/ADJUSTMENT)
- `AuditLogs` - System audit trail for critical operations

### Indexes and Performance
- Optimized indexes on ExpiryDate, MedicineId, SaleDate for fast queries
- Foreign key relationships with appropriate cascade behaviors
- Decimal precision for monetary values (10,2)

## User Roles and Permissions

### Admin
- Full system access
- User management
- All CRUD operations on medicines
- Generate and export reports
- System configuration

### Manager
- Add/update medicines
- Approve returns
- Generate reports
- Monitor stock and expiry dates
- Oversee pharmacist activities

### Pharmacist
- Record sales transactions
- View customer prescription history
- View alerts (low stock, expiry)
- Create daily activity reports

## Key Features

### Dashboard
- Real-time statistics
- Low stock alerts
- Medicines expiring in 90 days
- Top-selling medicines (last 30 days)

### Sales (POS) System
- Medicine search with autocomplete
- FEFO (First Expired, First Out) allocation
- Multiple payment methods
- Receipt generation
- Customer association

### Inventory Management
- Batch-level tracking
- Expiry date monitoring
- Automatic stock updates
- Reorder level alerts

### Business Logic
- **FEFO Allocation**: Automatically allocates stock from batches with earliest expiry dates
- **Transaction Safety**: All sales operations use database transactions
- **Stock Validation**: Prevents overselling with real-time stock checks
- **Audit Trail**: Tracks all critical operations

## API Endpoints

### Medicine Search API
```
GET /api/medicines?q=search_term
```
Returns JSON array of medicines matching the search term with current stock and pricing.

## Development Notes

### Architecture
- **Repository-Service Pattern**: Clean separation of concerns
- **Entity Framework Core**: Code-first approach with migrations
- **ASP.NET Identity**: Role-based authentication and authorization
- **MVC Pattern**: Controllers, Views, and Models

### Key Services
- `DispenseService`: Handles sales transactions with FEFO logic
- `SeedData`: Initializes roles, admin user, and sample data

### Security
- Role-based authorization on controllers and actions
- Password requirements enforced
- Secure authentication with ASP.NET Identity

## Testing

Run the application and test with the default admin credentials:
1. Login with admin@dupharma.local / ChangeMe123!
2. Navigate to Medicines to view sample data
3. Create a new sale to test the POS system
4. Check the dashboard for alerts and statistics

## Troubleshooting

### Database Connection Issues
- Ensure SQL Server/LocalDB is running
- Verify connection string in appsettings.json
- Check if database exists: `dotnet ef database update`

## DataBase Code
```sql
-- Create Database
CREATE DATABASE dupharma_db;
GO

USE dupharma_db;
GO

-- Create Tables
CREATE TABLE [Branches] (
    [BranchId] int IDENTITY(1,1) NOT NULL,
    [BranchName] nvarchar(100) NOT NULL,
    [Location] nvarchar(200) NOT NULL,
    CONSTRAINT [PK_Branches] PRIMARY KEY ([BranchId])
);

CREATE TABLE [Users] (
    [UserId] int IDENTITY(1,1) NOT NULL,
    [FullName] nvarchar(100) NOT NULL,
    [Email] nvarchar(100) NOT NULL,
    [PasswordHash] nvarchar(255) NOT NULL,
    [Phone] nvarchar(20) NULL,
    [BranchId] int NULL,
    [Role] int NOT NULL DEFAULT 3, -- 1=Admin, 2=Manager, 3=Pharmacist
    [IsActive] bit NOT NULL DEFAULT 1,
    [CreatedAt] datetime2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT [PK_Users] PRIMARY KEY ([UserId]),
    CONSTRAINT [FK_Users_Branches_BranchId] FOREIGN KEY ([BranchId]) REFERENCES [Branches] ([BranchId]) ON DELETE SET NULL
);

CREATE TABLE [Suppliers] (
    [SupplierId] int IDENTITY(1,1) NOT NULL,
    [Name] nvarchar(100) NOT NULL,
    [ContactPerson] nvarchar(100) NOT NULL,
    [Phone] nvarchar(20) NOT NULL,
    [Address] nvarchar(200) NOT NULL,
    CONSTRAINT [PK_Suppliers] PRIMARY KEY ([SupplierId])
);

CREATE TABLE [Medicines] (
    [MedicineId] int IDENTITY(1,1) NOT NULL,
    [GenericName] nvarchar(100) NOT NULL,
    [BrandName] nvarchar(100) NOT NULL,
    [Strength] nvarchar(50) NOT NULL,
    [Form] nvarchar(50) NOT NULL,
    [Unit] nvarchar(20) NOT NULL,
    [ReorderLevel] int NOT NULL,
    CONSTRAINT [PK_Medicines] PRIMARY KEY ([MedicineId])
);

CREATE TABLE [Batches] (
    [BatchId] int IDENTITY(1,1) NOT NULL,
    [MedicineId] int NOT NULL,
    [BatchNumber] nvarchar(50) NOT NULL,
    [ExpiryDate] datetime2 NOT NULL,
    [QuantityOnHand] int NOT NULL,
    [PurchasePrice] decimal(10,2) NOT NULL,
    [SellingPrice] decimal(10,2) NOT NULL,
    [SupplierId] int NOT NULL,
    [ReceivedDate] datetime2 NOT NULL,
    CONSTRAINT [PK_Batches] PRIMARY KEY ([BatchId]),
    CONSTRAINT [FK_Batches_Medicines_MedicineId] FOREIGN KEY ([MedicineId]) REFERENCES [Medicines] ([MedicineId]) ON DELETE CASCADE,
    CONSTRAINT [FK_Batches_Suppliers_SupplierId] FOREIGN KEY ([SupplierId]) REFERENCES [Suppliers] ([SupplierId])
);

CREATE TABLE [Customers] (
    [CustomerId] int IDENTITY(1,1) NOT NULL,
    [FullName] nvarchar(100) NOT NULL,
    [Phone] nvarchar(20) NOT NULL,
    [Address] nvarchar(200) NOT NULL,
    [NationalId] nvarchar(20) NOT NULL,
    CONSTRAINT [PK_Customers] PRIMARY KEY ([CustomerId])
);

CREATE TABLE [Sales] (
    [SaleId] int IDENTITY(1,1) NOT NULL,
    [InvoiceNumber] nvarchar(20) NOT NULL,
    [SoldByUserId] int NOT NULL,
    [CustomerId] int NULL,
    [SaleDate] datetime2 NOT NULL,
    [TotalAmount] decimal(10,2) NOT NULL,
    [PaymentMethod] nvarchar(20) NOT NULL,
    CONSTRAINT [PK_Sales] PRIMARY KEY ([SaleId]),
    CONSTRAINT [FK_Sales_Users_SoldByUserId] FOREIGN KEY ([SoldByUserId]) REFERENCES [Users] ([UserId]),
    CONSTRAINT [FK_Sales_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([CustomerId]) ON DELETE SET NULL
);

CREATE TABLE [SaleItems] (
    [SaleItemId] int IDENTITY(1,1) NOT NULL,
    [SaleId] int NOT NULL,
    [BatchId] int NOT NULL,
    [Quantity] int NOT NULL,
    [UnitPrice] decimal(10,2) NOT NULL,
    [SubTotal] decimal(10,2) NOT NULL,
    CONSTRAINT [PK_SaleItems] PRIMARY KEY ([SaleItemId]),
    CONSTRAINT [FK_SaleItems_Batches_BatchId] FOREIGN KEY ([BatchId]) REFERENCES [Batches] ([BatchId]),
    CONSTRAINT [FK_SaleItems_Sales_SaleId] FOREIGN KEY ([SaleId]) REFERENCES [Sales] ([SaleId]) ON DELETE CASCADE
);

CREATE TABLE [Prescriptions] (
    [PrescriptionId] int IDENTITY(1,1) NOT NULL,
    [PrescriptionNo] nvarchar(20) NOT NULL,
    [DoctorName] nvarchar(100) NOT NULL,
    [CustomerId] int NOT NULL,
    [CreatedByUserId] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [Notes] nvarchar(500) NOT NULL,
    CONSTRAINT [PK_Prescriptions] PRIMARY KEY ([PrescriptionId]),
    CONSTRAINT [FK_Prescriptions_Users_CreatedByUserId] FOREIGN KEY ([CreatedByUserId]) REFERENCES [Users] ([UserId]),
    CONSTRAINT [FK_Prescriptions_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([CustomerId]) ON DELETE CASCADE
);

CREATE TABLE [PrescriptionItems] (
    [PrescriptionItemId] int IDENTITY(1,1) NOT NULL,
    [PrescriptionId] int NOT NULL,
    [MedicineId] int NOT NULL,
    [Dosage] nvarchar(100) NOT NULL,
    [Quantity] int NOT NULL,
    [Frequency] nvarchar(100) NOT NULL,
    [Duration] nvarchar(100) NOT NULL,
    CONSTRAINT [PK_PrescriptionItems] PRIMARY KEY ([PrescriptionItemId]),
    CONSTRAINT [FK_PrescriptionItems_Medicines_MedicineId] FOREIGN KEY ([MedicineId]) REFERENCES [Medicines] ([MedicineId]),
    CONSTRAINT [FK_PrescriptionItems_Prescriptions_PrescriptionId] FOREIGN KEY ([PrescriptionId]) REFERENCES [Prescriptions] ([PrescriptionId]) ON DELETE CASCADE
);

CREATE TABLE [StockMovements] (
    [StockMovementId] int IDENTITY(1,1) NOT NULL,
    [BatchId] int NOT NULL,
    [MovementType] nvarchar(20) NOT NULL,
    [Quantity] int NOT NULL,
    [PerformedByUserId] int NOT NULL,
    [PerformedAt] datetime2 NOT NULL,
    [Reference] nvarchar(100) NOT NULL,
    CONSTRAINT [PK_StockMovements] PRIMARY KEY ([StockMovementId]),
    CONSTRAINT [FK_StockMovements_Users_PerformedByUserId] FOREIGN KEY ([PerformedByUserId]) REFERENCES [Users] ([UserId]),
    CONSTRAINT [FK_StockMovements_Batches_BatchId] FOREIGN KEY ([BatchId]) REFERENCES [Batches] ([BatchId]) ON DELETE CASCADE
);

CREATE TABLE [AuditLogs] (
    [AuditId] int IDENTITY(1,1) NOT NULL,
    [UserId] int NOT NULL,
    [Action] nvarchar(50) NOT NULL,
    [Entity] nvarchar(50) NOT NULL,
    [EntityId] int NOT NULL,
    [Detail] nvarchar(500) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_AuditLogs] PRIMARY KEY ([AuditId]),
    CONSTRAINT [FK_AuditLogs_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId])
);

-- Create Indexes
CREATE INDEX [IX_Batch_ExpiryDate] ON [Batches] ([ExpiryDate]);
CREATE INDEX [IX_Batch_MedicineId] ON [Batches] ([MedicineId]);
CREATE INDEX [IX_Sale_SaleDate] ON [Sales] ([SaleDate]);
CREATE UNIQUE INDEX [IX_Users_Email] ON [Users] ([Email]);

-- Insert Sample Data
-- Insert Branches
INSERT INTO [Branches] ([BranchName], [Location]) VALUES
('Main Branch', 'Downtown'),
('North Branch', 'North District'),
('South Branch', 'South District');

-- Insert Users (Password: ChangeMe123! for all)
INSERT INTO [Users] ([FullName], [Email], [PasswordHash], [Phone], [BranchId], [Role]) VALUES
-- Admin
('System Administrator', 'admin@dupharma.local', 'AQAAAAIAAYagAAAAEHqOZ8vQpQqTVvK+9X8VQxHQzJQzJQzJQzJQzJQzJQzJQzJQzJQzJQzJQzJQzJQzJQ==', '555-0001', 1, 1),
-- Managers
('John Manager', 'john.manager@dupharma.local', 'AQAAAAIAAYagAAAAEHqOZ8vQpQqTVvK+9X8VQxHQzJQzJQzJQzJQzJQzJQzJQzJQzJQzJQzJQzJQzJQzJQ==', '555-0002', 1, 2),
('Sarah Manager', 'sarah.manager@dupharma.local', 'AQAAAAIAAYagAAAAEHqOZ8vQpQqTVvK+9X8VQxHQzJQzJQzJQzJQzJQzJQzJQzJQzJQzJQzJQzJQzJQzJQ==', '555-0003', 2, 2),
-- Pharmacists
('Mike Pharmacist', 'mike.pharmacist@dupharma.local', 'AQAAAAIAAYagAAAAEHqOZ8vQpQqTVvK+9X8VQxHQzJQzJQzJQzJQzJQzJQzJQzJQzJQzJQzJQzJQzJQzJQ==', '555-0004', 1, 3),
('Lisa Pharmacist', 'lisa.pharmacist@dupharma.local', 'AQAAAAIAAYagAAAAEHqOZ8vQpQqTVvK+9X8VQxHQzJQzJQzJQzJQzJQzJQzJQzJQzJQzJQzJQzJQzJQzJQ==', '555-0005', 1, 3),
('David Pharmacist', 'david.pharmacist@dupharma.local', 'AQAAAAIAAYagAAAAEHqOZ8vQpQqTVvK+9X8VQxHQzJQzJQzJQzJQzJQzJQzJQzJQzJQzJQzJQzJQzJQzJQ==', '555-0006', 2, 3),
('Emma Pharmacist', 'emma.pharmacist@dupharma.local', 'AQAAAAIAAYagAAAAEHqOZ8vQpQqTVvK+9X8VQxHQzJQzJQzJQzJQzJQzJQzJQzJQzJQzJQzJQzJQzJQzJQ==', '555-0007', 2, 3);

-- Insert Suppliers
INSERT INTO [Suppliers] ([Name], [ContactPerson], [Phone], [Address]) VALUES
('PharmaCorp Ltd', 'John Smith', '123-456-7890', '123 Medical St'),
('MediSupply Inc', 'Jane Doe', '098-765-4321', '456 Health Ave'),
('HealthCare Distributors', 'Bob Johnson', '555-1234', '789 Wellness Blvd');

-- Insert Medicines
INSERT INTO [Medicines] ([GenericName], [BrandName], [Strength], [Form], [Unit], [ReorderLevel]) VALUES
('Paracetamol', 'Panadol', '500mg', 'Tablet', 'Piece', 100),
('Ibuprofen', 'Advil', '400mg', 'Tablet', 'Piece', 50),
('Amoxicillin', 'Amoxil', '250mg', 'Capsule', 'Piece', 75),
('Omeprazole', 'Prilosec', '20mg', 'Capsule', 'Piece', 30),
('Aspirin', 'Bayer', '100mg', 'Tablet', 'Piece', 80),
('Metformin', 'Glucophage', '500mg', 'Tablet', 'Piece', 60),
('Lisinopril', 'Prinivil', '10mg', 'Tablet', 'Piece', 40),
('Atorvastatin', 'Lipitor', '20mg', 'Tablet', 'Piece', 35);

-- Insert Batches (including expiring medicines)
INSERT INTO [Batches] ([MedicineId], [BatchNumber], [ExpiryDate], [QuantityOnHand], [PurchasePrice], [SellingPrice], [SupplierId], [ReceivedDate]) VALUES
-- Normal stock
(1, 'B2024001001', '2027-12-31', 500, 10.00, 15.00, 1, '2025-01-01'),
(2, 'B2024002001', '2027-11-30', 300, 12.00, 18.00, 1, '2025-01-01'),
(3, 'B2024003001', '2027-10-31', 200, 25.00, 35.00, 2, '2025-01-01'),
(4, 'B2024004001', '2027-09-30', 150, 30.00, 45.00, 2, '2025-01-01'),
-- Expiring soon (within 90 days)
(1, 'B2024001002', '2025-12-28', 80, 10.00, 15.00, 1, '2024-06-01'),
(2, 'B2024002002', '2025-12-15', 45, 12.00, 18.00, 1, '2024-07-01'),
(5, 'B2024005001', '2025-12-31', 120, 8.00, 12.00, 3, '2024-08-01'),
(6, 'B2024006001', '2025-12-15', 90, 15.00, 22.00, 2, '2024-09-01'),
-- Low stock items
(7, 'B2024007001', '2027-06-30', 25, 20.00, 30.00, 3, '2024-02-01'),
(8, 'B2024008001', '2027-05-31', 15, 35.00, 50.00, 2, '2024-03-01'),
-- Additional normal stock
(5, 'B2024005002', '2027-08-31', 200, 8.00, 12.00, 3, '2025-01-15'),
(6, 'B2024006002', '2027-07-31', 180, 15.00, 22.00, 2, '2025-01-20'),
(7, 'B2024007002', '2027-06-30', 160, 20.00, 30.00, 3, '2025-02-01'),
(8, 'B2024008002', '2027-05-31', 140, 35.00, 50.00, 2, '2025-02-15');

-- Insert Customers
INSERT INTO [Customers] ([FullName], [Phone], [Address], [NationalId]) VALUES
('Alice Johnson', '555-0101', '789 Oak St', 'ID001'),
('Bob Wilson', '555-0102', '321 Pine St', 'ID002'),
('Carol Davis', '555-0103', '654 Elm St', 'ID003'),
('Daniel Brown', '555-0104', '987 Maple Ave', 'ID004'),
('Eva Martinez', '555-0105', '246 Cedar Rd', 'ID005'),
('Frank Taylor', '555-0106', '135 Birch Ln', 'ID006');

-- Insert Sample Sales
INSERT INTO [Sales] ([InvoiceNumber], [SoldByUserId], [CustomerId], [SaleDate], [TotalAmount], [PaymentMethod]) VALUES
('INV20241201001', 4, 1, '2024-12-01 10:30:00', 45.00, 'Cash'),
('INV20241201002', 5, 2, '2024-12-01 14:15:00', 72.00, 'Card'),
('INV20241201003', 6, NULL, '2024-12-01 16:45:00', 30.00, 'Cash');

-- Insert Sample Sale Items
INSERT INTO [SaleItems] ([SaleId], [BatchId], [Quantity], [UnitPrice], [SubTotal]) VALUES
(1, 1, 2, 15.00, 30.00),
(1, 2, 1, 18.00, 18.00),
(2, 3, 2, 35.00, 70.00),
(3, 4, 1, 45.00, 45.00);

GO

PRINT 'Database dupharma_db created successfully with all tables and sample data!';
```

### Package Restore Issues
```bash
dotnet clean
dotnet restore
dotnet build
```

