# DuPharma API Documentation

Run The Application : 
--start /min dotnet run

Base URL: `https://localhost:5143/api` or `http://localhost:5143/api`

Swagger UI: `https://localhost:5143/swagger`

## Authentication
Currently, the API endpoints are open. Add `[Authorize]` attributes to controllers for authentication.

---

## Medicines API

### GET /api/MedicinesApi
Get all medicines with stock information.

**Query Parameters:**
- `search` (optional): Search by generic or brand name
- `branchId` (optional): Filter by branch

**Response:**
```json
[
  {
    "medicineId": 1,
    "genericName": "Paracetamol",
    "brandName": "Panadol",
    "strength": "500mg",
    "form": "Tablet",
    "unit": "Piece",
    "reorderLevel": 100,
    "totalStock": 580,
    "lowestPrice": 15.00
  }
]
```

### GET /api/MedicinesApi/{id}
Get medicine details with available batches.

**Query Parameters:**
- `branchId` (optional): Filter batches by branch

### GET /api/MedicinesApi/search?q={query}
Search medicines for POS system.

**Query Parameters:**
- `q`: Search term
- `branchId` (optional): Filter by branch

### GET /api/MedicinesApi/low-stock
Get medicines below reorder level.

**Query Parameters:**
- `branchId` (optional): Filter by branch

---

## Batches API

### GET /api/BatchesApi
Get all batches.

**Query Parameters:**
- `medicineId` (optional): Filter by medicine
- `branchId` (optional): Filter by branch

**Response:**
```json
[
  {
    "batchId": 1,
    "batchNumber": "B2024001001",
    "expiryDate": "2027-12-31",
    "quantityOnHand": 500,
    "purchasePrice": 10.00,
    "sellingPrice": 15.00,
    "receivedDate": "2025-01-01",
    "branchId": 1,
    "medicine": {
      "medicineId": 1,
      "genericName": "Paracetamol",
      "brandName": "Panadol",
      "strength": "500mg",
      "form": "Tablet"
    },
    "supplier": {
      "supplierId": 1,
      "name": "PharmaCorp Ltd"
    }
  }
]
```

### GET /api/BatchesApi/{id}
Get batch details.

### GET /api/BatchesApi/expiring
Get batches expiring soon.

**Query Parameters:**
- `days` (default: 90): Days until expiry
- `branchId` (optional): Filter by branch

---

## Sales API

### GET /api/SalesApi
Get all sales.

**Query Parameters:**
- `startDate` (optional): Filter from date
- `endDate` (optional): Filter to date
- `branchId` (optional): Filter by branch

**Response:**
```json
[
  {
    "saleId": 1,
    "invoiceNumber": "INV20241201001",
    "saleDate": "2024-12-01T10:30:00",
    "totalAmount": 45.00,
    "paymentMethod": "Cash",
    "branchId": 1,
    "soldBy": "Mike Pharmacist",
    "customer": "Alice Johnson",
    "itemCount": 2
  }
]
```

### GET /api/SalesApi/{id}
Get sale details with items.

### POST /api/SalesApi
Create a new sale.

**Request Body:**
```json
{
  "userId": 4,
  "customerId": 1,
  "paymentMethod": "Cash",
  "items": [
    {
      "medicineId": 1,
      "quantity": 2
    },
    {
      "medicineId": 2,
      "quantity": 1
    }
  ]
}
```

**Response:**
```json
{
  "saleId": 123,
  "invoiceNumber": "INV20250101001",
  "totalAmount": 48.00
}
```

### GET /api/SalesApi/summary
Get sales summary statistics.

**Query Parameters:**
- `startDate` (optional): Filter from date
- `endDate` (optional): Filter to date
- `branchId` (optional): Filter by branch

**Response:**
```json
{
  "totalSales": 150,
  "totalRevenue": 12500.00,
  "averageOrderValue": 83.33,
  "cashSales": 90,
  "cardSales": 60
}
```

---

## Customers API

### GET /api/CustomersApi
Get all customers.

**Query Parameters:**
- `search` (optional): Search by name, phone, or national ID

**Response:**
```json
[
  {
    "customerId": 1,
    "fullName": "Alice Johnson",
    "phone": "555-0101",
    "address": "789 Oak St",
    "nationalId": "ID001",
    "totalPurchases": 5,
    "totalSpent": 250.00
  }
]
```

### GET /api/CustomersApi/{id}
Get customer details with recent sales.

### GET /api/CustomersApi/{id}/prescriptions
Get customer prescriptions.

---

## Dashboard API

### GET /api/DashboardApi/stats
Get dashboard statistics.

**Query Parameters:**
- `branchId` (optional): Filter by branch

**Response:**
```json
{
  "todaySales": 1250.00,
  "todayTransactions": 15,
  "monthlyRevenue": 35000.00,
  "totalCustomers": 120,
  "lowStockCount": 5,
  "expiringCount": 8
}
```

### GET /api/DashboardApi/top-selling
Get top-selling medicines.

**Query Parameters:**
- `days` (default: 30): Period in days
- `limit` (default: 10): Number of results
- `branchId` (optional): Filter by branch

**Response:**
```json
[
  {
    "medicineId": 1,
    "genericName": "Paracetamol",
    "brandName": "Panadol",
    "strength": "500mg",
    "totalQuantitySold": 450,
    "totalRevenue": 6750.00,
    "transactionCount": 85
  }
]
```

### GET /api/DashboardApi/sales-trend
Get sales trend over time.

**Query Parameters:**
- `days` (default: 7): Number of days
- `branchId` (optional): Filter by branch

**Response:**
```json
[
  {
    "date": "2025-01-01",
    "totalSales": 1250.00,
    "transactionCount": 15
  }
]
```

---

## Error Responses

All endpoints return standard HTTP status codes:

- `200 OK`: Success
- `201 Created`: Resource created
- `400 Bad Request`: Invalid request
- `404 Not Found`: Resource not found
- `500 Internal Server Error`: Server error

**Error Response Format:**
```json
{
  "error": "Error message description"
}
```

---

## Testing with Swagger

1. Run the application: `dotnet run`
2. Navigate to: `https://localhost:5001/swagger`
3. Test endpoints directly from the browser

## Testing with cURL

```bash
# Get all medicines
curl https://localhost:5001/api/MedicinesApi

# Search medicines
curl "https://localhost:5001/api/MedicinesApi/search?q=para&branchId=1"

# Create a sale
curl -X POST https://localhost:5001/api/SalesApi \
  -H "Content-Type: application/json" \
  -d '{
    "userId": 4,
    "customerId": 1,
    "paymentMethod": "Cash",
    "items": [
      {"medicineId": 1, "quantity": 2}
    ]
  }'

# Get dashboard stats
curl "https://localhost:5001/api/DashboardApi/stats?branchId=1"
```
