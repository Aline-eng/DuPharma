# Permission-Based Access Control (PBAC) Implementation

## What is PBAC vs RBAC?

### RBAC (Role-Based Access Control) - OLD SYSTEM:
- Users assigned to **roles** (Admin, Manager, Pharmacist)
- Access granted based on **role membership**
- Less granular control
- Example: `[Authorize(Roles = "Admin,Manager")]`

### PBAC (Permission-Based Access Control) - NEW SYSTEM:
- Users assigned specific **permissions** (ViewMedicines, CreateSales, etc.)
- Access granted based on **individual permissions**
- More granular and flexible control
- Example: `[RequirePermission(Permissions.ViewMedicines)]`

## Why PBAC is Required

✅ **Granular Control**: Assign specific permissions instead of broad roles
✅ **Flexibility**: Users can have custom permission combinations
✅ **Security**: Principle of least privilege - users get only what they need
✅ **Scalability**: Easy to add new permissions without changing roles
✅ **Audit Trail**: Track who granted which permissions when

## Implementation Overview

### 1. Database Schema

```sql
Permissions Table:
- PermissionId (PK)
- Name (unique)
- Description
- Category
- IsActive

UserPermissions Table:
- UserPermissionId (PK)
- UserId (FK)
- PermissionId (FK)
- GrantedAt
- GrantedByUserId (FK)
```

### 2. Permission Constants

```csharp
public static class Permissions
{
    public const string ViewDashboard = "ViewDashboard";
    public const string ViewMedicines = "ViewMedicines";
    public const string CreateMedicines = "CreateMedicines";
    // ... 25+ permissions defined
}
```

### 3. Permission Service

```csharp
public interface IPermissionService
{
    Task<bool> HasPermissionAsync(int userId, string permission);
    Task<List<string>> GetUserPermissionsAsync(int userId);
    Task GrantPermissionAsync(int userId, string permission, int grantedByUserId);
    Task RevokePermissionAsync(int userId, string permission);
}
```

### 4. RequirePermission Attribute

```csharp
[RequirePermission(Permissions.ViewMedicines)]
public class MedicinesController : Controller
{
    [RequirePermission(Permissions.CreateMedicines)]
    public IActionResult Create() { }
}
```

## Files Created/Modified

### New Files:
1. `Models/Permission.cs` - Permission entities and constants
2. `Services/PermissionService.cs` - Permission management service
3. `Attributes/RequirePermissionAttribute.cs` - Permission authorization attribute
4. `CreatePermissionTables.sql` - Database setup script

### Modified Files:
1. `Models/User.cs` - Added UserPermissions navigation property
2. `Data/AppDbContext.cs` - Added Permission entities and relationships
3. `Program.cs` - Registered PermissionService in DI
4. `Controllers/HomeController.cs` - Converted from RBAC to PBAC
5. `Controllers/MedicinesController.cs` - Converted from RBAC to PBAC
6. `Controllers/SalesController.cs` - Converted from RBAC to PBAC
7. `Controllers/OrdersController.cs` - Converted from RBAC to PBAC

## Permission Matrix

| Permission | Admin | Manager | Pharmacist |
|------------|-------|---------|------------|
| ViewDashboard | ✅ | ✅ | ✅ |
| ViewMedicines | ✅ | ✅ | ✅ |
| CreateMedicines | ✅ | ✅ | ❌ |
| EditMedicines | ✅ | ✅ | ❌ |
| DeleteMedicines | ✅ | ❌ | ❌ |
| ViewSales | ✅ | ✅ | ✅ |
| CreateSales | ✅ | ✅ | ✅ |
| ViewAllBranchSales | ✅ | ❌ | ❌ |
| ViewOrders | ✅ | ✅ | ✅ |
| ApproveOrders | ✅ | ✅ | ❌ |
| ViewAllBranchOrders | ✅ | ❌ | ❌ |
| ManagePermissions | ✅ | ❌ | ❌ |
| DeleteUsers | ✅ | ❌ | ❌ |

## Setup Instructions

### 1. Run Database Migration
```bash
sqlcmd -S . -d dupharma_db -i CreatePermissionTables.sql
```

### 2. Build and Run Application
```bash
dotnet build
dotnet run
```

### 3. Test Permissions
- Login as different users
- Verify access to different features
- Check that permissions are enforced

## How to Use PBAC

### Protecting Controllers:
```csharp
[RequirePermission(Permissions.ViewMedicines)]
public class MedicinesController : Controller { }
```

### Protecting Actions:
```csharp
[RequirePermission(Permissions.CreateMedicines)]
public IActionResult Create() { }
```

### Checking Permissions in Code:
```csharp
var hasPermission = await User.HasPermissionAsync(_permissionService, Permissions.ViewReports);
if (hasPermission)
{
    // Show reports
}
```

### Checking Permissions in Views:
```html
@if (await User.HasPermissionAsync(permissionService, Permissions.CreateMedicines))
{
    <a href="/Medicines/Create" class="btn btn-primary">Add Medicine</a>
}
```

## Key Benefits Achieved

### 1. Granular Control
- Users can have specific permissions without full role access
- Example: A user can view medicines but not create them

### 2. Flexible Assignment
- Permissions can be granted/revoked individually
- No need to change user roles for minor access changes

### 3. Better Security
- Principle of least privilege enforced
- Users only get permissions they actually need

### 4. Audit Trail
- Track who granted permissions and when
- Full history of permission changes

### 5. Scalability
- Easy to add new permissions without affecting existing users
- Permissions can be grouped by categories

## Migration from RBAC

### Before (RBAC):
```csharp
[Authorize(Roles = "Admin,Manager")]
public IActionResult Create() { }

if (User.IsInRole("Admin")) { }
```

### After (PBAC):
```csharp
[RequirePermission(Permissions.CreateMedicines)]
public IActionResult Create() { }

if (await User.HasPermissionAsync(_permissionService, Permissions.ViewReports)) { }
```

## Testing Checklist

- [ ] Run `CreatePermissionTables.sql`
- [ ] Build application successfully
- [ ] Login as Admin → Access all features
- [ ] Login as Manager → Limited access (no user management)
- [ ] Login as Pharmacist → Basic access only
- [ ] Verify permission checks work on all controllers
- [ ] Test that unauthorized access is blocked
- [ ] Verify branch-based filtering still works

## Future Enhancements

1. **Permission Management UI**: Create admin interface to manage permissions
2. **Role Templates**: Create permission templates for common roles
3. **Time-based Permissions**: Permissions that expire after certain time
4. **Resource-based Permissions**: Permissions tied to specific resources
5. **Permission Groups**: Group related permissions together

## Troubleshooting

### Issue: Permission denied errors
**Solution**: Check that user has required permissions in database

### Issue: Service not found errors
**Solution**: Ensure `IPermissionService` is registered in `Program.cs`

### Issue: Database errors
**Solution**: Run `CreatePermissionTables.sql` script first

### Issue: Existing users can't access anything
**Solution**: Default permissions are assigned by the SQL script based on user roles

The system now uses **Permission-Based Access Control (PBAC)** instead of Role-Based Access Control (RBAC), providing much more granular and flexible security control!