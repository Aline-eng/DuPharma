using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DuPharma.Data;

namespace DuPharma.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public class BatchesApiController : ControllerBase
{
    private readonly AppDbContext _context;

    public BatchesApiController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? medicineId, [FromQuery] int? branchId)
    {
        var query = _context.Batches
            .Include(b => b.Medicine)
            .Include(b => b.Supplier)
            .AsQueryable();

        if (medicineId.HasValue)
            query = query.Where(b => b.MedicineId == medicineId);

        if (branchId.HasValue)
            query = query.Where(b => b.BranchId == branchId);

        var batches = await query
            .Select(b => new
            {
                b.BatchId,
                b.BatchNumber,
                b.ExpiryDate,
                b.QuantityOnHand,
                b.PurchasePrice,
                b.SellingPrice,
                b.ReceivedDate,
                b.BranchId,
                Medicine = new
                {
                    b.Medicine.MedicineId,
                    b.Medicine.GenericName,
                    b.Medicine.BrandName,
                    b.Medicine.Strength,
                    b.Medicine.Form
                },
                Supplier = new
                {
                    b.Supplier.SupplierId,
                    b.Supplier.Name
                }
            })
            .OrderBy(b => b.ExpiryDate)
            .ToListAsync();

        return Ok(batches);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var batch = await _context.Batches
            .Include(b => b.Medicine)
            .Include(b => b.Supplier)
            .Where(b => b.BatchId == id)
            .Select(b => new
            {
                b.BatchId,
                b.BatchNumber,
                b.ExpiryDate,
                b.QuantityOnHand,
                b.PurchasePrice,
                b.SellingPrice,
                b.ReceivedDate,
                b.BranchId,
                Medicine = new
                {
                    b.Medicine.MedicineId,
                    b.Medicine.GenericName,
                    b.Medicine.BrandName,
                    b.Medicine.Strength,
                    b.Medicine.Form
                },
                Supplier = new
                {
                    b.Supplier.SupplierId,
                    b.Supplier.Name,
                    b.Supplier.ContactPerson,
                    b.Supplier.Phone
                }
            })
            .FirstOrDefaultAsync();

        if (batch == null)
            return NotFound();

        return Ok(batch);
    }

    [HttpGet("expiring")]
    public async Task<IActionResult> GetExpiring([FromQuery] int? branchId, [FromQuery] int days = 90)
    {
        var expiryDate = DateTime.Now.AddDays(days);

        var query = _context.Batches
            .Include(b => b.Medicine)
            .Where(b => b.ExpiryDate <= expiryDate && b.ExpiryDate > DateTime.Now && b.QuantityOnHand > 0);

        if (branchId.HasValue)
            query = query.Where(b => b.BranchId == branchId);

        var batches = await query
            .Select(b => new
            {
                b.BatchId,
                b.BatchNumber,
                b.ExpiryDate,
                b.QuantityOnHand,
                b.SellingPrice,
                b.BranchId,
                DaysUntilExpiry = (b.ExpiryDate - DateTime.Now).Days,
                Medicine = new
                {
                    b.Medicine.GenericName,
                    b.Medicine.BrandName,
                    b.Medicine.Strength
                }
            })
            .OrderBy(b => b.ExpiryDate)
            .ToListAsync();

        return Ok(batches);
    }
}
