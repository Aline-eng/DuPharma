using DuPharma.Data;
using DuPharma.Models;
using Microsoft.EntityFrameworkCore;

namespace DuPharma.Services;

public class OrderDto
{
    public int? CustomerId { get; set; }
    public string PaymentMethod { get; set; } = "Cash";
    public List<OrderItemDto> Items { get; set; } = new();
}

public class OrderItemDto
{
    public int MedicineId { get; set; }
    public int Quantity { get; set; }
}

public class DispenseService
{
    private readonly AppDbContext _context;

    public DispenseService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Sale> DispenseAsync(OrderDto order, int userId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        
        try
        {
            // Validate stock availability
            var stockValidation = await ValidateStockAsync(order.Items);
            if (!stockValidation.IsValid)
                throw new InvalidOperationException($"Insufficient stock: {stockValidation.ErrorMessage}");

            // Generate invoice number
            var invoiceNumber = await GenerateInvoiceNumberAsync();

            // Create sale
            var sale = new Sale
            {
                InvoiceNumber = invoiceNumber,
                SoldByUserId = userId,
                CustomerId = order.CustomerId,
                SaleDate = DateTime.Now,
                PaymentMethod = order.PaymentMethod,
                TotalAmount = 0
            };

            _context.Sales.Add(sale);
            await _context.SaveChangesAsync();

            decimal totalAmount = 0;

            // Process each item using FEFO
            foreach (var item in order.Items)
            {
                var allocations = await AllocateStockFEFO(item.MedicineId, item.Quantity);
                
                foreach (var allocation in allocations)
                {
                    // Create sale item
                    var saleItem = new SaleItem
                    {
                        SaleId = sale.SaleId,
                        BatchId = allocation.BatchId,
                        Quantity = allocation.Quantity,
                        UnitPrice = allocation.UnitPrice,
                        SubTotal = allocation.Quantity * allocation.UnitPrice
                    };

                    _context.SaleItems.Add(saleItem);
                    totalAmount += saleItem.SubTotal;

                    // Update batch quantity
                    var batch = await _context.Batches.FindAsync(allocation.BatchId);
                    if (batch != null)
                    {
                        batch.QuantityOnHand -= allocation.Quantity;
                    }

                    // Create stock movement
                    var stockMovement = new StockMovement
                    {
                        BatchId = allocation.BatchId,
                        MovementType = "OUT",
                        Quantity = -allocation.Quantity,
                        PerformedByUserId = userId,
                        PerformedAt = DateTime.Now,
                        Reference = $"Sale {invoiceNumber}"
                    };

                    _context.StockMovements.Add(stockMovement);
                }
            }

            // Update sale total
            sale.TotalAmount = totalAmount;
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();
            return sale;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private async Task<(bool IsValid, string ErrorMessage)> ValidateStockAsync(List<OrderItemDto> items)
    {
        foreach (var item in items)
        {
            var totalStock = await _context.Batches
                .Where(b => b.MedicineId == item.MedicineId && b.QuantityOnHand > 0 && b.ExpiryDate > DateTime.Now)
                .SumAsync(b => b.QuantityOnHand);

            if (totalStock < item.Quantity)
            {
                var medicine = await _context.Medicines.FindAsync(item.MedicineId);
                return (false, $"{medicine?.GenericName}: Required {item.Quantity}, Available {totalStock}");
            }
        }
        return (true, string.Empty);
    }

    private async Task<List<StockAllocation>> AllocateStockFEFO(int medicineId, int requiredQuantity)
    {
        var batches = await _context.Batches
            .Where(b => b.MedicineId == medicineId && b.QuantityOnHand > 0 && b.ExpiryDate > DateTime.Now)
            .OrderBy(b => b.ExpiryDate)
            .ToListAsync();

        var allocations = new List<StockAllocation>();
        int remainingQuantity = requiredQuantity;

        foreach (var batch in batches)
        {
            if (remainingQuantity <= 0) break;

            int allocatedQuantity = Math.Min(remainingQuantity, batch.QuantityOnHand);
            
            allocations.Add(new StockAllocation
            {
                BatchId = batch.BatchId,
                Quantity = allocatedQuantity,
                UnitPrice = batch.SellingPrice
            });

            remainingQuantity -= allocatedQuantity;
        }

        if (remainingQuantity > 0)
            throw new InvalidOperationException($"Insufficient stock for medicine ID {medicineId}");

        return allocations;
    }

    private async Task<string> GenerateInvoiceNumberAsync()
    {
        var today = DateTime.Today;
        var count = await _context.Sales
            .Where(s => s.SaleDate.Date == today)
            .CountAsync();
        
        return $"INV{today:yyyyMMdd}{(count + 1):D4}";
    }

    private class StockAllocation
    {
        public int BatchId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}