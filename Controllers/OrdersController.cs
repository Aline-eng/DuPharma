using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using DuPharma.Data;
using DuPharma.Models;
using System.Security.Claims;

namespace DuPharma.Controllers;

[AllowAnonymous]
public class OrdersController : Controller
{
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _env;

    public OrdersController(AppDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] OrderRequest request)
    {
        var order = new Order
        {
            OrderNumber = $"ORD{DateTime.Now:yyyyMMddHHmmss}",
            CustomerName = request.CustomerName,
            CustomerEmail = request.CustomerEmail,
            CustomerPhone = request.CustomerPhone,
            DeliveryAddress = request.DeliveryAddress,
            OrderDate = DateTime.Now,
            BranchId = 1,
            Status = "Pending"
        };

        decimal total = 0;
        foreach (var item in request.Items)
        {
            var medicine = await _context.Medicines.FindAsync(item.MedicineId);
            if (medicine == null) continue;

            var price = await _context.Batches
                .Where(b => b.MedicineId == item.MedicineId && b.QuantityOnHand > 0)
                .MinAsync(b => (decimal?)b.SellingPrice) ?? 0;

            var orderItem = new OrderItem
            {
                MedicineId = item.MedicineId,
                Quantity = item.Quantity,
                UnitPrice = price,
                SubTotal = price * item.Quantity,
                PrescriptionImageUrl = item.PrescriptionImageUrl ?? ""
            };

            order.OrderItems.Add(orderItem);
            total += orderItem.SubTotal;
        }

        order.TotalAmount = total;
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        return Json(new { success = true, orderNumber = order.OrderNumber });
    }

    [HttpPost]
    public async Task<IActionResult> UploadPrescription(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return Json(new { success = false });

        var uploadsFolder = Path.Combine(_env.WebRootPath, "prescriptions");
        Directory.CreateDirectory(uploadsFolder);

        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var filePath = Path.Combine(uploadsFolder, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
            await file.CopyToAsync(stream);

        return Json(new { success = true, url = $"/prescriptions/{fileName}" });
    }

    [Authorize(Roles = "Admin,Manager,Pharmacist")]
    public async Task<IActionResult> Index()
    {
        var orders = await _context.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Medicine)
            .Include(o => o.Branch)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();

        return View(orders);
    }

    [Authorize(Roles = "Admin,Manager,Pharmacist")]
    [HttpPost]
    public async Task<IActionResult> UpdateStatus(int id, string status)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order == null) return NotFound();

        order.Status = status;
        if (status == "Approved")
            order.ApprovedByUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}

public class OrderRequest
{
    public string CustomerName { get; set; } = "";
    public string CustomerEmail { get; set; } = "";
    public string CustomerPhone { get; set; } = "";
    public string DeliveryAddress { get; set; } = "";
    public List<OrderItemRequest> Items { get; set; } = new();
}

public class OrderItemRequest
{
    public int MedicineId { get; set; }
    public int Quantity { get; set; }
    public string? PrescriptionImageUrl { get; set; }
}
