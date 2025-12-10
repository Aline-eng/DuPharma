using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using DuPharma.Data;
using DuPharma.Models;
using System.Security.Claims;
using DuPharma.Services;
using DuPharma.Attributes;

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
        // Validate branch selection
        if (request.BranchId <= 0)
            return Json(new { success = false, message = "Please select a branch" });

        // Check if all items are available in selected branch
        var unavailableItems = new List<string>();
        foreach (var item in request.Items)
        {
            var stock = await _context.Batches
                .Where(b => b.MedicineId == item.MedicineId && b.BranchId == request.BranchId && b.QuantityOnHand >= item.Quantity)
                .SumAsync(b => b.QuantityOnHand);

            if (stock < item.Quantity)
            {
                var medicine = await _context.Medicines.FindAsync(item.MedicineId);
                unavailableItems.Add(medicine?.BrandName ?? "Unknown");
            }
        }

        if (unavailableItems.Any())
            return Json(new { success = false, message = $"Items not available in selected branch: {string.Join(", ", unavailableItems)}" });

        // Create or find customer
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Phone == request.CustomerPhone);
        
        if (customer == null)
        {
            customer = new Customer
            {
                FullName = request.CustomerName,
                Phone = request.CustomerPhone,
                Address = request.DeliveryAddress,
                NationalId = request.CustomerPhone
            };
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();
        }

        var order = new Order
        {
            OrderNumber = $"ORD{DateTime.Now:yyyyMMddHHmmss}",
            CustomerId = customer.CustomerId,
            CustomerName = request.CustomerName,
            CustomerEmail = request.CustomerEmail,
            CustomerPhone = request.CustomerPhone,
            DeliveryAddress = request.DeliveryAddress,
            OrderDate = DateTime.Now,
            BranchId = request.BranchId,
            Status = "Pending"
        };

        decimal total = 0;
        foreach (var item in request.Items)
        {
            var medicine = await _context.Medicines.FindAsync(item.MedicineId);
            if (medicine == null) continue;

            var price = await _context.Batches
                .Where(b => b.MedicineId == item.MedicineId && b.BranchId == request.BranchId && b.QuantityOnHand > 0)
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

    [RequirePermission(Permissions.ViewOrders)]
    public async Task<IActionResult> Index()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await _context.Users.FindAsync(userId);
        var permissionService = HttpContext.RequestServices.GetRequiredService<IPermissionService>();
        var hasAllBranchAccess = await User.HasPermissionAsync(permissionService, Permissions.ViewAllBranchOrders);

        var query = _context.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Medicine)
            .Include(o => o.Branch)
            .AsQueryable();

        // Filter by branch if no all-branch access
        if (!hasAllBranchAccess && user?.BranchId != null)
        {
            query = query.Where(o => o.BranchId == user.BranchId);
        }

        var orders = await query.OrderByDescending(o => o.OrderDate).ToListAsync();
        return View(orders);
    }

    [RequirePermission(Permissions.ApproveOrders)]
    [HttpPost]
    public async Task<IActionResult> UpdateStatus(int id, string status, string paymentMethod = "Cash")
    {
        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.OrderId == id);
        
        if (order == null) return NotFound();

        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        
        order.Status = status;
        if (status == "Approved")
            order.ApprovedByUserId = userId;

        // Automatically create sale when order is completed
        if (status == "Completed")
        {
            try
            {
                var dispenseService = HttpContext.RequestServices.GetRequiredService<DispenseService>();
                
                var orderDto = new OrderDto
                {
                    CustomerId = order.CustomerId,
                    PaymentMethod = paymentMethod,
                    Items = order.OrderItems.Select(oi => new OrderItemDto
                    {
                        MedicineId = oi.MedicineId,
                        Quantity = oi.Quantity
                    }).ToList()
                };

                var sale = await dispenseService.DispenseAsync(orderDto, userId);
                order.Notes = $"Sale created: {sale.InvoiceNumber}";
                
                TempData["Success"] = $"Order completed and sale {sale.InvoiceNumber} created successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Failed to create sale: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

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
    public int BranchId { get; set; }
    public List<OrderItemRequest> Items { get; set; } = new();
}

public class OrderItemRequest
{
    public int MedicineId { get; set; }
    public int Quantity { get; set; }
    public string? PrescriptionImageUrl { get; set; }
}
