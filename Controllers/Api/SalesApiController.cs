using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DuPharma.Data;
using DuPharma.Services;

namespace DuPharma.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public class SalesApiController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly DispenseService _dispenseService;

    public SalesApiController(AppDbContext context, DispenseService dispenseService)
    {
        _context = context;
        _dispenseService = dispenseService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, [FromQuery] int? branchId)
    {
        var query = _context.Sales
            .Include(s => s.SoldByUser)
            .Include(s => s.Customer)
            .AsQueryable();

        if (startDate.HasValue)
            query = query.Where(s => s.SaleDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(s => s.SaleDate <= endDate.Value);

        if (branchId.HasValue)
            query = query.Where(s => s.BranchId == branchId);

        var sales = await query
            .Select(s => new
            {
                s.SaleId,
                s.InvoiceNumber,
                s.SaleDate,
                s.TotalAmount,
                s.PaymentMethod,
                s.BranchId,
                SoldBy = s.SoldByUser.FullName,
                Customer = s.Customer != null ? s.Customer.FullName : null,
                ItemCount = s.SaleItems.Count
            })
            .OrderByDescending(s => s.SaleDate)
            .ToListAsync();

        return Ok(sales);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var sale = await _context.Sales
            .Include(s => s.SoldByUser)
            .Include(s => s.Customer)
            .Include(s => s.SaleItems)
                .ThenInclude(si => si.Batch)
                    .ThenInclude(b => b.Medicine)
            .Where(s => s.SaleId == id)
            .Select(s => new
            {
                s.SaleId,
                s.InvoiceNumber,
                s.SaleDate,
                s.TotalAmount,
                s.PaymentMethod,
                s.BranchId,
                SoldBy = new
                {
                    s.SoldByUser.UserId,
                    s.SoldByUser.FullName
                },
                Customer = s.Customer != null ? new
                {
                    s.Customer.CustomerId,
                    s.Customer.FullName,
                    s.Customer.Phone
                } : null,
                Items = s.SaleItems.Select(si => new
                {
                    si.SaleItemId,
                    si.Quantity,
                    si.UnitPrice,
                    si.SubTotal,
                    Batch = new
                    {
                        si.Batch.BatchNumber,
                        si.Batch.ExpiryDate
                    },
                    Medicine = new
                    {
                        si.Batch.Medicine.GenericName,
                        si.Batch.Medicine.BrandName,
                        si.Batch.Medicine.Strength,
                        si.Batch.Medicine.Form
                    }
                })
            })
            .FirstOrDefaultAsync();

        if (sale == null)
            return NotFound();

        return Ok(sale);
    }

    [HttpPost]
    public async Task<IActionResult> CreateSale([FromBody] CreateSaleRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var orderDto = new OrderDto
            {
                CustomerId = request.CustomerId,
                PaymentMethod = request.PaymentMethod,
                Items = request.Items.Select(i => new OrderItemDto
                {
                    MedicineId = i.MedicineId,
                    Quantity = i.Quantity
                }).ToList()
            };

            var sale = await _dispenseService.DispenseAsync(orderDto, request.UserId);

            return CreatedAtAction(nameof(GetById), new { id = sale.SaleId }, new
            {
                sale.SaleId,
                sale.InvoiceNumber,
                sale.TotalAmount
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, [FromQuery] int? branchId)
    {
        var query = _context.Sales.AsQueryable();

        if (startDate.HasValue)
            query = query.Where(s => s.SaleDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(s => s.SaleDate <= endDate.Value);

        if (branchId.HasValue)
            query = query.Where(s => s.BranchId == branchId);

        var summary = await query
            .GroupBy(s => 1)
            .Select(g => new
            {
                TotalSales = g.Count(),
                TotalRevenue = g.Sum(s => s.TotalAmount),
                AverageOrderValue = g.Average(s => s.TotalAmount),
                CashSales = g.Count(s => s.PaymentMethod == "Cash"),
                CardSales = g.Count(s => s.PaymentMethod == "Card")
            })
            .FirstOrDefaultAsync();

        return Ok(summary ?? new
        {
            TotalSales = 0,
            TotalRevenue = 0m,
            AverageOrderValue = 0m,
            CashSales = 0,
            CardSales = 0
        });
    }
}

public class CreateSaleRequest
{
    public int UserId { get; set; }
    public int? CustomerId { get; set; }
    public string PaymentMethod { get; set; } = "Cash";
    public List<SaleItemRequest> Items { get; set; } = new();
}

public class SaleItemRequest
{
    public int MedicineId { get; set; }
    public int Quantity { get; set; }
}
