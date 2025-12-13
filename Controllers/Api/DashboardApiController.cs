using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DuPharma.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace DuPharma.Controllers.Api;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ApiController]
[Route("api/[controller]")]
public class DashboardApiController : ControllerBase
{
    private readonly AppDbContext _context;

    public DashboardApiController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats([FromQuery] int? branchId)
    {
        var today = DateTime.Today;
        var thirtyDaysAgo = today.AddDays(-30);

        var salesQuery = _context.Sales.AsQueryable();
        if (branchId.HasValue)
            salesQuery = salesQuery.Where(s => s.BranchId == branchId);

        var batchesQuery = _context.Batches.AsQueryable();
        if (branchId.HasValue)
            batchesQuery = batchesQuery.Where(b => b.BranchId == branchId);

        var stats = new
        {
            TodaySales = await salesQuery.Where(s => s.SaleDate.Date == today).SumAsync(s => s.TotalAmount),
            TodayTransactions = await salesQuery.CountAsync(s => s.SaleDate.Date == today),
            MonthlyRevenue = await salesQuery.Where(s => s.SaleDate >= thirtyDaysAgo).SumAsync(s => s.TotalAmount),
            TotalCustomers = await _context.Customers.CountAsync(),
            LowStockCount = await _context.Medicines
                .Where(m => m.Batches
                    .Where(b => b.ExpiryDate > DateTime.Now && (!branchId.HasValue || b.BranchId == branchId))
                    .Sum(b => b.QuantityOnHand) <= m.ReorderLevel)
                .CountAsync(),
            ExpiringCount = await batchesQuery
                .Where(b => b.ExpiryDate <= DateTime.Now.AddDays(90) && b.ExpiryDate > DateTime.Now && b.QuantityOnHand > 0)
                .CountAsync()
        };

        return Ok(stats);
    }

    [HttpGet("top-selling")]
    public async Task<IActionResult> GetTopSelling([FromQuery] int? branchId, [FromQuery] int days = 30, [FromQuery] int limit = 10)
    {
        var startDate = DateTime.Now.AddDays(-days);

        var query = _context.SaleItems
            .Include(si => si.Batch)
                .ThenInclude(b => b.Medicine)
            .Include(si => si.Sale)
            .Where(si => si.Sale.SaleDate >= startDate);

        if (branchId.HasValue)
            query = query.Where(si => si.Sale.BranchId == branchId);

        var topSelling = await query
            .GroupBy(si => new
            {
                si.Batch.Medicine.MedicineId,
                si.Batch.Medicine.GenericName,
                si.Batch.Medicine.BrandName,
                si.Batch.Medicine.Strength
            })
            .Select(g => new
            {
                g.Key.MedicineId,
                g.Key.GenericName,
                g.Key.BrandName,
                g.Key.Strength,
                TotalQuantitySold = g.Sum(si => si.Quantity),
                TotalRevenue = g.Sum(si => si.SubTotal),
                TransactionCount = g.Count()
            })
            .OrderByDescending(x => x.TotalQuantitySold)
            .Take(limit)
            .ToListAsync();

        return Ok(topSelling);
    }

    [HttpGet("sales-trend")]
    public async Task<IActionResult> GetSalesTrend([FromQuery] int? branchId, [FromQuery] int days = 7)
    {
        var startDate = DateTime.Today.AddDays(-days);

        var query = _context.Sales.Where(s => s.SaleDate >= startDate);
        if (branchId.HasValue)
            query = query.Where(s => s.BranchId == branchId);

        var trend = await query
            .GroupBy(s => s.SaleDate.Date)
            .Select(g => new
            {
                Date = g.Key,
                TotalSales = g.Sum(s => s.TotalAmount),
                TransactionCount = g.Count()
            })
            .OrderBy(x => x.Date)
            .ToListAsync();

        return Ok(trend);
    }
}
