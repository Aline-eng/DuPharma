using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using DuPharma.Data;
using DuPharma.Models;
using DuPharma.Services;
using System.Security.Claims;

namespace DuPharma.Controllers;

[Authorize]
public class SalesController : Controller
{
    private readonly AppDbContext _context;
    private readonly DispenseService _dispenseService;

    public SalesController(AppDbContext context, DispenseService dispenseService)
    {
        _context = context;
        _dispenseService = dispenseService;
    }

    public async Task<IActionResult> Index()
    {
        var sales = await _context.Sales
            .Include(s => s.Customer)
            .Include(s => s.SoldByUser)
            .OrderByDescending(s => s.SaleDate)
            .Take(50)
            .ToListAsync();

        return View(sales);
    }

    public async Task<IActionResult> Create()
    {
        var viewModel = new CreateSaleViewModel
        {
            Customers = await _context.Customers.ToListAsync(),
            Medicines = await _context.Medicines
                .Where(m => m.Batches.Any(b => b.QuantityOnHand > 0 && b.ExpiryDate > DateTime.Now))
                .ToListAsync()
        };

        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateSaleViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.Customers = await _context.Customers.ToListAsync();
            model.Medicines = await _context.Medicines
                .Where(m => m.Batches.Any(b => b.QuantityOnHand > 0 && b.ExpiryDate > DateTime.Now))
                .ToListAsync();
            return View(model);
        }

        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var order = new OrderDto
            {
                CustomerId = model.CustomerId,
                PaymentMethod = model.PaymentMethod,
                Items = model.Items.Where(i => i.Quantity > 0).ToList()
            };

            var sale = await _dispenseService.DispenseAsync(order, userId);
            return RedirectToAction(nameof(Receipt), new { id = sale.SaleId });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            model.Customers = await _context.Customers.ToListAsync();
            model.Medicines = await _context.Medicines
                .Where(m => m.Batches.Any(b => b.QuantityOnHand > 0 && b.ExpiryDate > DateTime.Now))
                .ToListAsync();
            return View(model);
        }
    }

    public async Task<IActionResult> Receipt(int id)
    {
        var sale = await _context.Sales
            .Include(s => s.Customer)
            .Include(s => s.SoldByUser)
            .Include(s => s.SaleItems)
            .ThenInclude(si => si.Batch)
            .ThenInclude(b => b.Medicine)
            .FirstOrDefaultAsync(s => s.SaleId == id);

        if (sale == null) return NotFound();
        return View(sale);
    }

    public async Task<IActionResult> Details(int id)
    {
        var sale = await _context.Sales
            .Include(s => s.Customer)
            .Include(s => s.SoldByUser)
            .Include(s => s.SaleItems)
            .ThenInclude(si => si.Batch)
            .ThenInclude(b => b.Medicine)
            .FirstOrDefaultAsync(s => s.SaleId == id);

        if (sale == null) return NotFound();
        return View(sale);
    }
}

public class CreateSaleViewModel
{
    public int? CustomerId { get; set; }
    public string PaymentMethod { get; set; } = "Cash";
    public List<OrderItemDto> Items { get; set; } = new();
    public List<Customer> Customers { get; set; } = new();
    public List<Medicine> Medicines { get; set; } = new();
}