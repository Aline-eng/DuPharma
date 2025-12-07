using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using DuPharma.Data;
using DuPharma.Models;

namespace DuPharma.Controllers;

[AllowAnonymous]
public class ShopController : Controller
{
    private readonly AppDbContext _context;

    public ShopController(AppDbContext context) => _context = context;

    public async Task<IActionResult> Index(string search = "")
    {
        var medicines = await _context.Medicines
            .Include(m => m.Batches)
            .Where(m => string.IsNullOrEmpty(search) || 
                       m.GenericName.Contains(search) || 
                       m.BrandName.Contains(search))
            .Select(m => new
            {
                m.MedicineId,
                m.GenericName,
                m.BrandName,
                m.Strength,
                m.Form,
                m.Description,
                m.ImageUrl,
                m.RequiresPrescription,
                Price = m.Batches.Where(b => b.QuantityOnHand > 0).Min(b => (decimal?)b.SellingPrice) ?? 0,
                Stock = m.Batches.Sum(b => b.QuantityOnHand)
            })
            .Where(m => m.Stock > 0)
            .ToListAsync();

        ViewBag.Search = search;
        return View(medicines);
    }

    public async Task<IActionResult> Details(int id)
    {
        var medicine = await _context.Medicines
            .Include(m => m.Batches)
            .FirstOrDefaultAsync(m => m.MedicineId == id);

        if (medicine == null) return NotFound();

        var price = medicine.Batches.Where(b => b.QuantityOnHand > 0).Min(b => (decimal?)b.SellingPrice) ?? 0;
        var stock = medicine.Batches.Sum(b => b.QuantityOnHand);

        ViewBag.Price = price;
        ViewBag.Stock = stock;
        return View(medicine);
    }
}
