using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DuPharma.Data;

namespace DuPharma.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public class MedicinesApiController : ControllerBase
{
    private readonly AppDbContext _context;

    public MedicinesApiController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] int? branchId)
    {
        var query = _context.Medicines.AsQueryable();

        if (!string.IsNullOrEmpty(search))
            query = query.Where(m => m.GenericName.Contains(search) || m.BrandName.Contains(search));

        var medicines = await query
            .Select(m => new
            {
                m.MedicineId,
                m.GenericName,
                m.BrandName,
                m.Strength,
                m.Form,
                m.Unit,
                m.ReorderLevel,
                TotalStock = m.Batches
                    .Where(b => b.ExpiryDate > DateTime.Now && b.QuantityOnHand > 0 
                        && (branchId == null || b.BranchId == branchId))
                    .Sum(b => b.QuantityOnHand),
                LowestPrice = m.Batches
                    .Where(b => b.ExpiryDate > DateTime.Now && b.QuantityOnHand > 0
                        && (branchId == null || b.BranchId == branchId))
                    .Min(b => (decimal?)b.SellingPrice)
            })
            .ToListAsync();

        return Ok(medicines);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id, [FromQuery] int? branchId)
    {
        var medicine = await _context.Medicines
            .Where(m => m.MedicineId == id)
            .Select(m => new
            {
                m.MedicineId,
                m.GenericName,
                m.BrandName,
                m.Strength,
                m.Form,
                m.Unit,
                m.ReorderLevel,
                Batches = m.Batches
                    .Where(b => b.ExpiryDate > DateTime.Now && b.QuantityOnHand > 0
                        && (branchId == null || b.BranchId == branchId))
                    .Select(b => new
                    {
                        b.BatchId,
                        b.BatchNumber,
                        b.ExpiryDate,
                        b.QuantityOnHand,
                        b.SellingPrice,
                        b.BranchId
                    })
                    .OrderBy(b => b.ExpiryDate)
            })
            .FirstOrDefaultAsync();

        if (medicine == null)
            return NotFound();

        return Ok(medicine);
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string q, [FromQuery] int? branchId)
    {
        if (string.IsNullOrEmpty(q))
            return Ok(new List<object>());

        var medicines = await _context.Medicines
            .Where(m => m.GenericName.Contains(q) || m.BrandName.Contains(q))
            .Select(m => new
            {
                m.MedicineId,
                m.GenericName,
                m.BrandName,
                m.Strength,
                m.Form,
                m.Unit,
                AvailableStock = m.Batches
                    .Where(b => b.ExpiryDate > DateTime.Now && b.QuantityOnHand > 0
                        && (branchId == null || b.BranchId == branchId))
                    .Sum(b => b.QuantityOnHand),
                Price = m.Batches
                    .Where(b => b.ExpiryDate > DateTime.Now && b.QuantityOnHand > 0
                        && (branchId == null || b.BranchId == branchId))
                    .OrderBy(b => b.ExpiryDate)
                    .Select(b => b.SellingPrice)
                    .FirstOrDefault()
            })
            .Where(m => m.AvailableStock > 0)
            .Take(20)
            .ToListAsync();

        return Ok(medicines);
    }

    [HttpGet("low-stock")]
    public async Task<IActionResult> GetLowStock([FromQuery] int? branchId)
    {
        var lowStock = await _context.Medicines
            .Select(m => new
            {
                m.MedicineId,
                m.GenericName,
                m.BrandName,
                m.ReorderLevel,
                CurrentStock = m.Batches
                    .Where(b => b.ExpiryDate > DateTime.Now 
                        && (branchId == null || b.BranchId == branchId))
                    .Sum(b => b.QuantityOnHand)
            })
            .Where(m => m.CurrentStock <= m.ReorderLevel)
            .OrderBy(m => m.CurrentStock)
            .ToListAsync();

        return Ok(lowStock);
    }
}
