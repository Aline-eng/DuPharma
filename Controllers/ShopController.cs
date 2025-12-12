using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using DuPharma.Data;
using DuPharma.Models;
using System.Linq;

namespace DuPharma.Controllers;

[AllowAnonymous]
public class ShopController : Controller
{
    private readonly AppDbContext _context;

    public ShopController(AppDbContext context) => _context = context;

    public IActionResult Home() => View();
    public IActionResult About() => View();
    public IActionResult Contact()
    {
        ViewBag.Branches = _context.Branches.Select(b => new { b.BranchId, b.BranchName }).ToList();
        return View(new ContactFormViewModel());
    }

    [HttpPost]
    public IActionResult Contact(ContactFormViewModel model)
    {
        ModelState.Remove("HoneyPot");
        
        if (!string.IsNullOrEmpty(model?.HoneyPot))
        {
            TempData["Success"] = "Your message has been sent!";
            return RedirectToAction("Contact");
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Branches = _context.Branches.Select(b => new { b.BranchId, b.BranchName }).ToList();
            TempData["Error"] = "Please fill in all required fields correctly.";
            return View(model);
        }

        try
        {
            var contactMessage = new ContactMessage
            {
                FullName = model.FullName,
                Email = model.Email,
                Phone = model.Phone ?? string.Empty,
                Subject = model.Subject,
                Message = model.Message,
                BranchId = model.BranchId,
                CreatedAt = DateTime.Now
            };

            _context.ContactMessages.Add(contactMessage);
            _context.SaveChanges();

            TempData["Success"] = "Thank you! Your message has been sent successfully.";
            return RedirectToAction("Contact");
        }
        catch
        {
            TempData["Error"] = "Sorry, there was an error. Please try again.";
            return View(model);
        }
    }

    public async Task<IActionResult> Index(string search = "", string category = "", int? branchId = null)
    {
        var query = _context.Medicines
            .Include(m => m.Batches)
            .ThenInclude(b => b.Branch)
            .AsQueryable();

        if (!string.IsNullOrEmpty(search))
            query = query.Where(m => m.GenericName.Contains(search) || m.BrandName.Contains(search));

        if (!string.IsNullOrEmpty(category))
            query = query.Where(m => m.Form.Contains(category));

        var medicines = await query
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
                Price = m.Batches.Where(b => b.QuantityOnHand > 0 && (!branchId.HasValue || b.BranchId == branchId.Value))
                    .Min(b => (decimal?)b.SellingPrice) ?? 0,
                Stock = m.Batches.Where(b => !branchId.HasValue || b.BranchId == branchId.Value)
                    .Sum(b => b.QuantityOnHand),
                AvailableBranches = m.Batches.Where(b => b.QuantityOnHand > 0)
                    .Select(b => new { b.Branch.BranchId, b.Branch.BranchName })
                    .Distinct().ToList()
            })
            .Where(m => m.Stock > 0)
            .ToListAsync();

        ViewBag.Search = search;
        ViewBag.Category = category;
        ViewBag.BranchId = branchId;
        ViewBag.Branches = await _context.Branches.ToListAsync();
        return View(medicines);
    }

    [HttpGet]
    public async Task<IActionResult> SearchLive(string q = "", int? branchId = null)
    {
        var query = _context.Medicines
            .Include(m => m.Batches)
            .Where(m => m.GenericName.Contains(q) || m.BrandName.Contains(q));

        var medicines = await query
            .Select(m => new
            {
                m.MedicineId,
                m.GenericName,
                m.BrandName,
                m.Strength,
                m.Form,
                m.ImageUrl,
                m.RequiresPrescription,
                Price = m.Batches.Where(b => b.QuantityOnHand > 0 && (!branchId.HasValue || b.BranchId == branchId.Value))
                    .Min(b => (decimal?)b.SellingPrice) ?? 0,
                Stock = m.Batches.Where(b => !branchId.HasValue || b.BranchId == branchId.Value)
                    .Sum(b => b.QuantityOnHand)
            })
            .Where(m => m.Stock > 0)
            .Take(20)
            .ToListAsync();

        return Json(medicines);
    }

    public async Task<IActionResult> Details(int id, int? branchId = null)
    {
        var medicine = await _context.Medicines
            .Include(m => m.Batches)
            .ThenInclude(b => b.Branch)
            .FirstOrDefaultAsync(m => m.MedicineId == id);

        if (medicine == null) return NotFound();

        var availableBranches = medicine.Batches
            .Where(b => b.QuantityOnHand > 0)
            .Select(b => new { b.Branch.BranchId, b.Branch.BranchName, Stock = b.QuantityOnHand })
            .GroupBy(b => new { b.BranchId, b.BranchName })
            .Select(g => new { g.Key.BranchId, g.Key.BranchName, Stock = g.Sum(x => x.Stock) })
            .ToList();

        var price = medicine.Batches
            .Where(b => b.QuantityOnHand > 0 && (!branchId.HasValue || b.BranchId == branchId.Value))
            .Min(b => (decimal?)b.SellingPrice) ?? 0;
        
        var stock = medicine.Batches
            .Where(b => !branchId.HasValue || b.BranchId == branchId.Value)
            .Sum(b => b.QuantityOnHand);

        ViewBag.Price = price;
        ViewBag.Stock = stock;
        ViewBag.AvailableBranches = availableBranches;
        ViewBag.BranchId = branchId;
        return View(medicine);
    }
}
