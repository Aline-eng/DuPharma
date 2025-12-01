using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using DuPharma.Data;
using System.Text;

namespace DuPharma.Controllers;

[Authorize]
public class ReportsController : Controller
{
    private readonly AppDbContext _context;

    public ReportsController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string period = "daily")
    {
        var userRole = GetUserRole();
        var allowedPeriods = GetAllowedPeriods(userRole);
        
        if (!allowedPeriods.Contains(period))
            period = allowedPeriods.First();

        var reportData = await GetReportData(period);
        
        var viewModel = new ReportsViewModel
        {
            Period = period,
            AllowedPeriods = allowedPeriods,
            CanExport = User.IsInRole("Admin"),
            ReportData = reportData
        };

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> ExportCsv(string period = "daily")
    {
        if (!User.IsInRole("Admin"))
            return Forbid();

        var reportData = await GetReportData(period);
        var csv = GenerateCsv(reportData, period);
        
        var fileName = $"sales_report_{period}_{DateTime.Now:yyyyMMdd}.csv";
        return File(Encoding.UTF8.GetBytes(csv), "text/csv", fileName);
    }

    private string GetUserRole()
    {
        if (User.IsInRole("Admin")) return "Admin";
        if (User.IsInRole("Manager")) return "Manager";
        return "Pharmacist";
    }

    private List<string> GetAllowedPeriods(string role)
    {
        return role switch
        {
            "Admin" => new List<string> { "daily", "weekly", "monthly" },
            "Manager" => new List<string> { "weekly" },
            "Pharmacist" => new List<string> { "daily" },
            _ => new List<string> { "daily" }
        };
    }

    private async Task<List<ReportItem>> GetReportData(string period)
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var user = await _context.Users.FindAsync(userId);
        
        var startDate = period switch
        {
            "daily" => DateTime.Now.AddHours(-24),
            "weekly" => DateTime.Now.AddDays(-7),
            "monthly" => DateTime.Now.AddDays(-30),
            _ => DateTime.Now.AddHours(-24)
        };

        var query = _context.Sales
            .Where(s => s.SaleDate >= startDate);
        
        if (!User.IsInRole("Admin") && user?.BranchId != null)
        {
            query = query.Where(s => s.BranchId == user.BranchId);
        }

        return await query
            .Include(s => s.SaleItems)
            .ThenInclude(si => si.Batch)
            .ThenInclude(b => b.Medicine)
            .Include(s => s.Customer)
            .Include(s => s.SoldByUser)
            .SelectMany(s => s.SaleItems.Select(si => new ReportItem
            {
                SaleId = s.InvoiceNumber,
                Medicine = si.Batch.Medicine.GenericName,
                Quantity = si.Quantity,
                Total = si.SubTotal,
                Customer = s.Customer != null ? s.Customer.FullName : "Walk-in",
                Date = s.SaleDate,
                SoldBy = s.SoldByUser.FullName
            }))
            .OrderByDescending(r => r.Date)
            .ToListAsync();
    }

    private string GenerateCsv(List<ReportItem> data, string period)
    {
        var csv = new StringBuilder();
        csv.AppendLine($"Sales Report - {period.ToUpper()}");
        csv.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        csv.AppendLine();
        csv.AppendLine("Sale ID,Medicine,Quantity,Total,Customer,Date,Sold By");
        
        foreach (var item in data)
        {
            csv.AppendLine($"{item.SaleId},{item.Medicine},{item.Quantity},{item.Total:F2},{item.Customer},{item.Date:yyyy-MM-dd HH:mm},{item.SoldBy}");
        }
        
        return csv.ToString();
    }
}

public class ReportsViewModel
{
    public string Period { get; set; } = "daily";
    public List<string> AllowedPeriods { get; set; } = new();
    public bool CanExport { get; set; }
    public List<ReportItem> ReportData { get; set; } = new();
}

public class ReportItem
{
    public string SaleId { get; set; } = string.Empty;
    public string Medicine { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Total { get; set; }
    public string Customer { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string SoldBy { get; set; } = string.Empty;
}