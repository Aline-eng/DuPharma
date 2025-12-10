# Branch-Based Orders Update

## Changes Implemented

### 1. Branch-Based Order Filtering
**File: `Controllers/OrdersController.cs`**

✅ **Orders are now filtered by branch:**
- **Admin users**: Can see ALL orders from ALL branches
- **Manager/Pharmacist users**: Can ONLY see orders from THEIR assigned branch
- Orders are filtered based on the user's `BranchId`

**Logic:**
```csharp
// Admin sees all orders
if (isAdmin) {
    // Show all orders
}
// Non-admin sees only their branch orders
else if (user.BranchId != null) {
    // Filter: orders.Where(o => o.BranchId == user.BranchId)
}
```

### 2. Branch Column Added to Orders Page
**File: `Pages/Orders/Index.cshtml`**

✅ **New "Branch" column added:**
- Displays branch name as a badge
- Shows which branch the order belongs to
- Visible to both Admin and branch staff
- Helps Admin identify which branch has each order

**Table Structure:**
```
Order # | Branch | Customer | Phone | Date | Items | Total | Status | Actions
```

### 3. Test Data Added
**File: `AddBranchMedicines.sql`**

✅ **Medicines added to branches for testing:**

**North Branch (BranchId = 2):**
- Paracetamol (Panadol) - 300 units - $15.00
- Ibuprofen (Advil) - 250 units - $18.00
- Aspirin (Bayer) - 200 units - $12.00

**South Branch (BranchId = 3):**
- Amoxicillin (Amoxil) - 180 units - $35.00
- Metformin (Glucophage) - 220 units - $22.00
- Lisinopril (Prinivil) - 150 units - $30.00

## How to Test

### Step 1: Add Test Medicines to Branches
Run the SQL script to add medicines to North and South branches:

```bash
# Using SQL Server Management Studio or sqlcmd
sqlcmd -S . -d dupharma_db -i AddBranchMedicines.sql
```

Or manually execute the SQL in SSMS.

### Step 2: Create Test Orders

#### Test Order for North Branch:
1. Go to public shop: `https://localhost:5001/Shop/Index`
2. Click "Branches" dropdown → Select "North Branch"
3. You should see: Panadol, Advil, Aspirin
4. Add items to cart
5. Checkout and select "North Branch"
6. Place order

#### Test Order for South Branch:
1. Go to public shop
2. Click "Branches" dropdown → Select "South Branch"
3. You should see: Amoxil, Glucophage, Prinivil
4. Add items to cart
5. Checkout and select "South Branch"
6. Place order

#### Test Order for Main Branch:
1. Go to public shop
2. Click "Branches" dropdown → Select "Main Branch"
3. You should see existing Main Branch medicines
4. Add items to cart
5. Checkout and select "Main Branch"
6. Place order

### Step 3: Test Order Visibility

#### As Admin:
1. Login as: `admin@dupharma.local` / `ChangeMe123!`
2. Go to Orders page
3. **Expected**: See ALL orders from ALL branches (Main, North, South)
4. **Branch column**: Shows which branch each order belongs to

#### As Manager/Pharmacist (Main Branch):
1. Login as: `john.manager@dupharma.local` / `ChangeMe123!`
2. Go to Orders page
3. **Expected**: See ONLY orders from Main Branch
4. **Branch column**: Shows "Main Branch" for all visible orders

#### As Manager/Pharmacist (North Branch):
1. Login as: `sarah.manager@dupharma.local` / `ChangeMe123!`
   (Note: You may need to update this user's branch to North in database)
2. Go to Orders page
3. **Expected**: See ONLY orders from North Branch
4. **Branch column**: Shows "North Branch" for all visible orders

### Step 4: Update User Branch Assignment (Optional)

To test with different branches, update user's branch in database:

```sql
-- Assign Sarah Manager to North Branch
UPDATE Users SET BranchId = 2 WHERE Email = 'sarah.manager@dupharma.local';

-- Assign David Pharmacist to South Branch
UPDATE Users SET BranchId = 3 WHERE Email = 'david.pharmacist@dupharma.local';
```

## Database Schema

### Users Table
- `BranchId` (int, nullable) - Links user to their branch

### Orders Table
- `BranchId` (int, not null) - Links order to branch where it was placed

### Batches Table
- `BranchId` (int, not null) - Links medicine batch to branch

## Business Rules

### Order Placement:
1. Customer selects branch during checkout
2. System validates medicine availability in selected branch
3. Order is created with selected `BranchId`

### Order Visibility:
1. **Admin Role**: 
   - Can view ALL orders regardless of branch
   - Useful for oversight and management

2. **Manager/Pharmacist Role**:
   - Can ONLY view orders for their assigned branch
   - Prevents cross-branch order access
   - Maintains branch-level privacy

### Order Processing:
- Each branch processes only their own orders
- Admin can monitor all branches
- Branch staff cannot see other branches' orders

## Testing Checklist

- [ ] Run `AddBranchMedicines.sql` script
- [ ] Verify medicines appear in North Branch filter
- [ ] Verify medicines appear in South Branch filter
- [ ] Place order for North Branch
- [ ] Place order for South Branch
- [ ] Place order for Main Branch
- [ ] Login as Admin → See all 3 orders
- [ ] Login as Main Branch Manager → See only Main Branch order
- [ ] Verify Branch column displays correctly
- [ ] Verify order approval works per branch
- [ ] Test that non-admin cannot see other branch orders

## Files Modified

1. `Controllers/OrdersController.cs` - Added branch filtering logic
2. `Pages/Orders/Index.cshtml` - Added Branch column

## Files Created

1. `AddBranchMedicines.sql` - Test data for North and South branches
2. `BRANCH_ORDERS_UPDATE.md` - This documentation

## Benefits

✅ **Security**: Branch staff cannot access other branches' orders
✅ **Privacy**: Customer orders are only visible to relevant branch
✅ **Admin Oversight**: Admin can monitor all branches
✅ **Clear Identification**: Branch column shows order location
✅ **Scalability**: Easy to add more branches

## Notes

- Existing orders in database will need `BranchId` set if null
- All new orders automatically get `BranchId` from checkout
- Admin role is determined by `User.IsInRole("Admin")`
- Branch assignment is based on `User.BranchId`
