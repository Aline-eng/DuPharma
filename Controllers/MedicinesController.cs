using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using DuPharma.Data;
using DuPharma.Models;
using DuPharma.Attributes;
using DuPharma.Services;

namespace DuPharma.Controllers;

[RequirePermission(Permissions.ViewMedicines)]
public class MedicinesController : Controller
{
    private readonly AppDbContext _context;
    private readonly IPermissionService _permissionService;

    public MedicinesController(AppDbContext context, IPermissionService permissionService)
    {
        _context = context;
        _permissionService = permissionService;
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

    [RequirePermission(Permissions.CreateMedicines)]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [RequirePermission(Permissions.CreateMedicines)]
    public async Task<IActionResult> Create(Medicine medicine)
    {
        
        if (ModelState.IsValid)
        {
            _context.Medicines.Add(medicine);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(medicine);
    }

    [RequirePermission(Permissions.EditMedicines)]
    public async Task<IActionResult> Edit(int id)
    {
        
        var medicine = await _context.Medicines.FindAsync(id);
        if (medicine == null) return NotFound();
        return View(medicine);
    }

    [HttpPost]
    [RequirePermission(Permissions.EditMedicines)]
    public async Task<IActionResult> Edit(int id, Medicine medicine)
    {
        
        if (id != medicine.MedicineId) return NotFound();

        if (ModelState.IsValid)
        {
            _context.Update(medicine);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(medicine);
    }

    [RequirePermission(Permissions.DeleteMedicines)]
    public async Task<IActionResult> Delete(int id)
    {
        
        var medicine = await _context.Medicines
            .Include(m => m.Batches)
            .FirstOrDefaultAsync(m => m.MedicineId == id);
        
        if (medicine == null) return NotFound();
        return View(medicine);
    }

    [HttpPost, ActionName("Delete")]
    [RequirePermission(Permissions.DeleteMedicines)]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        
        var medicine = await _context.Medicines.FindAsync(id);
        if (medicine != null)
        {
            _context.Medicines.Remove(medicine);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    [RequirePermission(Permissions.ViewMedicineBatches)]
    public async Task<IActionResult> Batches(int id)
    {
        var medicine = await _context.Medicines
            .Include(m => m.Batches)
            .ThenInclude(b => b.Supplier)
            .FirstOrDefaultAsync(m => m.MedicineId == id);

        if (medicine == null) return NotFound();
        return View(medicine);
    }


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