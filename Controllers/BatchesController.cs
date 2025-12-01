using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using DuPharma.Data;
using DuPharma.Models;

namespace DuPharma.Controllers;

[Authorize(Roles = "Admin,Manager")]
public class BatchesController : Controller
{
    private readonly AppDbContext _context;

    public BatchesController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var batches = await _context.Batches
            .Include(b => b.Medicine)
            .Include(b => b.Supplier)
            .OrderBy(b => b.ExpiryDate)
            .ToListAsync();
        return View(batches);
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.Medicines = await _context.Medicines.ToListAsync();
        ViewBag.Suppliers = await _context.Suppliers.ToListAsync();
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(Batch batch)
    {
        ModelState.Remove("Medicine");
        ModelState.Remove("Supplier");
        
        if (ModelState.IsValid)
        {
            _context.Batches.Add(batch);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        
        ViewBag.Medicines = await _context.Medicines.ToListAsync();
        ViewBag.Suppliers = await _context.Suppliers.ToListAsync();
        return View(batch);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var batch = await _context.Batches.FindAsync(id);
        if (batch == null) return NotFound();
        
        ViewBag.Medicines = await _context.Medicines.ToListAsync();
        ViewBag.Suppliers = await _context.Suppliers.ToListAsync();
        return View(batch);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, Batch batch)
    {
        if (id != batch.BatchId) return NotFound();
        
        ModelState.Remove("Medicine");
        ModelState.Remove("Supplier");

        if (ModelState.IsValid)
        {
            _context.Update(batch);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        
        ViewBag.Medicines = await _context.Medicines.ToListAsync();
        ViewBag.Suppliers = await _context.Suppliers.ToListAsync();
        return View(batch);
    }
}