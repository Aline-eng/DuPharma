using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using DuPharma.Data;
using System.Security.Claims;

namespace DuPharma.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly AppDbContext _context;

    public HomeController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var dashboardData = new DashboardViewModel
        {
            LowStockMedicines = await GetLowStockMedicines(),
            ExpiringMedicines = await GetExpiringMedicines(),
            TopSellingMedicines = await GetTopSellingMedicines(),
            TodaySales = await GetTodaySalesCount(),
            TotalMedicines = await _context.Medicines.CountAsync(),
            TotalCustomers = await _context.Customers.CountAsync()
        };

        return View(dashboardData);
    }

    private async Task<List<LowStockAlert>> GetLowStockMedicines()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var user = await _context.Users.FindAsync(userId);
        var isAdmin = User.IsInRole("Admin");
        
        if (isAdmin)
        {
            // Admin sees low stock per branch
            return await _context.Branches
                .SelectMany(branch => _context.Medicines
                    .Select(m => new LowStockAlert
                    {
                        MedicineId = m.MedicineId,
                        MedicineName = m.GenericName,
                        CurrentStock = m.Batches.Where(b => b.BranchId == branch.BranchId && b.ExpiryDate > DateTime.Now).Sum(b => b.QuantityOnHand),
                        ReorderLevel = m.ReorderLevel,
                        BranchName = branch.BranchName
                    })
                    .Where(x => x.CurrentStock <= x.ReorderLevel))
                .ToListAsync();
        }
        else
        {
            // Manager/Pharmacist sees only their branch
            var branchId = user?.BranchId ?? 1;
            return await _context.Medicines
                .Select(m => new LowStockAlert
                {
                    MedicineId = m.MedicineId,
                    MedicineName = m.GenericName,
                    CurrentStock = m.Batches.Where(b => b.BranchId == branchId && b.ExpiryDate > DateTime.Now).Sum(b => b.QuantityOnHand),
                    ReorderLevel = m.ReorderLevel,
                    BranchName = ""
                })
                .Where(x => x.CurrentStock <= x.ReorderLevel)
                .ToListAsync();
        }
    }

    private async Task<List<ExpiryAlert>> GetExpiringMedicines()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var user = await _context.Users.FindAsync(userId);
        var threeMonthsFromNow = DateTime.Now.AddDays(90);
        
        var query = _context.Batches
            .Where(b => b.ExpiryDate <= threeMonthsFromNow && b.ExpiryDate > DateTime.Now && b.QuantityOnHand > 0);
        
        if (!User.IsInRole("Admin") && user?.BranchId != null)
        {
            query = query.Where(b => b.BranchId == user.BranchId);
        }
        
        return await query
            .Select(b => new ExpiryAlert
            {
                BatchId = b.BatchId,
                MedicineName = b.Medicine.GenericName,
                BatchNumber = b.BatchNumber,
                ExpiryDate = b.ExpiryDate,
                Quantity = b.QuantityOnHand
            })
            .OrderBy(x => x.ExpiryDate)
            .ToListAsync();
    }

    private async Task<List<TopSellingMedicine>> GetTopSellingMedicines()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var user = await _context.Users.FindAsync(userId);
        var thirtyDaysAgo = DateTime.Now.AddDays(-30);
        
        var query = _context.SaleItems
            .Where(si => si.Sale.SaleDate >= thirtyDaysAgo);
        
        if (!User.IsInRole("Admin") && user?.BranchId != null)
        {
            query = query.Where(si => si.Sale.BranchId == user.BranchId);
        }
        
        return await query
            .GroupBy(si => new { si.Batch.Medicine.MedicineId, si.Batch.Medicine.GenericName })
            .Select(g => new TopSellingMedicine
            {
                MedicineName = g.Key.GenericName,
                TotalQuantitySold = g.Sum(si => si.Quantity),
                TotalRevenue = g.Sum(si => si.SubTotal)
            })
            .OrderByDescending(x => x.TotalQuantitySold)
            .Take(10)
            .ToListAsync();
    }

    private async Task<int> GetTodaySalesCount()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var user = await _context.Users.FindAsync(userId);
        
        var query = _context.Sales.Where(s => s.SaleDate.Date == DateTime.Today);
        
        if (!User.IsInRole("Admin") && user?.BranchId != null)
        {
            query = query.Where(s => s.BranchId == user.BranchId);
        }
        
        return await query.CountAsync();
    }
}

public class DashboardViewModel
{
    public List<LowStockAlert> LowStockMedicines { get; set; } = new();
    public List<ExpiryAlert> ExpiringMedicines { get; set; } = new();
    public List<TopSellingMedicine> TopSellingMedicines { get; set; } = new();
    public int TodaySales { get; set; }
    public int TotalMedicines { get; set; }
    public int TotalCustomers { get; set; }
}

public class LowStockAlert
{
    public int MedicineId { get; set; }
    public string MedicineName { get; set; } = string.Empty;
    public int CurrentStock { get; set; }
    public int ReorderLevel { get; set; }
    public string BranchName { get; set; } = string.Empty;
}

public class ExpiryAlert
{
    public int BatchId { get; set; }
    public string MedicineName { get; set; } = string.Empty;
    public string BatchNumber { get; set; } = string.Empty;
    public DateTime ExpiryDate { get; set; }
    public int Quantity { get; set; }
}

public class TopSellingMedicine
{
    public string MedicineName { get; set; } = string.Empty;
    public int TotalQuantitySold { get; set; }
    public decimal TotalRevenue { get; set; }
}