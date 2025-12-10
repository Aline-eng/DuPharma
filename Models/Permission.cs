using System.ComponentModel.DataAnnotations;

namespace DuPharma.Models;

public class Permission
{
    public int PermissionId { get; set; }
    
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(200)]
    public string Description { get; set; } = string.Empty;
    
    [MaxLength(50)]
    public string Category { get; set; } = string.Empty;
    
    public bool IsActive { get; set; } = true;
    
    public ICollection<UserPermission> UserPermissions { get; set; } = new List<UserPermission>();
}

public class UserPermission
{
    public int UserPermissionId { get; set; }
    public int UserId { get; set; }
    public int PermissionId { get; set; }
    public DateTime GrantedAt { get; set; } = DateTime.Now;
    public int? GrantedByUserId { get; set; }
    
    public User User { get; set; } = null!;
    public Permission Permission { get; set; } = null!;
    public User? GrantedByUser { get; set; }
}

// Permission constants for easy reference
public static class Permissions
{
    // Dashboard
    public const string ViewDashboard = "ViewDashboard";
    
    // Medicines
    public const string ViewMedicines = "ViewMedicines";
    public const string CreateMedicines = "CreateMedicines";
    public const string EditMedicines = "EditMedicines";
    public const string DeleteMedicines = "DeleteMedicines";
    public const string ViewMedicineBatches = "ViewMedicineBatches";
    
    // Sales
    public const string ViewSales = "ViewSales";
    public const string CreateSales = "CreateSales";
    public const string ViewAllBranchSales = "ViewAllBranchSales";
    
    // Orders
    public const string ViewOrders = "ViewOrders";
    public const string ApproveOrders = "ApproveOrders";
    public const string ViewAllBranchOrders = "ViewAllBranchOrders";
    
    // Customers
    public const string ViewCustomers = "ViewCustomers";
    public const string CreateCustomers = "CreateCustomers";
    public const string EditCustomers = "EditCustomers";
    
    // Users
    public const string ViewUsers = "ViewUsers";
    public const string CreateUsers = "CreateUsers";
    public const string EditUsers = "EditUsers";
    public const string DeleteUsers = "DeleteUsers";
    
    // Reports
    public const string ViewReports = "ViewReports";
    public const string ExportReports = "ExportReports";
    
    // Batches
    public const string ViewBatches = "ViewBatches";
    public const string CreateBatches = "CreateBatches";
    public const string EditBatches = "EditBatches";
    
    // System Administration
    public const string ManagePermissions = "ManagePermissions";
    public const string ViewAuditLogs = "ViewAuditLogs";
    
    // Contact Messages
    public const string ViewContactMessages = "ViewContactMessages";
    public const string ReplyContactMessages = "ReplyContactMessages";
    public const string DeleteContactMessages = "DeleteContactMessages";
}