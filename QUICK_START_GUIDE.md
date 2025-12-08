# DuPharma Quick Start Guide

## Running the Application

```bash
cd DuPharma
dotnet restore
dotnet build
dotnet run
```

The application will start at `https://localhost:5001` or `http://localhost:5000`

## Default Landing Page

The application now opens to: **`/Shop/Home`** (Customer Home Page)

## Navigation Structure

### Public Shop (No Login Required)

```
┌─────────────────────────────────────────────────────┐
│  DuPharma Pharmacy                                  │
│  [Home] [About Us] [Shop] [Categories▼] [Contact]  │
│                              [Staff Login Button]   │
└─────────────────────────────────────────────────────┘

Home Page (/Shop/Home)
├── Hero section with "Shop Now" button
├── Feature highlights
└── Why Choose DuPharma section

About Us (/Shop/About)
├── Company story
├── Mission, Vision, Values
└── Branch locations

Shop (/Shop)
├── Live search bar
├── Branch filter dropdown
├── Medicine grid
└── Cart button

Contact Us (/Shop/Contact)
├── Contact information
├── Contact form
└── Branch details

Medicine Details (/Shop/Details/{id})
├── Product information
├── Available in Branches section
└── Add to Cart button
```

### Staff Area (Login Required)

```
Staff Login (/Account/Login)
├── [Back to Shop] button (top-left)
├── Email & Password fields
└── Login button

After Login → Staff Dashboard
├── Dashboard
├── Medicines
├── Sales
├── Orders
└── Reports
```

## Key Features

### 1. Live Search
- Type in search box → Results appear instantly
- Minimum 2 characters to trigger
- Works with branch filter

### 2. Branch Selection

#### On Shop Page:
- Filter dropdown: "All Branches" | "Main" | "North" | "South"
- Shows only medicines available in selected branch

#### On Checkout:
- Required dropdown: "Select Your Branch"
- Validates stock availability
- Shows error if items not available

#### On Details Page:
- "Available in Branches" section
- Shows which branches have stock
- Displays quantity per branch

### 3. Shopping Cart
- Stored in browser localStorage
- Persists across page refreshes
- Shows item count in navbar
- Modal view with item list

### 4. Checkout Process
1. Add items to cart
2. Click "Cart" button
3. Review items
4. Click "Checkout"
5. **Select branch** (required)
6. Fill customer details
7. Place order

## Branch-Based Shopping Flow

```
Customer Journey:
1. Browse medicines (all branches or filter by branch)
2. View medicine details (see which branches have it)
3. Add to cart
4. Proceed to checkout
5. Select pickup branch
6. System validates availability in selected branch
7. Order placed or error shown if unavailable
```

## Error Handling

### Checkout Errors:
- **No branch selected**: "Please select a branch"
- **Item unavailable**: "Items not available in selected branch: [Medicine Name]"
- **Empty cart**: "Cart is empty"

### Search:
- **No results**: "No medicines found. Try a different search term"
- **No branch stock**: "No medicines available in the selected branch"

## API Endpoints for Developers

### Public Endpoints (No Auth)
```
GET  /Shop/Home                    - Home page
GET  /Shop/About                   - About Us page
GET  /Shop/Contact                 - Contact Us page
GET  /Shop                         - Shop page (with filters)
     ?search={query}
     &category={category}
     &branchId={id}
GET  /Shop/Details/{id}            - Medicine details
     ?branchId={id}
GET  /Shop/SearchLive              - Live search API
     ?q={query}
     &branchId={id}
POST /Orders/Create                - Create order
     Body: { customerName, customerEmail, customerPhone, 
             deliveryAddress, branchId, items[] }
POST /Orders/UploadPrescription    - Upload prescription file
```

### Staff Endpoints (Auth Required)
```
GET  /Account/Login                - Staff login page
POST /Account/Login                - Login action
GET  /Home/Index                   - Staff dashboard
GET  /Medicines                    - Medicine management
GET  /Sales                        - Sales management
GET  /Orders                       - Order management
```

## Testing the New Features

### Test Live Search:
1. Go to `/Shop`
2. Type "para" in search box
3. See results filter instantly
4. Clear search → all medicines return

### Test Branch Filter:
1. Go to `/Shop`
2. Select "Main" from branch dropdown
3. See only Main branch medicines
4. Select "All Branches" → see all medicines

### Test Branch Availability:
1. Go to any medicine details page
2. Scroll to "Available in Branches" section
3. See green badges for available branches

### Test Checkout Validation:
1. Add medicine to cart
2. Click "Checkout"
3. Try to place order without selecting branch → Error shown
4. Select branch → Order placed successfully

### Test Navigation:
1. Click each navbar link
2. Verify pages load correctly
3. On login page, click "Back to Shop"
4. Verify return to shop

## Customization

### Update Branch Information:
Edit in database `Branches` table or in:
- `Pages/Shop/About.cshtml` (branch cards)
- `Pages/Shop/Contact.cshtml` (branch details)

### Update Contact Information:
Edit in:
- `Pages/Shared/_ShopLayout.cshtml` (footer)
- `Pages/Shop/Contact.cshtml` (contact page)

### Update Social Media Links:
Edit in `Pages/Shared/_ShopLayout.cshtml` footer section:
```html
<a href="#" class="social-icon"><i class="bi bi-facebook"></i></a>
```
Replace `#` with actual URLs.

### Update Colors:
Edit in `wwwroot/css/site.css`:
- Primary color: `#3498db`
- Success color: `#27ae60`
- Danger color: `#e74c3c`
- Dark background: `#0D182B`

## Troubleshooting

### Issue: Live search not working
**Solution**: Check browser console for JavaScript errors. Ensure jQuery is loaded.

### Issue: Branch filter shows no medicines
**Solution**: Verify batches have correct `BranchId` in database.

### Issue: Checkout fails
**Solution**: Check that `BranchId` is being sent in request. Open browser DevTools → Network tab.

### Issue: "Back to Shop" button not visible
**Solution**: Clear browser cache and refresh page.

### Issue: Footer not displaying correctly
**Solution**: Ensure Bootstrap CSS is loaded. Check browser console.

## Default Credentials

**Staff Login:**
- Email: `admin@dupharma.local`
- Password: `ChangeMe123!`
- Role: Admin

## Support

For issues or questions:
- Check `UPGRADE_SUMMARY.md` for detailed changes
- Review `README.md` for project overview
- Check browser console for JavaScript errors
- Verify database connection in `appsettings.json`
