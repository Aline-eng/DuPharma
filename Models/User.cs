using System.ComponentModel.DataAnnotations;

namespace DuPharma.Models;

public class User
{
    public int UserId { get; set; }
    [Required, MaxLength(100)]
    public string FullName { get; set; } = string.Empty;
    [Required, MaxLength(100)]
    public string Email { get; set; } = string.Empty;
    [Required, MaxLength(255)]
    public string PasswordHash { get; set; } = string.Empty;
    [MaxLength(20)]
    public string Phone { get; set; } = string.Empty;
    public int? BranchId { get; set; }
    public int Role { get; set; } = 3; // 1=Admin, 2=Manager, 3=Pharmacist
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    
    public Branch? Branch { get; set; }
    public ICollection<Sale> Sales { get; set; } = new List<Sale>();
    public ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
    public ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();
    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
}