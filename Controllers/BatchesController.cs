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
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var user = await _context.Users.FindAsync(userId);
        
        var query = _context.Batches
            .Include(b => b.Medicine)
            .Include(b => b.Supplier)
            .Include(b => b.Branch)
            .AsQueryable();
        
        if (!User.IsInRole("Admin") && user?.BranchId != null)
        {
            query = query.Where(b => b.BranchId == user.BranchId);
        }
        
        var batches = await query.OrderBy(b => b.ExpiryDate).ToListAsync();
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
        ModelState.Remove("Branch");
        
        if (ModelState.IsValid)
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
            var user = await _context.Users.FindAsync(userId);
            batch.BranchId = user?.BranchId ?? 1;
            
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
        ModelState.Remove("Branch");

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