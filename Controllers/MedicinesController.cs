using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using DuPharma.Data;
using DuPharma.Models;

namespace DuPharma.Controllers;

[Authorize]
public class MedicinesController : Controller
{
    private readonly AppDbContext _context;

    public MedicinesController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var medicines = await _context.Medicines
            .Include(m => m.Batches)
            .Select(m => new MedicineListViewModel
            {
                MedicineId = m.MedicineId,
                GenericName = m.GenericName,
                BrandName = m.BrandName,
                Strength = m.Strength,
                Form = m.Form,
                Unit = m.Unit,
                ReorderLevel = m.ReorderLevel,
                TotalStock = m.Batches.Where(b => b.ExpiryDate > DateTime.Now).Sum(b => b.QuantityOnHand)
            })
            .ToListAsync();

        return View(medicines);
    }

    public IActionResult Create()
    {
        if (!IsManagerOrAdmin()) return Forbid();
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(Medicine medicine)
    {
        if (!IsManagerOrAdmin()) return Forbid();
        
        if (ModelState.IsValid)
        {
            _context.Medicines.Add(medicine);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(medicine);
    }

    public async Task<IActionResult> Edit(int id)
    {
        if (!IsManagerOrAdmin()) return Forbid();
        
        var medicine = await _context.Medicines.FindAsync(id);
        if (medicine == null) return NotFound();
        return View(medicine);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, Medicine medicine)
    {
        if (!IsManagerOrAdmin()) return Forbid();
        
        if (id != medicine.MedicineId) return NotFound();

        if (ModelState.IsValid)
        {
            _context.Update(medicine);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(medicine);
    }

    public async Task<IActionResult> Delete(int id)
    {
        if (!IsAdmin()) return Forbid();
        
        var medicine = await _context.Medicines
            .Include(m => m.Batches)
            .FirstOrDefaultAsync(m => m.MedicineId == id);
        
        if (medicine == null) return NotFound();
        return View(medicine);
    }

    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        if (!IsAdmin()) return Forbid();
        
        var medicine = await _context.Medicines.FindAsync(id);
        if (medicine != null)
        {
            _context.Medicines.Remove(medicine);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Batches(int id)
    {
        var medicine = await _context.Medicines
            .Include(m => m.Batches)
            .ThenInclude(b => b.Supplier)
            .FirstOrDefaultAsync(m => m.MedicineId == id);

        if (medicine == null) return NotFound();
        return View(medicine);
    }

    private bool IsAdmin() => User.IsInRole("Admin");
    private bool IsManagerOrAdmin() => User.IsInRole("Admin") || User.IsInRole("Manager");
}

public class MedicineListViewModel
{
    public int MedicineId { get; set; }
    public string GenericName { get; set; } = string.Empty;
    public string BrandName { get; set; } = string.Empty;
    public string Strength { get; set; } = string.Empty;
    public string Form { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public int ReorderLevel { get; set; }
    public int TotalStock { get; set; }
}