# DuPharma Project Structure Documentation

## Root Files

### Program.cs
**Purpose**: Application entry point and configuration
- Configures dependency injection for services (AppDbContext, AuthService, DispenseService)
- Sets up Entity Framework Core with SQL Server
- Configures cookie-based authentication
- Registers MVC controllers and Razor views
- Defines middleware pipeline

### appsettings.json
**Purpose**: Application configuration
- Contains database connection string (`DefaultConnection`)
- Logging configuration
- Environment-specific settings

### appsettings.Development.json
**Purpose**: Development environment overrides
- Development-specific logging levels
- Debug configurations

### DuPharma.csproj
**Purpose**: Project file
- Defines .NET 8.0 target framework
- Lists NuGet package dependencies (EF Core, SQL Server, etc.)
- Build configurations

---

## Folders

### Controllers/
**Purpose**: MVC Controllers - Handle HTTP requests and business logic

#### AccountController.cs
- **Login (GET)**: Displays login page
- **Login (POST)**: Authenticates user, creates claims, sets authentication cookie
- **Logout (POST)**: Signs out user and clears authentication
- Uses `AuthService` for authentication

#### ApiController.cs
- **SearchMedicines**: JSON API endpoint for medicine autocomplete
- Returns medicines with available stock and pricing
- Used by sales POS system

#### BatchesController.cs
**Role-based**: Admin, Manager only
- **Index**: Lists batches filtered by user's branch (Admin sees all)
- **Create (GET/POST)**: Add new medicine batches with branch assignment
- **Edit (GET/POST)**: Update batch details (quantity, prices, expiry)
- Automatically assigns batches to user's branch

#### CustomersController.cs
- **Index**: Lists all customers
- **Create (GET/POST)**: Add new customer with contact details
- **Edit (GET/POST)**: Update customer information
- **Delete (GET/POST)**: Remove customer (Admin only)

#### HomeController.cs
- **Index**: Dashboard with branch-filtered metrics
- **GetLowStockMedicines()**: Alerts for medicines below reorder level
- **GetExpiringMedicines()**: Batches expiring within 90 days
- **GetTopSellingMedicines()**: Top 10 medicines by sales (last 30 days)
- **GetTodaySalesCount()**: Today's sales count
- All metrics filtered by branch (Admin sees all branches)

#### MedicinesController.cs
- **Index**: Lists all medicines with total stock
- **Create (GET/POST)**: Add new medicine with generic name, brand, strength, form
- **Edit (GET/POST)**: Update medicine details
- **Batches**: View all batches for a specific medicine

#### ReportsController.cs
- **Index**: Generates sales reports (daily/weekly/monthly)
- **ExportCsv**: Exports report to CSV (Admin only)
- **Role-based periods**:
  - Admin: daily, weekly, monthly
  - Manager: weekly
  - Pharmacist: daily
- Reports filtered by branch (Admin sees all)

#### SalesController.cs
- **Index**: Lists sales filtered by user's branch (Admin sees all)
- **Create (GET/POST)**: POS system for creating sales
  - Filters available medicines by branch
  - Uses `DispenseService` for FEFO allocation
- **Receipt**: Displays sale receipt with items and totals

#### UsersController.cs
**Role-based**: Admin only
- **Index**: Lists all users with branch and role
- **Create (GET/POST)**: Add new user with email, password, role, branch
- **Edit (GET/POST)**: Update user details, change role, reset password, activate/deactivate
- Uses `AuthService` for password hashing

---

### Data/
**Purpose**: Database context and configuration

#### AppDbContext.cs
- Inherits from `DbContext`
- Defines DbSets for all entities (Users, Medicines, Batches, Sales, etc.)
- Configures entity relationships and constraints
- Seeds initial data (branches, admin user, sample data)

---

### Models/
**Purpose**: Data models and entities

#### User.cs
- User entity with authentication properties
- Properties: UserId, FullName, Email, PasswordHash, Phone, BranchId, Role, IsActive
- Navigation: Branch, Sales, Prescriptions, StockMovements, AuditLogs
- Role values: 1=Admin, 2=Manager, 3=Pharmacist

#### Entities.cs
Contains all domain entities:

**Branch**
- BranchId, BranchName, Location
- Collections: Users, Sales, Batches

**Supplier**
- SupplierId, Name, ContactPerson, Phone, Address
- Collections: Batches

**Medicine**
- MedicineId, GenericName, BrandName, Strength, Form, Unit, ReorderLevel
- Collections: Batches, PrescriptionItems

**Batch**
- BatchId, MedicineId, BatchNumber, ExpiryDate, QuantityOnHand, PurchasePrice, SellingPrice, SupplierId, ReceivedDate, BranchId
- Navigation: Medicine, Supplier, Branch
- Collections: SaleItems, StockMovements

**Customer**
- CustomerId, FullName, Phone, Address, NationalId
- Collections: Sales, Prescriptions

**Sale**
- SaleId, InvoiceNumber, SoldByUserId, CustomerId, SaleDate, TotalAmount, PaymentMethod, BranchId
- Navigation: SoldByUser, Customer, Branch
- Collections: SaleItems

**SaleItem**
- SaleItemId, SaleId, BatchId, Quantity, UnitPrice, SubTotal
- Navigation: Sale, Batch

**Prescription**
- PrescriptionId, PrescriptionNo, DoctorName, CustomerId, CreatedByUserId, CreatedAt, Notes
- Navigation: Customer, CreatedByUser
- Collections: PrescriptionItems

**PrescriptionItem**
- PrescriptionItemId, PrescriptionId, MedicineId, Dosage, Quantity, Frequency, Duration
- Navigation: Prescription, Medicine

**StockMovement**
- StockMovementId, BatchId, MovementType (IN/OUT/ADJUSTMENT), Quantity, PerformedByUserId, PerformedAt, Reference
- Navigation: Batch, PerformedByUser

**AuditLog**
- AuditId, UserId, Action, Entity, EntityId, Detail, CreatedAt
- Navigation: User

---

### Services/
**Purpose**: Business logic services

#### AuthService.cs
- **AuthenticateAsync()**: Validates user credentials, returns User object
- **HashPassword()**: Hashes password using SHA256 with salt
- **VerifyPassword()**: Compares hashed passwords
- **GetRoleName()**: Converts role integer to string (Admin/Manager/Pharmacist)

#### DispenseService.cs
**Core sales processing service**
- **DispenseAsync()**: Main method for creating sales
  - Validates stock availability by branch
  - Generates invoice number
  - Creates Sale record with BranchId
  - Allocates stock using FEFO
  - Updates batch quantities
  - Creates stock movements
  - Uses database transaction for atomicity
- **ValidateStockAsync()**: Checks if sufficient stock exists in branch
- **AllocateStockFEFO()**: First Expired, First Out allocation algorithm
  - Selects batches by earliest expiry date
  - Allocates from multiple batches if needed
  - Filters by branch
- **GenerateInvoiceNumberAsync()**: Creates unique invoice numbers (INVyyyyMMddnnnn)

---

### Pages/
**Purpose**: Razor views (UI)

#### Account/
**Login.cshtml**: Login form with email, password, remember me

#### Batches/
- **Index.cshtml**: Table of batches with expiry status, color-coded alerts
- **Create.cshtml**: Form to add new batch (medicine, supplier, quantity, prices, dates)
- **Edit.cshtml**: Form to update batch details

#### Customers/
- **Index.cshtml**: Customer list with edit/delete actions
- **Create.cshtml**: Form to add customer (name, phone, address, national ID)
- **Edit.cshtml**: Form to update customer information

#### Home/
**Index.cshtml**: Dashboard with cards showing:
- Low stock alerts
- Expiring medicines (within 90 days)
- Top selling medicines
- Today's sales count
- Total medicines and customers

#### Medicines/
- **Index.cshtml**: Medicine list with stock levels and actions
- **Create.cshtml**: Form to add medicine (generic name, brand, strength, form, unit, reorder level)
- **Edit.cshtml**: Form to update medicine details
- **Batches.cshtml**: View all batches for specific medicine

#### Reports/
**Index.cshtml**: Sales report with period selector (daily/weekly/monthly), data table, CSV export button

#### Sales/
- **Index.cshtml**: Sales history table with invoice, customer, date, amount
- **Create.cshtml**: POS system with medicine search, quantity input, customer selection, payment method
- **Receipt.cshtml**: Printable sale receipt with items, totals, invoice details

#### Users/
**Admin only**
- **Index.cshtml**: User list with name, email, branch, role, status
- **Create.cshtml**: Form to add user (name, email, password, phone, branch, role)
- **Edit.cshtml**: Form to update user, change password, activate/deactivate

#### Shared/
- **_Layout.cshtml**: Master layout with sidebar navigation, user info header
  - Conditional menu items based on role
  - Logout button
- **_RoleActions.cshtml**: Partial view for role-based action buttons
- **_ValidationScriptsPartial.cshtml**: Client-side validation scripts

#### _ViewImports.cshtml
- Imports namespaces for all views
- Adds tag helpers

#### _ViewStart.cshtml
- Sets default layout for all views

---

### Properties/
**launchSettings.json**: Development server configuration, ports, environment variables

---

### wwwroot/
**Purpose**: Static files (CSS, JavaScript, images)

#### css/site.css
- Custom styles for sidebar, dashboard cards, tables
- Color scheme and layout

#### js/site.js
- Client-side JavaScript for interactivity
- Medicine search autocomplete
- Form validations

#### lib/
Third-party libraries:
- **bootstrap/**: CSS framework for responsive design
- **jquery/**: JavaScript library for DOM manipulation
- **jquery-validation/**: Form validation
- **jquery-validation-unobtrusive/**: ASP.NET Core validation integration

#### Images
- **DupharmaIcon.svg**: Application logo
- **dupharmaLogin.svg**: Login page illustration
- **favicon.ico**: Browser tab icon

---

## Key Architectural Patterns

### Repository-Service Pattern
- Controllers use Services for business logic
- Services use AppDbContext for data access
- Separation of concerns

### Role-Based Access Control
- Admin: Full access to all features and all branches
- Manager: Medicines, batches, reports (weekly), branch-specific data
- Pharmacist: Sales, customers, reports (daily), branch-specific data

### Branch Isolation
- Sales filtered by user's branch
- Batches filtered by user's branch
- Stock allocation within branch only
- Dashboard metrics branch-specific
- Admin sees aggregated data from all branches

### FEFO (First Expired, First Out)
- Automatic stock allocation from batches with earliest expiry
- Prevents medicine expiration
- Implemented in DispenseService

### Transaction Safety
- Sales use database transactions
- Rollback on errors
- Ensures data consistency

### Audit Trail
- StockMovements track all inventory changes
- AuditLogs track critical operations
- Reference to source transaction
