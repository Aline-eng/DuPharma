using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DuPharma.Models;

public class Branch
{
    public int BranchId { get; set; }
    [Required, MaxLength(100)]
    public string BranchName { get; set; } = string.Empty;
    [MaxLength(200)]
    public string Location { get; set; } = string.Empty;
    public ICollection<User> Users { get; set; } = new List<User>();
}

public class Supplier
{
    public int SupplierId { get; set; }
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    [MaxLength(100)]
    public string ContactPerson { get; set; } = string.Empty;
    [MaxLength(20)]
    public string Phone { get; set; } = string.Empty;
    [MaxLength(200)]
    public string Address { get; set; } = string.Empty;
    public ICollection<Batch> Batches { get; set; } = new List<Batch>();
}

public class Medicine
{
    public int MedicineId { get; set; }
    [Required, MaxLength(100)]
    public string GenericName { get; set; } = string.Empty;
    [MaxLength(100)]
    public string BrandName { get; set; } = string.Empty;
    [MaxLength(50)]
    public string Strength { get; set; } = string.Empty;
    [MaxLength(50)]
    public string Form { get; set; } = string.Empty;
    [MaxLength(20)]
    public string Unit { get; set; } = string.Empty;
    public int ReorderLevel { get; set; }
    public ICollection<Batch> Batches { get; set; } = new List<Batch>();
    public ICollection<PrescriptionItem> PrescriptionItems { get; set; } = new List<PrescriptionItem>();
}

public class Batch
{
    public int BatchId { get; set; }
    public int MedicineId { get; set; }
    [Required, MaxLength(50)]
    public string BatchNumber { get; set; } = string.Empty;
    public DateTime ExpiryDate { get; set; }
    public int QuantityOnHand { get; set; }
    [Column(TypeName = "decimal(10,2)")]
    public decimal PurchasePrice { get; set; }
    [Column(TypeName = "decimal(10,2)")]
    public decimal SellingPrice { get; set; }
    public int SupplierId { get; set; }
    public DateTime ReceivedDate { get; set; }
    
    public Medicine Medicine { get; set; } = null!;
    public Supplier Supplier { get; set; } = null!;
    public ICollection<SaleItem> SaleItems { get; set; } = new List<SaleItem>();
    public ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();
}

public class Customer
{
    public int CustomerId { get; set; }
    [Required, MaxLength(100)]
    public string FullName { get; set; } = string.Empty;
    [MaxLength(20)]
    public string Phone { get; set; } = string.Empty;
    [MaxLength(200)]
    public string Address { get; set; } = string.Empty;
    [MaxLength(20)]
    public string NationalId { get; set; } = string.Empty;
    public ICollection<Sale> Sales { get; set; } = new List<Sale>();
    public ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
}

public class Sale
{
    public int SaleId { get; set; }
    [Required, MaxLength(20)]
    public string InvoiceNumber { get; set; } = string.Empty;
    public int SoldByUserId { get; set; }
    public int? CustomerId { get; set; }
    public DateTime SaleDate { get; set; }
    [Column(TypeName = "decimal(10,2)")]
    public decimal TotalAmount { get; set; }
    [MaxLength(20)]
    public string PaymentMethod { get; set; } = "Cash";
    
    public User SoldByUser { get; set; } = null!;
    public Customer? Customer { get; set; }
    public ICollection<SaleItem> SaleItems { get; set; } = new List<SaleItem>();
}

public class SaleItem
{
    public int SaleItemId { get; set; }
    public int SaleId { get; set; }
    public int BatchId { get; set; }
    public int Quantity { get; set; }
    [Column(TypeName = "decimal(10,2)")]
    public decimal UnitPrice { get; set; }
    [Column(TypeName = "decimal(10,2)")]
    public decimal SubTotal { get; set; }
    
    public Sale Sale { get; set; } = null!;
    public Batch Batch { get; set; } = null!;
}

public class Prescription
{
    public int PrescriptionId { get; set; }
    [Required, MaxLength(20)]
    public string PrescriptionNo { get; set; } = string.Empty;
    [MaxLength(100)]
    public string DoctorName { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public int CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; }
    [MaxLength(500)]
    public string Notes { get; set; } = string.Empty;
    
    public Customer Customer { get; set; } = null!;
    public User CreatedByUser { get; set; } = null!;
    public ICollection<PrescriptionItem> PrescriptionItems { get; set; } = new List<PrescriptionItem>();
}

public class PrescriptionItem
{
    public int PrescriptionItemId { get; set; }
    public int PrescriptionId { get; set; }
    public int MedicineId { get; set; }
    [MaxLength(100)]
    public string Dosage { get; set; } = string.Empty;
    public int Quantity { get; set; }
    [MaxLength(100)]
    public string Frequency { get; set; } = string.Empty;
    [MaxLength(100)]
    public string Duration { get; set; } = string.Empty;
    
    public Prescription Prescription { get; set; } = null!;
    public Medicine Medicine { get; set; } = null!;
}

public class StockMovement
{
    public int StockMovementId { get; set; }
    public int BatchId { get; set; }
    [MaxLength(20)]
    public string MovementType { get; set; } = string.Empty; // IN, OUT, ADJUSTMENT
    public int Quantity { get; set; }
    public int PerformedByUserId { get; set; }
    public DateTime PerformedAt { get; set; }
    [MaxLength(100)]
    public string Reference { get; set; } = string.Empty;
    
    public Batch Batch { get; set; } = null!;
    public User PerformedByUser { get; set; } = null!;
}

public class AuditLog
{
    [Key]
    public int AuditId { get; set; }
    public int UserId { get; set; }
    [MaxLength(50)]
    public string Action { get; set; } = string.Empty;
    [MaxLength(50)]
    public string Entity { get; set; } = string.Empty;
    public int EntityId { get; set; }
    [MaxLength(500)]
    public string Detail { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    
    public User User { get; set; } = null!;
}