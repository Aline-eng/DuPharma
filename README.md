# DuPharma - Pharmacy Management System

A comprehensive ASP.NET Core 8 MVC pharmacy management system with role-based access control, inventory management, and sales tracking.

## Features

- **Role-based Authentication**: Admin, Manager, Pharmacist roles
- **Medicine Management**: CRUD operations with batch tracking
- **Sales Management**: POS system with FEFO allocation
- **Inventory Tracking**: Real-time stock levels and expiry alerts
- **Dashboard**: Low stock alerts, expiring medicines, top-selling items
- **Reports**: Sales reports and analytics

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

### 2. Database Setup

#### Option A: Using SQL Server LocalDB (Recommended for Development)
The connection string is already configured for LocalDB in `appsettings.json`:
```json
"DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=dupharma_db;Trusted_Connection=true;MultipleActiveResultSets=true"
```

#### Option B: Using SQL Server
Update the connection string in `appsettings.json`:
```json
"DefaultConnection": "Server=YOUR_SERVER;Database=dupharma_db;Trusted_Connection=true;MultipleActiveResultSets=true"
```

Or with SQL Authentication:
```json
"DefaultConnection": "Server=YOUR_SERVER;Database=dupharma_db;User Id=YOUR_USERNAME;Password=YOUR_PASSWORD;MultipleActiveResultSets=true"
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
- Admin, Manager, and Pharmacist roles
- Default admin user (admin@smartpharmacy.local / ChangeMe123!)
- Sample branch, suppliers, medicines, and batches
- Sample customers for testing

### 4. Run the Application

```bash
dotnet run
```

The application will be available at `https://localhost:5001` or `http://localhost:5000`

## Default Login Credentials

- **Email**: admin@smartpharmacy.local
- **Password**: ChangeMe123!
- **Role**: Admin

## Database Schema

The system automatically creates the `dupharma_db` database with the following tables:

### Core Tables
- `AspNetUsers` (Extended with FullName, BranchId) - User accounts with Identity
- `AspNetRoles`, `AspNetUserRoles` - Identity role management
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
1. Login with admin@smartpharmacy.local / ChangeMe123!
2. Navigate to Medicines to view sample data
3. Create a new sale to test the POS system
4. Check the dashboard for alerts and statistics

## Troubleshooting

### Database Connection Issues
- Ensure SQL Server/LocalDB is running
- Verify connection string in appsettings.json
- Check if database exists: `dotnet ef database update`

### Migration Issues
```bash
# Remove existing migrations and recreate
dotnet ef migrations remove
dotnet ef migrations add Initial
dotnet ef database update
```

### Package Restore Issues
```bash
dotnet clean
dotnet restore
dotnet build
```

## Production Deployment

1. Update connection string for production database
2. Set environment to Production
3. Configure HTTPS certificates
4. Update password policies as needed
5. Configure logging and monitoring

## License

This project is for educational and commercial use.