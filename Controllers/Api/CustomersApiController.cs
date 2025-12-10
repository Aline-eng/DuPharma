using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DuPharma.Data;

namespace DuPharma.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public class CustomersApiController : ControllerBase
{
    private readonly AppDbContext _context;

    public CustomersApiController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? search)
    {
        var query = _context.Customers.AsQueryable();

        if (!string.IsNullOrEmpty(search))
            query = query.Where(c => c.FullName.Contains(search) || c.Phone.Contains(search) || c.NationalId.Contains(search));

        var customers = await query
            .Select(c => new
            {
                c.CustomerId,
                c.FullName,
                c.Phone,
                c.Address,
                c.NationalId,
                TotalPurchases = c.Sales.Count,
                TotalSpent = c.Sales.Sum(s => s.TotalAmount)
            })
            .OrderBy(c => c.FullName)
            .ToListAsync();

        return Ok(customers);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var customer = await _context.Customers
            .Where(c => c.CustomerId == id)
            .Select(c => new
            {
                c.CustomerId,
                c.FullName,
                c.Phone,
                c.Address,
                c.NationalId,
                TotalPurchases = c.Sales.Count,
                TotalSpent = c.Sales.Sum(s => s.TotalAmount),
                RecentSales = c.Sales
                    .OrderByDescending(s => s.SaleDate)
                    .Take(5)
                    .Select(s => new
                    {
                        s.SaleId,
                        s.InvoiceNumber,
                        s.SaleDate,
                        s.TotalAmount
                    })
            })
            .FirstOrDefaultAsync();

        if (customer == null)
            return NotFound();

        return Ok(customer);
    }

    [HttpGet("{id}/prescriptions")]
    public async Task<IActionResult> GetPrescriptions(int id)
    {
        var prescriptions = await _context.Prescriptions
            .Where(p => p.CustomerId == id)
            .Include(p => p.PrescriptionItems)
                .ThenInclude(pi => pi.Medicine)
            .Select(p => new
            {
                p.PrescriptionId,
                p.PrescriptionNo,
                p.DoctorName,
                p.CreatedAt,
                p.Notes,
                Items = p.PrescriptionItems.Select(pi => new
                {
                    Medicine = new
                    {
                        pi.Medicine.GenericName,
                        pi.Medicine.BrandName
                    },
                    pi.Dosage,
                    pi.Quantity,
                    pi.Frequency,
                    pi.Duration
                })
            })
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        return Ok(prescriptions);
    }
}
