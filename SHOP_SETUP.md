# Shop Feature Setup Guide

## Overview
The shop feature allows customers to browse medicines, add them to cart, and place orders online. Staff can then review and approve orders.

## Setup Steps

### 1. Run Database Migration

Execute the SQL script to add shop tables and columns:

```bash
# Using SQL Server Management Studio or command line
sqlcmd -S . -d dupharma_db -i AddShopFeatures.sql
```

Or manually run the SQL from `AddShopFeatures.sql` file and `UpdateMedecineImages.sql` file.

### 2. Add Medicine Images(optional)

Place medicine images in `wwwroot/images/` folder. Update medicine records with image URLs:

```sql
UPDATE Medicines SET ImageUrl = '/images/paracetamol.jpg' WHERE MedicineId = 1;
```

Default image path is `/images/medicine-default.png`

### 3. Access the Shop

- **Customer Shop**: Navigate to `https://localhost:5143/Shop`
- **Staff Orders Management**: Login and go to Orders menu

## Features

### Customer Features
- Browse all available medicines
- Search medicines by name
- View detailed medicine information
- Add medicines to cart
- Upload prescription for prescription-required medicines
- Place orders with delivery information
- Receive order confirmation number

### Staff Features
- View all customer orders
- Review prescription uploads
- Approve or reject orders
- Mark orders as completed
- Track order status

## Order Workflow

1. **Customer** browses and adds medicines to cart
2. **Customer** uploads prescription (if required)
3. **Customer** provides delivery details and places order
4. **Staff** reviews order and prescription
5. **Staff** approves or rejects order
6. **Staff** marks order as completed after fulfillment

## Database Tables

### Orders
- OrderId, OrderNumber, CustomerName, CustomerEmail, CustomerPhone
- DeliveryAddress, OrderDate, TotalAmount, Status, BranchId
- Status: Pending, Approved, Rejected, Completed

### OrderItems
- OrderItemId, OrderId, MedicineId, Quantity, UnitPrice, SubTotal
- PrescriptionImageUrl (for prescription uploads)

### Medicines (New Columns)
- Description: Medicine description for customers
- ImageUrl: Path to medicine image
- RequiresPrescription: Boolean flag

## File Upload

Prescriptions are uploaded to `wwwroot/prescriptions/` folder with unique GUID filenames.

## Testing

1. Visit `/Shop` without logging in
2. Add medicines to cart (mix prescription and non-prescription)
3. Upload prescription for required medicines
4. Complete checkout with customer details
5. Login as staff and approve order from Orders page

## Notes

- Shop is publicly accessible (no login required)
- Orders management requires staff authentication
- Cart data is stored in browser localStorage
- Prescription images are stored on server
- Stock levels are checked from Batches table
- Prices are taken from lowest-priced available batch
