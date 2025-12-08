using Microsoft.EntityFrameworkCore;
using DuPharma.Data;
using DuPharma.Models;
using System.Security.Claims;

namespace DuPharma.Services;

public interface IPermissionService
{
    Task<bool> HasPermissionAsync(int userId, string permission);
    Task<List<string>> GetUserPermissionsAsync(int userId);
    Task GrantPermissionAsync(int userId, string permission, int grantedByUserId);
    Task RevokePermissionAsync(int userId, string permission);
    Task<List<Permission>> GetAllPermissionsAsync();
    Task SeedDefaultPermissionsAsync();
    Task AssignDefaultPermissionsToUserAsync(int userId, int role);
}

public class PermissionService : IPermissionService
{
    private readonly AppDbContext _context;

    public PermissionService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> HasPermissionAsync(int userId, string permission)
    {
        return await _context.UserPermissions
            .AnyAsync(up => up.UserId == userId && 
                           up.Permission.Name == permission && 
                           up.Permission.IsActive);
    }

    public async Task<List<string>> GetUserPermissionsAsync(int userId)
    {
        return await _context.UserPermissions
            .Where(up => up.UserId == userId && up.Permission.IsActive)
            .Select(up => up.Permission.Name)
            .ToListAsync();
    }

    public async Task GrantPermissionAsync(int userId, string permission, int grantedByUserId)
    {
        var permissionEntity = await _context.Permissions
            .FirstOrDefaultAsync(p => p.Name == permission);

        if (permissionEntity == null) return;

        var existingPermission = await _context.UserPermissions
            .FirstOrDefaultAsync(up => up.UserId == userId && up.PermissionId == permissionEntity.PermissionId);

        if (existingPermission == null)
        {
            _context.UserPermissions.Add(new UserPermission
            {
                UserId = userId,
                PermissionId = permissionEntity.PermissionId,
                GrantedByUserId = grantedByUserId,
                GrantedAt = DateTime.Now
            });

            await _context.SaveChangesAsync();
        }
    }

    public async Task RevokePermissionAsync(int userId, string permission)
    {
        var userPermission = await _context.UserPermissions
            .Include(up => up.Permission)
            .FirstOrDefaultAsync(up => up.UserId == userId && up.Permission.Name == permission);

        if (userPermission != null)
        {
            _context.UserPermissions.Remove(userPermission);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<Permission>> GetAllPermissionsAsync()
    {
        return await _context.Permissions
            .Where(p => p.IsActive)
            .OrderBy(p => p.Category)
            .ThenBy(p => p.Name)
            .ToListAsync();
    }

    public async Task SeedDefaultPermissionsAsync()
    {
        var permissions = new[]
        {
            new Permission { Name = Permissions.ViewDashboard, Description = "View dashboard and statistics", Category = "Dashboard" },
            
            new Permission { Name = Permissions.ViewMedicines, Description = "View medicines list", Category = "Medicines" },
            new Permission { Name = Permissions.CreateMedicines, Description = "Create new medicines", Category = "Medicines" },
            new Permission { Name = Permissions.EditMedicines, Description = "Edit existing medicines", Category = "Medicines" },
            new Permission { Name = Permissions.DeleteMedicines, Description = "Delete medicines", Category = "Medicines" },
            new Permission { Name = Permissions.ViewMedicineBatches, Description = "View medicine batches", Category = "Medicines" },
            
            new Permission { Name = Permissions.ViewSales, Description = "View sales records", Category = "Sales" },
            new Permission { Name = Permissions.CreateSales, Description = "Create new sales", Category = "Sales" },
            new Permission { Name = Permissions.ViewAllBranchSales, Description = "View sales from all branches", Category = "Sales" },
            
            new Permission { Name = Permissions.ViewOrders, Description = "View customer orders", Category = "Orders" },
            new Permission { Name = Permissions.ApproveOrders, Description = "Approve/reject orders", Category = "Orders" },
            new Permission { Name = Permissions.ViewAllBranchOrders, Description = "View orders from all branches", Category = "Orders" },
            
            new Permission { Name = Permissions.ViewCustomers, Description = "View customers list", Category = "Customers" },
            new Permission { Name = Permissions.CreateCustomers, Description = "Create new customers", Category = "Customers" },
            new Permission { Name = Permissions.EditCustomers, Description = "Edit customer information", Category = "Customers" },
            
            new Permission { Name = Permissions.ViewUsers, Description = "View users list", Category = "Users" },
            new Permission { Name = Permissions.CreateUsers, Description = "Create new users", Category = "Users" },
            new Permission { Name = Permissions.EditUsers, Description = "Edit user information", Category = "Users" },
            new Permission { Name = Permissions.DeleteUsers, Description = "Delete users", Category = "Users" },
            
            new Permission { Name = Permissions.ViewReports, Description = "View reports", Category = "Reports" },
            new Permission { Name = Permissions.ExportReports, Description = "Export reports", Category = "Reports" },
            
            new Permission { Name = Permissions.ViewBatches, Description = "View medicine batches", Category = "Batches" },
            new Permission { Name = Permissions.CreateBatches, Description = "Create new batches", Category = "Batches" },
            new Permission { Name = Permissions.EditBatches, Description = "Edit batch information", Category = "Batches" },
            
            new Permission { Name = Permissions.ManagePermissions, Description = "Manage user permissions", Category = "Administration" },
            new Permission { Name = Permissions.ViewAuditLogs, Description = "View audit logs", Category = "Administration" }
        };

        foreach (var permission in permissions)
        {
            var exists = await _context.Permissions.AnyAsync(p => p.Name == permission.Name);
            if (!exists)
            {
                _context.Permissions.Add(permission);
            }
        }

        await _context.SaveChangesAsync();
    }

    public async Task AssignDefaultPermissionsToUserAsync(int userId, int role)
    {
        var permissions = new List<string>();

        switch (role)
        {
            case 1: // Admin
                permissions.AddRange(new[]
                {
                    Permissions.ViewDashboard,
                    Permissions.ViewMedicines, Permissions.CreateMedicines, Permissions.EditMedicines, Permissions.DeleteMedicines, Permissions.ViewMedicineBatches,
                    Permissions.ViewSales, Permissions.CreateSales, Permissions.ViewAllBranchSales,
                    Permissions.ViewOrders, Permissions.ApproveOrders, Permissions.ViewAllBranchOrders,
                    Permissions.ViewCustomers, Permissions.CreateCustomers, Permissions.EditCustomers,
                    Permissions.ViewUsers, Permissions.CreateUsers, Permissions.EditUsers, Permissions.DeleteUsers,
                    Permissions.ViewReports, Permissions.ExportReports,
                    Permissions.ViewBatches, Permissions.CreateBatches, Permissions.EditBatches,
                    Permissions.ManagePermissions, Permissions.ViewAuditLogs
                });
                break;

            case 2: // Manager
                permissions.AddRange(new[]
                {
                    Permissions.ViewDashboard,
                    Permissions.ViewMedicines, Permissions.CreateMedicines, Permissions.EditMedicines, Permissions.ViewMedicineBatches,
                    Permissions.ViewSales, Permissions.CreateSales,
                    Permissions.ViewOrders, Permissions.ApproveOrders,
                    Permissions.ViewCustomers, Permissions.CreateCustomers, Permissions.EditCustomers,
                    Permissions.ViewReports,
                    Permissions.ViewBatches, Permissions.CreateBatches, Permissions.EditBatches
                });
                break;

            case 3: // Pharmacist
                permissions.AddRange(new[]
                {
                    Permissions.ViewDashboard,
                    Permissions.ViewMedicines, Permissions.ViewMedicineBatches,
                    Permissions.ViewSales, Permissions.CreateSales,
                    Permissions.ViewOrders,
                    Permissions.ViewCustomers, Permissions.CreateCustomers,
                    Permissions.ViewBatches
                });
                break;
        }

        foreach (var permission in permissions)
        {
            await GrantPermissionAsync(userId, permission, userId);
        }
    }
}