using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DuPharma.Data;

namespace DuPharma.Controllers;

[ApiController]
[Route("api/medicines")]
public class MedicinesApiController : ControllerBase
{
    private readonly AppDbContext _context;

    public MedicinesApiController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Search(string q = "")
    {
        var medicines = await _context.Medicines
            .Where(m => string.IsNullOrEmpty(q) || 
                       m.GenericName.Contains(q) || 
                       m.BrandName.Contains(q))
            .Select(m => new
            {
                m.MedicineId,
                m.GenericName,
                m.BrandName,
                m.Strength,
                m.Form,
                m.Unit,
                AvailableStock = m.Batches
                    .Where(b => b.ExpiryDate > DateTime.Now && b.QuantityOnHand > 0)
                    .Sum(b => b.QuantityOnHand),
                Price = m.Batches
                    .Where(b => b.ExpiryDate > DateTime.Now && b.QuantityOnHand > 0)
                    .OrderBy(b => b.ExpiryDate)
                    .Select(b => b.SellingPrice)
                    .FirstOrDefault()
            })
            .Where(m => m.AvailableStock > 0)
            .Take(20)
            .ToListAsync();

        return Ok(medicines);
    }
}