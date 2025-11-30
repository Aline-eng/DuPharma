using Microsoft.EntityFrameworkCore;
using DuPharma.Models;

namespace DuPharma.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Branch> Branches { get; set; }
    public DbSet<Supplier> Suppliers { get; set; }
    public DbSet<Medicine> Medicines { get; set; }
    public DbSet<Batch> Batches { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Sale> Sales { get; set; }
    public DbSet<SaleItem> SaleItems { get; set; }
    public DbSet<Prescription> Prescriptions { get; set; }
    public DbSet<PrescriptionItem> PrescriptionItems { get; set; }
    public DbSet<StockMovement> StockMovements { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Indexes for performance
        builder.Entity<Batch>()
            .HasIndex(b => b.ExpiryDate)
            .HasDatabaseName("IX_Batch_ExpiryDate");

        builder.Entity<Batch>()
            .HasIndex(b => b.MedicineId)
            .HasDatabaseName("IX_Batch_MedicineId");

        builder.Entity<Sale>()
            .HasIndex(s => s.SaleDate)
            .HasDatabaseName("IX_Sale_SaleDate");

        builder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique()
            .HasDatabaseName("IX_Users_Email");

        // Foreign key relationships
        builder.Entity<User>()
            .HasOne(u => u.Branch)
            .WithMany(b => b.Users)
            .HasForeignKey(u => u.BranchId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<Batch>()
            .HasOne(b => b.Medicine)
            .WithMany(m => m.Batches)
            .HasForeignKey(b => b.MedicineId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Batch>()
            .HasOne(b => b.Supplier)
            .WithMany(s => s.Batches)
            .HasForeignKey(b => b.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Sale>()
            .HasOne(s => s.SoldByUser)
            .WithMany(u => u.Sales)
            .HasForeignKey(s => s.SoldByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Sale>()
            .HasOne(s => s.Customer)
            .WithMany(c => c.Sales)
            .HasForeignKey(s => s.CustomerId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<SaleItem>()
            .HasOne(si => si.Sale)
            .WithMany(s => s.SaleItems)
            .HasForeignKey(si => si.SaleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<SaleItem>()
            .HasOne(si => si.Batch)
            .WithMany(b => b.SaleItems)
            .HasForeignKey(si => si.BatchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Prescription>()
            .HasOne(p => p.Customer)
            .WithMany(c => c.Prescriptions)
            .HasForeignKey(p => p.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Prescription>()
            .HasOne(p => p.CreatedByUser)
            .WithMany(u => u.Prescriptions)
            .HasForeignKey(p => p.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<PrescriptionItem>()
            .HasOne(pi => pi.Prescription)
            .WithMany(p => p.PrescriptionItems)
            .HasForeignKey(pi => pi.PrescriptionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<PrescriptionItem>()
            .HasOne(pi => pi.Medicine)
            .WithMany(m => m.PrescriptionItems)
            .HasForeignKey(pi => pi.MedicineId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<StockMovement>()
            .HasOne(sm => sm.Batch)
            .WithMany(b => b.StockMovements)
            .HasForeignKey(sm => sm.BatchId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<StockMovement>()
            .HasOne(sm => sm.PerformedByUser)
            .WithMany(u => u.StockMovements)
            .HasForeignKey(sm => sm.PerformedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<AuditLog>()
            .HasOne(al => al.User)
            .WithMany(u => u.AuditLogs)
            .HasForeignKey(al => al.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}