# DuPharma API Guide

## Quick Start

1. **Start the application:**
   ```bash
   dotnet run
   ```

2. **Access Swagger UI:**
   ```
   https://localhost:5001/swagger
   ```

3. **API Base URL:**
   ```
   https://localhost:5001/api
   ```

---

## Available Endpoints

### 🏥 Medicines API (`/api/MedicinesApi`)

#### Get All Medicines
```http
GET /api/MedicinesApi?search=paracetamol&branchId=1
```
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

#### Search Medicines (Used in POS)
```http
GET /api/MedicinesApi/search?q=para&branchId=1
```

#### Get Medicine Details
```http
GET /api/MedicinesApi/1?branchId=1
```

#### Get Low Stock Medicines
```http
GET /api/MedicinesApi/low-stock?branchId=1
```

---

### 📦 Batches API (`/api/BatchesApi`)

#### Get All Batches
```http
GET /api/BatchesApi?medicineId=1&branchId=1
```

#### Get Expiring Batches
```http
GET /api/BatchesApi/expiring?branchId=1&days=90
```
**Response:**
```json
[
  {
    "batchId": 5,
    "batchNumber": "B2024001002",
    "expiryDate": "2025-12-28T00:00:00",
    "quantityOnHand": 80,
    "sellingPrice": 15.00,
    "branchId": 1,
    "daysUntilExpiry": 27,
    "medicine": {
      "genericName": "Paracetamol",
      "brandName": "Panadol",
      "strength": "500mg"
    }
  }
]
```

---

### 💰 Sales API (`/api/SalesApi`)

#### Get All Sales
```http
GET /api/SalesApi?startDate=2025-01-01&endDate=2025-01-31&branchId=1
```

#### Create New Sale
```http
POST /api/SalesApi
Content-Type: application/json

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
      "medicineId": 3,
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

#### Get Sales Summary
```http
GET /api/SalesApi/summary?branchId=1&startDate=2025-01-01&endDate=2025-01-31
```

---

### 👥 Customers API (`/api/CustomersApi`)

#### Get All Customers
```http
GET /api/CustomersApi?search=alice
```

#### Get Customer Details
```http
GET /api/CustomersApi/1
```

#### Get Customer Prescriptions
```http
GET /api/CustomersApi/1/prescriptions
```

---

### 📊 Dashboard API (`/api/DashboardApi`)

#### Get Dashboard Stats
```http
GET /api/DashboardApi/stats?branchId=1
```
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

#### Get Top Selling Medicines
```http
GET /api/DashboardApi/top-selling?branchId=1&days=30&limit=10
```

#### Get Sales Trend
```http
GET /api/DashboardApi/sales-trend?branchId=1&days=7
```

---

## Usage Examples

### JavaScript (Frontend)
```javascript
// Search medicines for POS
async function searchMedicines(query, branchId) {
    const response = await fetch(
        `/api/MedicinesApi/search?q=${query}&branchId=${branchId}`
    );
    return await response.json();
}

// Create a sale
async function createSale(saleData) {
    const response = await fetch('/api/SalesApi', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(saleData)
    });
    return await response.json();
}

// Get dashboard stats
async function getDashboardStats(branchId) {
    const response = await fetch(`/api/DashboardApi/stats?branchId=${branchId}`);
    return await response.json();
}
```

### C# (Console App)
```csharp
using System.Net.Http.Json;

var client = new HttpClient { BaseAddress = new Uri("https://localhost:5001") };

// Get medicines
var medicines = await client.GetFromJsonAsync<List<Medicine>>(
    "/api/MedicinesApi?branchId=1"
);

// Create sale
var saleRequest = new {
    UserId = 4,
    CustomerId = 1,
    PaymentMethod = "Cash",
    Items = new[] { new { MedicineId = 1, Quantity = 2 } }
};
var response = await client.PostAsJsonAsync("/api/SalesApi", saleRequest);
```

### Python
```python
import requests

BASE_URL = 'https://localhost:5001/api'

# Get low stock medicines
response = requests.get(f'{BASE_URL}/MedicinesApi/low-stock?branchId=1', verify=False)
low_stock = response.json()

# Create sale
sale_data = {
    'userId': 4,
    'customerId': 1,
    'paymentMethod': 'Cash',
    'items': [{'medicineId': 1, 'quantity': 2}]
}
response = requests.post(f'{BASE_URL}/SalesApi', json=sale_data, verify=False)
```

### cURL
```bash
# Get medicines
curl "https://localhost:5001/api/MedicinesApi?branchId=1"

# Search medicines
curl "https://localhost:5001/api/MedicinesApi/search?q=para&branchId=1"

# Create sale
curl -X POST https://localhost:5001/api/SalesApi \
  -H "Content-Type: application/json" \
  -d '{
    "userId": 4,
    "customerId": 1,
    "paymentMethod": "Cash",
    "items": [{"medicineId": 1, "quantity": 2}]
  }'

# Get dashboard stats
curl "https://localhost:5001/api/DashboardApi/stats?branchId=1"
```

---

## Authentication

Currently, API endpoints are **open** (no authentication required). To add authentication:

1. Add `[Authorize]` attribute to controllers
2. Configure JWT or API keys in `Program.cs`
3. Include authentication headers in requests

---

## Error Handling

All endpoints return standard HTTP status codes:

- `200 OK` - Success
- `201 Created` - Resource created
- `400 Bad Request` - Invalid request data
- `404 Not Found` - Resource not found
- `500 Internal Server Error` - Server error

**Error Response Format:**
```json
{
  "error": "Insufficient stock: Paracetamol: Required 5, Available 2"
}
```

---

## Integration Scenarios

### 1. Mobile POS App
- Use `/api/MedicinesApi/search` for medicine lookup
- Use `/api/SalesApi` POST to create sales
- Use `/api/CustomersApi` for customer management

### 2. Inventory Dashboard
- Use `/api/DashboardApi/stats` for overview
- Use `/api/MedicinesApi/low-stock` for alerts
- Use `/api/BatchesApi/expiring` for expiry warnings

### 3. Reporting System
- Use `/api/SalesApi` with date filters for sales reports
- Use `/api/DashboardApi/top-selling` for product analysis
- Use `/api/DashboardApi/sales-trend` for trend analysis

### 4. Third-Party Integration
- Sync inventory with external systems
- Export sales data to accounting software
- Import customer data from CRM systems

---

## Testing

### Using Swagger UI
1. Navigate to `https://localhost:5001/swagger`
2. Expand any endpoint
3. Click "Try it out"
4. Fill parameters and click "Execute"

### Using Postman
Import the collection from the repository or create requests manually using the examples above.

---

## Branch Filtering

Most endpoints support `branchId` parameter to filter data by pharmacy branch:
- `branchId=1` - Main Branch
- `branchId=2` - North Branch  
- `branchId=3` - South Branch
- No `branchId` - All branches (Admin only)

---

## Performance Notes

- All endpoints use Entity Framework with optimized queries
- Large datasets are automatically limited (e.g., search returns max 20 results)
- Use date filters on sales endpoints for better performance
- Consider pagination for large result sets in production

---

## Support

For API support:
1. Check Swagger UI for endpoint details
2. Review error messages in responses
3. Ensure database connection is working
4. Verify branch permissions for filtered endpoints